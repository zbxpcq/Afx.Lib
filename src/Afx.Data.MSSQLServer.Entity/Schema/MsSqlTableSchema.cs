using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using Afx.Data;
using Afx.Data.Entity.Schema;

namespace Afx.Data.MSSQLServer.Entity.Schema
{
    /// <summary>
    /// 表结构接口
    /// </summary>
    public sealed class MsSqlTableSchema : TableSchema
    {
        private Database db;
        private string database;

        /// <summary>
        /// 执行sql logs
        /// </summary>
        public override Action<string> Log
        {
            get
            {
                return this.db != null && !this.db.IsDisposed ? this.db.Log : null;
            }
            set
            {
                if (this.db != null && !this.db.IsDisposed) this.db.Log = value;
            }
        } 
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionString"></param>
        public MsSqlTableSchema(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
            var stringBuilder = new SqlConnectionStringBuilder(connectionString);
            this.database = stringBuilder.InitialCatalog;
            this.db = new MsSqlDatabase(connectionString);
        }
        /// <summary>
        /// 获取数据库所有表名
        /// </summary>
        /// <returns>数据库所有表名</returns>
        public override List<string> GetTables()
        {
            List<string> list = new List<string>();
            this.db.ClearParameters();
            this.db.CommandText = @"SELECT TABLE_NAME FROM information_schema.tables WHERE TABLE_CATALOG=@database AND TABLE_TYPE='BASE TABLE'";
            this.db.AddParameter("@database", this.database);
            using (DataTable dt = new DataTable())
            {
                db.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    string s = row[0].ToString();
                    if (!string.IsNullOrEmpty(s))
                        list.Add(s);
                }
            }

            return list;
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="indexs">索引列信息</param>
        public override void AddIndex(string table, List<IndexModel> indexs)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (indexs == null) throw new ArgumentNullException("indexs");
            var list = indexs.FindAll(q => !string.IsNullOrEmpty(q.Name) && !string.IsNullOrEmpty(q.ColumnName));
            if (list.Count > 0)
            {
                var group = list.GroupBy(q => q.Name, StringComparer.OrdinalIgnoreCase);
                foreach (var item in group)
                {
                    string indexName = item.Key;
                    bool isUnique = item.Count(q => q.IsUnique) > 0;
                    List<string> columnList = new List<string>(item.Count());
                    foreach (var m in item)
                    {
                        columnList.Add(m.ColumnName);
                    }
                    this.AddIndex(table, indexName, isUnique, columnList);
                }
            }
        }
        /// <summary>
        /// 创建数据库表
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="columns">列信息</param>
        /// <returns>是否成功</returns>
        public override bool CreateTable(string table, List<ColumnInfoModel> columns)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (columns == null) throw new ArgumentNullException("columns");
            if (columns.Count == 0)  return false;
            int count = 0;
            StringBuilder createTableSql = new StringBuilder();
            StringBuilder createKeySql = new StringBuilder();
            List<ColumnInfoModel> keyColumns = columns.Where(q => q.IsKey).ToList();
            List<IndexModel> indexs = new List<IndexModel>();
            createTableSql.AppendFormat("CREATE TABLE [{0}](", table);
            foreach (var column in columns)
            {
                createTableSql.AppendFormat("[{0}] {1} {2} NULL", column.Name, column.DataType, column.IsNullable ? "" : "NOT");
                if (column.IsAutoIncrement) createTableSql.Append(" IDENTITY (1, 1)");
                createTableSql.Append(",");

                if (column.Indexs != null && column.Indexs.Count > 0) indexs.AddRange(column.Indexs);
            }
            createTableSql.Remove(createTableSql.Length - 1, 1);
            createTableSql.Append(")");

            if (keyColumns.Count > 0)
            {
                createTableSql.Append(" ON [PRIMARY]");

                createKeySql.AppendFormat("ALTER TABLE [{0}] ADD CONSTRAINT PK_{0} PRIMARY KEY CLUSTERED(", table);
                foreach (var column in keyColumns)
                {
                    createKeySql.AppendFormat("[{0}],", column.Name);
                }
                createKeySql.Remove(createKeySql.Length - 1, 1);
                createKeySql.Append(") WITH(STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
            }

            this.db.ClearParameters();
            using (var tx = this.db.BeginTransaction())
            {
                this.db.CommandText = createTableSql.ToString();
                count = this.db.ExecuteNonQuery();
                createTableSql.Clear();
                if (createKeySql.Length > 0)
                {
                    this.db.CommandText = createKeySql.ToString();
                    this.db.ExecuteNonQuery();
                }

                if (indexs.Count > 0) this.AddIndex(table, indexs);

                tx.Commit();
            }
            
            return count > 0;
        }

        //public override bool DeleteTable(string table)
        //{
        //    this.db.ClearParameters();
        //    this.db.CommandText = string.Format("DROP TABLE [{0}]", table);
        //    int count = this.db.ExecuteNonQuery();

        //    return count > 0;
        //}
        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="column">列信息</param>
        /// <returns>是否成功</returns>
        public override bool AddColumn(string table, ColumnInfoModel column)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (column == null) throw new ArgumentNullException("column");
            this.db.ClearParameters();
            this.db.CommandText = string.Format(AddColumnSql, table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
            int count = this.db.ExecuteNonQuery();

            return count > 0;
        }
        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="indexName">索引名称</param>
        /// <param name="isUnique">是否唯一索引</param>
        /// <param name="columns">列名</param>
        /// <returns>是否成功</returns>
        public override bool AddIndex(string table, string indexName, bool isUnique, List<string> columns)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentNullException("indexName");
            if (columns == null) throw new ArgumentNullException("columns");
            if (columns.Count == 0)
                return false;

            StringBuilder strColumns = new StringBuilder();
            foreach (var s in columns)
                strColumns.AppendFormat("[{0}],", s);
            strColumns.Remove(strColumns.Length - 1, 1);

            this.db.ClearParameters();
            this.db.CommandText = string.Format(AddIndexSql, isUnique ? "UNIQUE" : "", indexName,
                table, strColumns.ToString());
            int count = this.db.ExecuteNonQuery();

            return count > 0;
        }
        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="index">索引列信息</param>
        /// <returns>是否成功</returns>
        public override bool AddIndex(string table, IndexModel index)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (index == null) throw new ArgumentNullException("index");
            int count = 0;
            if (!string.IsNullOrEmpty(index.Name) && !string.IsNullOrEmpty(index.ColumnName))
            {
                this.db.ClearParameters();
                this.db.CommandText = string.Format(AddIndexSql, index.IsUnique ? "UNIQUE" : "", index.Name, table, "[" + index.ColumnName + "]");
                count = this.db.ExecuteNonQuery();
            }

            return count > 0;
        }

        //public override bool DeleteColumn(string table, ColumnInfoModel column)
        //{
        //    this.db.ClearParameters();
        //    int count = 0;
        //    using (var tx = this.db.BeginTransaction())
        //    {
        //        if (!string.IsNullOrEmpty(column.IndexName))
        //        {
        //            this.db.CommandText = string.Format(DeleteIndexSql, column.IndexName, table);
        //            try { this.db.ExecuteNonQuery(); }
        //            catch { }
        //        }
        //        this.db.CommandText = string.Format(DeleteColumnSql, table, column.Name);
        //        count = this.db.ExecuteNonQuery();

        //        tx.Commit();
        //    }

        //    return count > 0;
        //}
        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="index">索引名称</param>
        /// <returns>是否成功</returns>
        public override bool DeleteIndex(string table, string index)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (string.IsNullOrEmpty(index)) throw new ArgumentNullException("index");
            this.db.ClearParameters();
            int count = 0;
            this.db.CommandText = string.Format(DeleteIndexSql, index, table);
            try { count = this.db.ExecuteNonQuery(); }
            catch { }

            return count > 0;
        }

        //public override bool AlterColumn(string table, ColumnInfoModel column)
        //{
        //    this.db.ClearParameters();
        //    this.db.CommandText = string.Format(AlterColumnSql, table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
        //    int count = this.db.ExecuteNonQuery();

        //    return count > 0;
        //}

        //public override bool EqualColumn(ColumnInfoModel column1, ColumnInfoModel column2)
        //{
        //    if (column1 != null && column2 != null)
        //    {
        //        int i = column1.DataType.IndexOf('(');
        //        string s1 = i > 0 ? column1.DataType.Substring(0, i).Trim() : column1.DataType;
        //        i = column2.DataType.IndexOf('(');
        //        string s2 = i > 0 ? column2.DataType.Substring(0, i).Trim() : column2.DataType;
        //        if (!s1.Equals(s2, StringComparison.OrdinalIgnoreCase))
        //        {
        //            return false;
        //        }

        //        if ((s1.Equals("nchar", StringComparison.OrdinalIgnoreCase)
        //            || s1.Equals("nvarchar", StringComparison.OrdinalIgnoreCase))
        //            && column1.MaxLength != column2.MaxLength)
        //        {
        //            return false;
        //        }

        //        if (s1.Equals("decimal", StringComparison.OrdinalIgnoreCase)
        //            && column1.MaxLength != column2.MaxLength
        //            && column1.MinLength != column2.MinLength)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}
        /// <summary>
        /// 获取表列信息
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        public override List<ColumnInfoModel> GetTableColumns(string table)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            List<ColumnInfoModel> list = new List<ColumnInfoModel>();
            this.db.ClearParameters();
            this.db.CommandText = SelectColumnSql;
            this.db.AddParameter("@table", table);
            using (DataTable dt = new DataTable())
            {
                db.Fill(dt);
                this.db.ClearParameters();
                this.db.CommandText = SelectTableIndexSql;
                this.db.AddParameter("@table", table);
                using (var index_dt = new DataTable())
                {
                    db.Fill(index_dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        ColumnInfoModel m = new ColumnInfoModel();
                        list.Add(m);

                        int len = 0;
                        if (int.TryParse(row["MaxLength"].ToString(), out len))
                            m.MaxLength = len;
                        if (int.TryParse(row["MinLength"].ToString(), out len))
                            m.MinLength = len;
                        m.Name = row["Name"].ToString();
                        m.DataType = row["DataType"].ToString();
                        m.IsAutoIncrement = Convert.ToBoolean(row["IsAutoIncrement"]);
                        m.IsNullable = Convert.ToBoolean(row["IsNullable"]);
                        m.Order = Convert.ToInt32(row["Order"]);
                        var index_row = index_dt.Select("IsKey = 1 and Order = " + m.Order);
                        m.IsKey = index_row != null && index_row.Length > 0;
                        index_row = index_dt.Select("IsKey = 0 and Order = " + m.Order);
                        if (index_row != null && index_row.Length > 0)
                        {
                            m.Indexs = new List<IndexModel>(index_row.Length);
                            foreach(var r in index_row)
                            {
                                IndexModel index = new IndexModel();
                                m.Indexs.Add(index);
                                index.ColumnName = m.Name;
                                index.Name = row["IndexName"].ToString();
                                index.IsUnique = Convert.ToBoolean(row["IsUnique"]);
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 获取列数据库类型
        /// </summary>
        /// <param name="propertyType">model 属性类型</param>
        /// <param name="maxLength">类型最大长度</param>
        /// <param name="minLength">类型最小长度</param>
        /// <returns>列数据库类型</returns>
        public override string GetColumnType(Type propertyType, int maxLength, int minLength)
        {
            if (propertyType == null) throw new ArgumentNullException("propertyType");
            string type = null;
            if (propertyType.IsEnum)
            {
                propertyType = typeof(int);
            }
            else if (typeof(string) == propertyType)
            {
                if (maxLength > 4000)
                    type = "text";
                if (maxLength <= 0) maxLength = 50;
            }
            else if (typeof(decimal) == propertyType || typeof(decimal?) == propertyType)
            {
                if (minLength == 0 && maxLength == 0)
                {
                    maxLength = 18;
                    minLength = 7;
                }
                if (maxLength <= 0) maxLength = 18;
                if (minLength >= maxLength) minLength = maxLength - 1;
            }

            if (null != type || dic.TryGetValue(propertyType, out type))
            {
                type = string.Format(type, maxLength, minLength);
            }

            return type;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this.db != null) db.Dispose();
            this.db = null;
            base.Dispose();
        }

        private static Dictionary<Type, string> dic = new Dictionary<Type, string>();
        static MsSqlTableSchema()
        {
            dic.Add(typeof(int), "int");
            dic.Add(typeof(int?), "int");

            dic.Add(typeof(IntPtr), "int");
            dic.Add(typeof(IntPtr?), "int");

            dic.Add(typeof(long), "bigint");
            dic.Add(typeof(long?), "bigint");

            dic.Add(typeof(bool), "bit");
            dic.Add(typeof(bool?), "bit");

            dic.Add(typeof(short), "smallint");
            dic.Add(typeof(short?), "smallint");

            dic.Add(typeof(char), "nchar(1)");
            dic.Add(typeof(char?), "nchar(1)");

            dic.Add(typeof(char[]), "nchar({0})");
            
            dic.Add(typeof(byte), "tinyint");
            dic.Add(typeof(byte?), "tinyint");

            dic.Add(typeof(decimal), "decimal({0},{1})");
            dic.Add(typeof(decimal?), "decimal({0},{1})");

            dic.Add(typeof(float), "real");
            dic.Add(typeof(float?), "real");

            dic.Add(typeof(double), "float");
            dic.Add(typeof(double?), "float");

            dic.Add(typeof(DateTime), "datetime");
            dic.Add(typeof(DateTime?), "datetime");

            dic.Add(typeof(DateTimeOffset), "datetimeoffset(7)");
            dic.Add(typeof(DateTimeOffset?), "datetimeoffset(7)");

            dic.Add(typeof(TimeSpan),  "timestamp");
            dic.Add(typeof(TimeSpan?), "timestamp");
            
            dic.Add(typeof(Guid), "Uniqueidentifier");
            dic.Add(typeof(Guid?), "Uniqueidentifier");

            dic.Add(typeof(string), "nvarchar({0})");

            dic.Add(typeof(byte[]), "image");
        }

        private const string SelectColumnSql = @"SELECT col.column_id [Order], col.name [Name], t.name [DataType],
(CASE WHEN t.name='nvarchar' OR t.name='nchar' THEN col.max_length/2
ELSE (CASE WHEN t.name='decimal' OR t.name = 'numeric' THEN col.[precision] ELSE col.max_length END) END) [MaxLength],
ISNULL(col.scale, 0) [MinLength],
col.is_nullable IsNullable,
col.is_identity IsAutoIncrement
FROM sys.columns col
INNER JOIN sys.objects o ON col.object_id=o.object_id AND o.type='U'
INNER JOIN sys.types t ON col.system_type_id=t.system_type_id AND col.user_type_id=t.user_type_id
WHERE o.name=@table";
        private const string SelectTableIndexSql = @"SELECT col.column_id [Order],
ISNULL(idx.is_primary_key, 0) IsKey,
ISNULL(idx.name,'') IndexName, ISNULL(idx.is_unique,0) IsUnique
FROM sys.columns col
INNER JOIN sys.objects o ON col.[object_id] = o.[object_id] AND o.[type] = 'U'
INNER JOIN sys.index_columns idx_col ON col.[object_id] = idx_col.[object_id] AND col.column_id = idx_col.column_id
INNER JOIN sys.indexes idx ON col.[object_id]=idx.[object_id] AND idx_col.index_id=idx.index_id
WHERE o.name=@table";
        private const string AddColumnSql = @"ALTER TABLE [{0}] ADD [{1}] {2} {3} NULL";
        private const string AddIndexSql = @"CREATE {0} NONCLUSTERED INDEX [{1}] ON [{2}]	({3})
WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]";
        private const string DeleteColumnSql = @"ALTER TABLE [{0}] DROP COLUMN [{1}]";
        private const string DeleteIndexSql = @"DROP INDEX [{0}] ON [{1}]";
        private const string AlterColumnSql = @"ALTER TABLE [{0}] ALTER COLUMN [{1}] {2} {3} NULL";
    }
}

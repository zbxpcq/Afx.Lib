using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;
using Afx.Data;
using System.Data;
using Afx.Data.Entity.Schema;

namespace Afx.Data.MySql.Entity.Schema
{
    /// <summary>
    /// 表结构接口
    /// </summary>
    public sealed class MySqlTableSchema : TableSchema
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
        /// <param name="connectionString">数据库链接字符串</param>
        public MySqlTableSchema(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
            var stringBuilder = new MySqlConnectionStringBuilder(connectionString);
            this.database = stringBuilder.Database;
            this.db = new MySqlDatabase(connectionString);
        }
        /// <summary>
        /// 获取数据库所有表名
        /// </summary>
        /// <returns>数据库所有表名</returns>
        public override List<TableInfoModel> GetTables()
        {
            List<TableInfoModel> list = null;
            this.db.ClearParameters();
            this.db.CommandText = "SELECT table_name,table_comment FROM information_schema.tables tb WHERE tb.table_schema=?database  AND table_type = 'BASE TABLE';";
            this.db.AddParameter("?database", this.database);
            using (DataTable dt = new DataTable())
            {
                db.Fill(dt);
                list = new List<TableInfoModel>(dt.Rows.Count);
                foreach (DataRow row in dt.Rows)
                {
                    string s = row[0].ToString();
                    if (!string.IsNullOrEmpty(s))
                    {
                        list.Add(new TableInfoModel()
                        {
                            Name = s,
                            Comment = row[1].ToString()
                        });
                    }
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
            if (indexs == null) throw new ArgumentNullException("columns");
            var list = indexs.FindAll(q => !string.IsNullOrEmpty(q.Name) && !string.IsNullOrEmpty(q.ColumnName));
            if (list.Count > 0)
            {
                var group = list.GroupBy(q => q.Name, StringComparer.OrdinalIgnoreCase);
                foreach (var item in group)
                {
                    string indexName = item.Key;
                    bool isUnique = item.Count(q => q.IsUnique) > 0;
                    List<string> columnList = new List<string>();
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
        public override bool CreateTable(TableInfoModel table, List<ColumnInfoModel> columns)
        {
            if (table == null || string.IsNullOrEmpty(table.Name)) throw new ArgumentNullException("table");
            if (columns == null) throw new ArgumentNullException("columns");
            if (columns.Count == 0) return false;
            int count = 0;
            StringBuilder createTableSql = new StringBuilder();
            List<ColumnInfoModel> keyColumns = columns.Where(q => q.IsKey).ToList();
            List<IndexModel> indexs = new List<IndexModel>();
            createTableSql.AppendFormat("CREATE TABLE `{0}`(", table.Name);
            foreach (var column in columns)
            {
                createTableSql.AppendFormat("`{0}` {1} {2} NULL", column.Name, column.DataType, column.IsNullable ? "" : "NOT");
                if (column.IsAutoIncrement) createTableSql.Append(" AUTO_INCREMENT");
                if (!string.IsNullOrEmpty(column.Comment)) createTableSql.AppendFormat(" COMMENT '{0}'", column.Comment.Replace("'", ""));
                createTableSql.Append(",");

                if (column.Indexs != null && column.Indexs.Count > 0) indexs.AddRange(column.Indexs);
            }
            createTableSql.Remove(createTableSql.Length - 1, 1);

            if (keyColumns.Count > 0)
            {
                createTableSql.Append(", PRIMARY KEY (");

                foreach (var column in keyColumns)
                {
                    createTableSql.AppendFormat("`{0}`,", column.Name);
                }
                createTableSql.Remove(createTableSql.Length - 1, 1);
                createTableSql.Append(")");
            }

            createTableSql.Append(") ENGINE=INNODB CHARSET=utf8 COLLATE=utf8_general_ci");

            if(!string.IsNullOrEmpty(table.Comment)) createTableSql.AppendFormat(" COMMENT '{0}'", table.Comment.Replace("'", ""));
            createTableSql.Append(";");
            this.db.ClearParameters();
            using (var tx = this.db.BeginTransaction())
            {
                this.db.CommandText = createTableSql.ToString();
                count = this.db.ExecuteNonQuery();

                if (indexs.Count > 0) this.AddIndex(table.Name, indexs);

                tx.Commit();
            }

            return count > 0;
        }

        //public override bool DeleteTable(string table)
        //{
        //    this.db.ClearParameters();
        //    this.db.CommandText = string.Format("DROP TABLE `{0}`", table);
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
            this.db.CommandText = string.Format("ALTER TABLE `{0}` ADD COLUMN `{1}` {2} {3} NULL Comment '{4}';",
                table, column.Name, column.DataType, column.IsNullable ? "" : "NOT", (column.Comment ?? "").Replace("'", ""));
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
            {
                strColumns.AppendFormat("`{0}`,", s);
            }
            strColumns.Remove(strColumns.Length - 1, 1);

            this.db.ClearParameters();
            this.db.CommandText = string.Format("ALTER TABLE `{0}` ADD {1} INDEX `{2}` ({3});",
                table, isUnique ? "UNIQUE" : "", indexName, strColumns.ToString());
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
            int count = 0;
            if (!string.IsNullOrEmpty(index.Name) && !string.IsNullOrEmpty(index.ColumnName))
            {
                this.db.ClearParameters();
                this.db.CommandText = string.Format("ALTER TABLE `{0}` ADD {1} INDEX `{2}` (`{3}`);",
                    table, index.IsUnique ? "UNIQUE" : "", index.Name, index.ColumnName);
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
        //            this.db.CommandText = string.Format("DROP INDEX `{0}` ON `{1}`",
        //                column.IndexName, table);
        //            try { this.db.ExecuteNonQuery(); }
        //            catch { }
        //        }
        //        this.db.CommandText = string.Format("ALTER TABLE `{0}` DROP `{1}`", table, column.Name);
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
            this.db.ClearParameters();
            int count = 0;
            this.db.CommandText = string.Format("DROP INDEX `{0}` ON `{1}`", index, table);
            try { count = this.db.ExecuteNonQuery(); }
            catch { }

            return count > 0;
        }

        //public override bool AlterColumn(string table, ColumnInfoModel column)
        //{
        //    this.db.ClearParameters();
        //    this.db.CommandText = string.Format("ALTER TABLE `{0}` MODIFY COLUMN `{1}` {2} {3} NULL;", table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
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

        //        if ((s1.Equals("char", StringComparison.OrdinalIgnoreCase)
        //            || s1.Equals("varchar", StringComparison.OrdinalIgnoreCase))
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
            List<ColumnInfoModel> list = new List<ColumnInfoModel>();
            this.db.ClearParameters();
            this.db.CommandText = SelectColumnSql;
            this.db.AddParameter("?database", this.database);
            this.db.AddParameter("?table", table);
            using (DataTable dt = new DataTable())
            {
                db.Fill(dt);

                this.db.ClearParameters();
                this.db.CommandText = SelectTableIndexSql;
                this.db.AddParameter("?database", this.database);
                this.db.AddParameter("?table", table);
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
                        m.IsKey = Convert.ToBoolean(row["IsKey"]);
                        m.IsNonClustered = false;
                        m.IsNullable = Convert.ToBoolean(row["IsNullable"]);
                        m.Order = Convert.ToInt32(row["Order"]);
                        m.Comment = row["Comment"].ToString();
                        var index_row = index_dt.Select("Order = " + m.Order);
                        if (index_row != null && index_row.Length > 0)
                        {
                            m.Indexs = new List<IndexModel>(index_row.Length);
                            foreach (var r in index_row)
                            {
                                IndexModel index = new IndexModel();
                                m.Indexs.Add(index);
                                index.ColumnName = m.Name;
                                index.Name = r["IndexName"].ToString();
                                index.IsUnique = Convert.ToBoolean(r["IsUnique"]);
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
            string type = null;
            if (propertyType.IsEnum)
            {
                propertyType = typeof(int);
            }
            else if (typeof(string) == propertyType)
            {
                if (maxLength > 255 && maxLength < 65535)
                    type = "text";
                else if (maxLength > 65535)
                    type = "longtext";
                else if (maxLength <= 0)
                    maxLength = 50;
            }
            else if (typeof(decimal) == propertyType || typeof(decimal?) == propertyType)
            {
                if (minLength == 0 && maxLength == 0)
                {
                    maxLength = 18;
                    minLength = 7;
                }
                if (maxLength <= 0) maxLength = 38;
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
            if (this.db != null) this.db.Dispose();
            this.db = null;
            base.Dispose();
        }

        private static Dictionary<Type, string> dic = new Dictionary<Type, string>();
        static MySqlTableSchema()
        {
            dic.Add(typeof(int), "int");

            dic.Add(typeof(IntPtr), "int");

            dic.Add(typeof(long), "bigint");

            dic.Add(typeof(bool), "bool");

            dic.Add(typeof(short), "smallint");

            dic.Add(typeof(char), "char(1)");
            dic.Add(typeof(char[]), "char({0})");

            dic.Add(typeof(byte), "tinyint");

            dic.Add(typeof(decimal), "decimal({0},{1})");

            dic.Add(typeof(float), "float");

            dic.Add(typeof(double), "double");

            dic.Add(typeof(DateTime), "datetime");
            dic.Add(typeof(DateTimeOffset), "datetime");

            dic.Add(typeof(TimeSpan),  "timestamp");

            dic.Add(typeof(Guid), "varchar(40)");

            dic.Add(typeof(string), "varchar({0})");

            dic.Add(typeof(byte[]), "longblob");
        }

        private const string SelectColumnSql = @"
SELECT col.`ORDINAL_POSITION` `Order`, col.`COLUMN_NAME` `Name`, col.`DATA_TYPE` `DataType`,
IFNULL((CASE WHEN col.`DATA_TYPE` IN('decimal', 'double', 'float', 'numeric', 'real') THEN  col.`NUMERIC_PRECISION` ELSE col.`CHARACTER_MAXIMUM_LENGTH` END), 0) `MaxLength`,
IFNULL(col.`NUMERIC_SCALE`, 0) `MinLength`,
IF(col.`IS_NULLABLE`='YES', 1, 0) `IsNullable`,
IF(col.`COLUMN_KEY`='PRI', 1, 0) `IsKey`,
IF(col.`EXTRA`='auto_increment', 1,0) `IsAutoIncrement`,
col.`COLUMN_COMMENT` `Comment`
FROM information_schema.columns col
WHERE col.table_schema=?database AND col.table_name=?table;";

        private const string SelectTableIndexSql = @"
SELECT col.`ORDINAL_POSITION` `Order`, 
IFNULL(statis.`INDEX_NAME`, '') `IndexName`,
IF(statis.`NON_UNIQUE`=0, 1, 0) `IsUnique`
FROM information_schema.columns col
INNER JOIN information_schema.statistics statis ON col.`TABLE_SCHEMA`=statis.`TABLE_SCHEMA` AND col.`TABLE_NAME`=statis.`TABLE_NAME` AND col.`COLUMN_NAME`=statis.`COLUMN_NAME`
WHERE col.table_schema=?database AND col.table_name=?table AND statis.`INDEX_NAME` <> 'PRIMARY';";
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

#if NETCOREAPP || NETSTANDARD
using Microsoft.Data.Sqlite;
#else
using System.Data.SQLite;
#endif
using Afx.Data;
using Afx.Data.Entity.Schema;

namespace Afx.Data.SQLite.Entity.Schema
{
    /// <summary>
    /// 表结构接口
    /// </summary>
    public sealed class SQLiteTableSchema : TableSchema
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
        public SQLiteTableSchema(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
#if NETCOREAPP || NETSTANDARD
            var stringBuilder = new SqliteConnectionStringBuilder(connectionString);
#else
            var stringBuilder = new SQLiteConnectionStringBuilder(connectionString);
#endif
            this.database = System.IO.Path.GetFileNameWithoutExtension(stringBuilder.DataSource);
            this.db = new SQLiteDatabase(connectionString);
        }
        /// <summary>
        /// 获取数据库所有表名
        /// </summary>
        /// <returns>数据库所有表名</returns>
        public override List<string> GetTables()
        {
            List<string> list = new List<string>();
            this.db.ClearParameters();
            this.db.CommandText = @"SELECT [name] FROM [sqlite_master] WHERE [type]='table' AND [tbl_name]!='sqlite_sequence'";
            using (var dt = new DataTable())
            {
                this.db.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(row[0].ToString());
                }
            }

            return list;
        }
        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="columns">索引列信息</param>
        public override void AddIndex(string table, List<ColumnInfoModel> columns)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (columns == null) throw new ArgumentNullException("columns");
            var list = columns.FindAll(q => !q.IsKey && !string.IsNullOrEmpty(q.IndexName));
            if (list.Count > 0)
            {
                var group = list.GroupBy(q => q.IndexName, StringComparer.OrdinalIgnoreCase);
                foreach (var item in group)
                {
                    string indexName = item.Key;
                    bool isUnique = item.Count(q => q.IsUnique) > 0;
                    List<string> columnList = new List<string>();
                    foreach (var m in item)
                    {
                        columnList.Add(m.Name);
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
            if (string.IsNullOrEmpty(table) || columns == null && columns.Count == 0)
                return true;

            int count = 0;
            StringBuilder createTableSql = new StringBuilder();
            List<ColumnInfoModel> keyColumns = columns.Where(q=>q.IsKey).ToList();
            createTableSql.AppendFormat("CREATE TABLE [{0}](", table);
            for (int i = 0; i < columns.Count; i++ )
            {
                var column = columns[i];
                createTableSql.AppendFormat("[{0}] {1} {2} NULL", column.Name, column.DataType, column.IsNullable ? "" : "NOT");
                if (column.IsKey && keyColumns.Count == 1)
                {
                    createTableSql.Append(" PRIMARY KEY");
                    if (column.IsAutoIncrement) createTableSql.Append(" AUTOINCREMENT");
                }
                createTableSql.Append(",");
            }
            createTableSql.Remove(createTableSql.Length - 1, 1);

            if (keyColumns.Count > 1)
            {
                createTableSql.AppendFormat(", CONSTRAINT PK_{0} PRIMARY KEY (", table);

                foreach (var column in keyColumns)
                {
                    createTableSql.AppendFormat("[{0}],", column.Name);
                }
                createTableSql.Remove(createTableSql.Length - 1, 1);
                createTableSql.Append(")");
            }

            createTableSql.Append(")");

            this.db.ClearParameters();
            using (var tx = this.db.BeginTransaction())
            {
                this.db.CommandText = createTableSql.ToString();
               count = this.db.ExecuteNonQuery();

               this.AddIndex(table, columns);

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
            this.db.ClearParameters();
            this.db.CommandText = string.Format("ALTER TABLE [{0}] ADD COLUMN [{1}] {2} {3} NULL;",
                table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
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
            if (columns == null && columns.Count == 0)
                return false;

            StringBuilder strColumns = new StringBuilder();
            foreach (var s in columns)
                strColumns.AppendFormat("[{0}],", s);
            strColumns.Remove(strColumns.Length - 1, 1);

            this.db.ClearParameters();
            this.db.CommandText = string.Format("CREATE {0} INDEX [{1}] ON [{2}] ({3})",
                isUnique ? "UNIQUE" : "", indexName, table, strColumns.ToString());
            int count = this.db.ExecuteNonQuery();

            return count > 0;
        }
        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="column">索引列信息</param>
        /// <returns>是否成功</returns>
        public override bool AddIndex(string table, ColumnInfoModel column)
        {
            int count = 0;
            if (!column.IsKey && !string.IsNullOrEmpty(column.IndexName))
            {
                this.db.ClearParameters();
                this.db.CommandText = string.Format("CREATE {0} INDEX [{1}] ON [{2}] ([{3}])",
                    column.IsUnique ? "UNIQUE" : "", column.IndexName, table, column.Name);
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
        //            this.db.CommandText = string.Format("DROP INDEX [{0}] ON [{1}]",
        //                column.IndexName, table);
        //            try { this.db.ExecuteNonQuery(); }
        //            catch { }
        //        }
        //        this.db.CommandText = string.Format("ALTER TABLE [{0}] DROP [{1}]", table, column.Name);
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
            this.db.CommandText = string.Format("DROP INDEX [{0}] ON [{1}]", index, table);
            try { count = this.db.ExecuteNonQuery(); }
            catch { }

            return count > 0;
        }

        //public override bool AlterColumn(string table, ColumnInfoModel column)
        //{
        //    //this.db.ClearParameters();
        //    //this.db.CommandText = string.Format("ALTER TABLE [{0}] ALTER COLUMN [{1}] {2} {3} NULL", table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
        //    //int count = this.db.ExecuteNonQuery();

        //    return false;// count > 0;
        //}

        //public override bool EqualColumn(ColumnInfoModel column1, ColumnInfoModel column2)
        //{
        //    //if (column1 != null && column2 != null)
        //    //{
        //    //    int i = column1.DataType.IndexOf('(');
        //    //    string s1 = i > 0 ? column1.DataType.Substring(0, i).Trim() : column1.DataType;
        //    //    i = column2.DataType.IndexOf('(');
        //    //    string s2 = i > 0 ? column2.DataType.Substring(0, i).Trim() : column2.DataType;
        //    //    if (!s1.Equals(s2, StringComparison.OrdinalIgnoreCase))
        //    //    {
        //    //        return false;
        //    //    }

        //    //    if ((s1.Equals("nchar", StringComparison.OrdinalIgnoreCase)
        //    //        || s1.Equals("varchar", StringComparison.OrdinalIgnoreCase)
        //    //        || s1.Equals("nvarchar", StringComparison.OrdinalIgnoreCase))
        //    //        && column1.MaxLength != column2.MaxLength)
        //    //    {
        //    //        return false;
        //    //    }

        //    //    if (s1.Equals("decimal", StringComparison.OrdinalIgnoreCase)
        //    //        && column1.MaxLength != column2.MaxLength
        //    //        && column1.MinLength != column2.MinLength)
        //    //    {
        //    //        return false;
        //    //    }
        //    //}

        //    return true;
        //}

        private void GetIndexName(string table, List<ColumnInfoModel> columns)
        {
            this.db.ClearParameters();
            this.db.CommandText = @"SELECT [name],[sql] FROM [sqlite_master] WHERE [type]='index' AND [tbl_name]=@tb";
            this.db.AddParameter("@tb", table);
            using (var dt = new DataTable())
            {
                this.db.Fill(dt);
                foreach(DataRow row in dt.Rows)
                {
                    string indexName = row["name"].ToString();
                    string sql = row["sql"].ToString();
                    bool isUnique = sql.IndexOf("CREATE UNIQUE ", StringComparison.OrdinalIgnoreCase) > 0;
                    int begin = sql.IndexOf("(");
                    if (begin > 0)
                    {
                        begin += 1;
                        int end = sql.IndexOf(")", begin);
                        if (end > begin)
                        {
                            string s = sql.Substring(begin, end - begin);
                            string[] arr = s.Split(',');
                            if (arr != null && arr.Length > 0)
                            {
                                foreach (var item in arr)
                                {
                                    string col = item.Trim();
                                    col = col.TrimStart('[');
                                    col = col.TrimEnd(']');
                                    if (!string.IsNullOrEmpty(col))
                                    {
                                        var m = columns.Find(q => string.Compare(q.Name, col,
                                            StringComparison.OrdinalIgnoreCase) == 0);
                                        if (m != null && !m.IsKey)
                                        {
                                            m.IndexName = indexName;
                                            m.IsUnique = isUnique;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取表列信息
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        public override List<ColumnInfoModel> GetTableColumns(string table)
        {
            List<ColumnInfoModel> list = new List<ColumnInfoModel>();
            this.db.ClearParameters();
            this.db.CommandText = string.Format(@"PRAGMA TABLE_INFO('{0}')", table);
            using (var dt = new DataTable())
            {
                this.db.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    string typeString = row["type"].ToString();
                    ColumnInfoModel m = new ColumnInfoModel();
                    list.Add(m);
                    m.Name = row["name"].ToString();
                    m.IsKey = Convert.ToBoolean(row["pk"]);
                    m.IsNullable = !Convert.ToBoolean(row["notnull"]);
                    m.IsAutoIncrement = false;
                    m.DataType = typeString;
                    int index = typeString.IndexOf('(');
                    if (index > 0)
                    {
                        m.DataType = typeString.Substring(0, index);
                        string s = typeString.Substring(index + 1, typeString.Length - index - 2);
                        string[] sarr = s.Split(',');
                        int len = 0;
                        if (int.TryParse(sarr[0].Trim(), out len))
                            m.MaxLength = len;
                        if (sarr.Length > 1 && int.TryParse(sarr[1].Trim(), out len))
                            m.MinLength = len;
                    }
                    m.Order = Convert.ToInt32(row["cid"]);
                    m.IsUnique = false;
                    m.IndexName = null;
                }
                if (list.Count(q => q.IsKey) == 1)
                {
                    this.db.ClearParameters();
                    this.db.CommandText = string.Format(@"SELECT [sql] FROM [sqlite_master] WHERE [type]='table' AND [name]='{0}'", table);
                    var sql = (db.ExecuteScalar() ?? "").ToString().ToUpper();
                    var m = list.Find(q => q.IsKey);
                    m.IsAutoIncrement = sql.Contains("PRIMARY KEY AUTOINCREMENT");
                }
                this.GetIndexName(table, list);
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
                if (minLength > maxLength) minLength = maxLength - 1;
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
        static SQLiteTableSchema()
        {
            dic.Add(typeof(int), "integer");
            dic.Add(typeof(int?), "integer");

            dic.Add(typeof(IntPtr), "integer");
            dic.Add(typeof(IntPtr?), "integer");

            dic.Add(typeof(long), "bigint");
            dic.Add(typeof(long?), "bigint");

            dic.Add(typeof(bool), "boolean");
            dic.Add(typeof(bool?), "boolean");

            dic.Add(typeof(short), "smallint");
            dic.Add(typeof(short?), "smallint");

            dic.Add(typeof(char), "nchar(1)");
            dic.Add(typeof(char?), "nchar(1)");
            dic.Add(typeof(char[]), "nchar({0})");

            dic.Add(typeof(byte), "tinyint");
            dic.Add(typeof(byte?), "tinyint");

            dic.Add(typeof(decimal), "decimal({0},{1})");
            dic.Add(typeof(decimal?), "decimal({0},{1})");

            dic.Add(typeof(float), "float");
            dic.Add(typeof(float?), "float");

            dic.Add(typeof(double), "double");
            dic.Add(typeof(double?), "double");

            dic.Add(typeof(DateTime), "varchar(30)");
            dic.Add(typeof(DateTime?), "varchar(30)");
            dic.Add(typeof(DateTimeOffset), "varchar(30)");
            dic.Add(typeof(DateTimeOffset?), "varchar(30)");

            dic.Add(typeof(TimeSpan), "varchar(30)");
            dic.Add(typeof(TimeSpan?), "varchar(30)");

            dic.Add(typeof(Guid), "varchar(40)");
            dic.Add(typeof(Guid?), "varchar(40)");

            dic.Add(typeof(string), "nvarchar({0})");

            dic.Add(typeof(byte[]), "blob");
        }
    }
}

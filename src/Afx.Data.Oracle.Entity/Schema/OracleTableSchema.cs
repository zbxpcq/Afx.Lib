using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Afx.Data;
using Oracle.ManagedDataAccess.Client;
using Afx.Data.Entity.Schema;

namespace Afx.Data.Oracle.Entity.Schema
{
    /// <summary>
    /// 表结构接口
    /// </summary>
    public class OracleTableSchema : TableSchema
    {
        private Database db;
        private string database;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionString">数据库链接字符串</param>
        public OracleTableSchema(string connectionString)
        {
            var stringBuilder = new OracleConnectionStringBuilder(connectionString);
            this.database = stringBuilder.UserID.ToUpper();
            this.db = new OracleDatabase(connectionString);
        }
        /// <summary>
        /// 执行sql logs
        /// </summary>
        public override Action<string> Log
        {
            get { return this.db.Log; }
            set { this.db.Log = value; }
        }
        /// <summary>
        /// 获取数据库所有表名
        /// </summary>
        /// <returns>数据库所有表名</returns>
        public override List<string> GetTables()
        {
            List<string> list = new List<string>();
            this.db.ClearParameters();
            this.db.CommandText = "SELECT TABLE_NAME FROM ALL_TABLES WHERE OWNER = :p_db";
            this.db.AddParameter("p_db", this.database);
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
        /// <param name="columns">索引列信息</param>
        public override void AddIndex(string table, List<ColumnInfoModel> columns)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (columns == null || columns.Count == 0) throw new ArgumentNullException("columns");
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

            int count = 0;
            this.db.ClearParameters();
            this.db.CommandText = "SELECT COUNT(1) FROM ALL_INDEXES WHERE OWNER = :p_db AND TABLE_NAME = :p_tb AND TABLE_TYPE = 'TABLE' AND INDEX_NAME = :index_name";
            this.db.AddParameter("p_db", this.database);
            this.db.AddParameter("p_tb", table);
            this.db.AddParameter("index_name", indexName);
            object obj = db.ExecuteScalar();
            if (Convert.ToInt32(obj) == 0)
            {
                StringBuilder strColumns = new StringBuilder();
                foreach (var s in columns)
                {
                    strColumns.AppendFormat("\"{0}\", ", s);
                }
                strColumns.Remove(strColumns.Length - 2, 2);

                this.db.ClearParameters();
                this.db.CommandText = string.Format("CREATE {0} INDEX \"{1}\" ON \"{2}\" ({3});",
                    isUnique ? "UNIQUE" : "", indexName, table, strColumns.ToString());
                count = this.db.ExecuteNonQuery();
            }

            return count != 0;
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
                this.db.CommandText = "SELECT COUNT(1) FROM ALL_INDEXES WHERE OWNER = :p_db AND TABLE_NAME = :p_tb AND TABLE_TYPE = 'TABLE' AND INDEX_NAME = :index_name";
                this.db.AddParameter("p_db", this.database);
                this.db.AddParameter("p_tb", table);
                this.db.AddParameter("index_name", column.IndexName);
                object obj = db.ExecuteScalar();
                if (Convert.ToInt32(obj) == 0)
                {
                    this.db.ClearParameters();
                    this.db.CommandText = string.Format("CREATE {0} INDEX \"{2}\" ON \"{3}\" (\"{4}\");",
                        column.IsUnique ? "UNIQUE" : "", column.IndexName, table, column.Name);
                    count = this.db.ExecuteNonQuery();
                }
            }

            return count != 0;
        }
        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="index">索引名称</param>
        /// <returns>是否成功</returns>
        public override bool DeleteIndex(string table, string index)
        {
            int count = 0;
            if (!string.IsNullOrEmpty(index))
            {
                this.db.ClearParameters();
                this.db.CommandText = "SELECT COUNT(1) FROM ALL_INDEXES WHERE OWNER = :p_db AND TABLE_NAME = :p_tb AND TABLE_TYPE = 'TABLE' AND INDEX_NAME = :index_name";
                this.db.AddParameter("p_db", this.database);
                this.db.AddParameter("p_tb", table);
                this.db.AddParameter("index_name", index);
                object obj = db.ExecuteScalar();
                if (Convert.ToInt32(obj) > 0)
                {
                    this.db.ClearParameters();
                    this.db.CommandText = string.Format("DROP INDEX \"{0}\"", index);
                    count = this.db.ExecuteNonQuery();
                }
            }

            return count != 0;
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
            if (columns == null || columns.Count == 0) throw new ArgumentNullException("columns");
            int count = 0;
            StringBuilder createTableSql = new StringBuilder();
            List<ColumnInfoModel> keyColumns = new List<ColumnInfoModel>();
            createTableSql.AppendFormat("CREATE TABLE \"{0}\"(", table);
            foreach (var column in columns)
            {
                createTableSql.AppendFormat("\"{0}\" {1} {2} NULL, ", column.Name, column.DataType, column.IsNullable ? "" : "NOT");
                if (column.IsKey) keyColumns.Add(column);
            }
            createTableSql.Remove(createTableSql.Length - 2, 2);

            if (keyColumns.Count > 0)
            {
                createTableSql.AppendFormat(", CONSTRAINT \"PK_{0}\" PRIMARY KEY (", table);

                foreach (var column in keyColumns)
                {
                    createTableSql.AppendFormat("\"{0}\", ", column.Name);
                }
                createTableSql.Remove(createTableSql.Length - 2, 2);
                createTableSql.Append(")");
            }

            createTableSql.Append(")");

            this.db.ClearParameters();
            using (var tx = this.db.BeginTransaction())
            {
                this.db.CommandText = createTableSql.ToString();
                count = this.db.ExecuteNonQuery();
                foreach (var column in columns.Where(q => q.IsAutoIncrement))
                {
                    this.AddAutoIncrement(table, column);
                }
                this.AddIndex(table, columns);

                tx.Commit();
            }

            return count != 0;
        }

        private string GetSequenceName(string table, string column)
        {
            return string.Format("SQ_{0}_{1}", table, column);
        }

        private string GetTriggerName(string table, string column)
        {
            return string.Format("TR_{0}_{1}", table, column);
        }

        private int AddAutoIncrement(string table, ColumnInfoModel column)
        {
            int count = 0;
            if(column.IsAutoIncrement)
            {
                string sq_name = this.GetSequenceName(table, column.Name);
                this.db.ClearParameters();
                this.db.CommandText = "SELECT COUNT(1) FROM ALL_SEQUENCES WHERE SEQUENCE_OWNER = :p_db AND SEQUENCE_NAME = :sq_name";
                this.db.AddParameter(":p_db", this.database);
                this.db.AddParameter(":sq_name", sq_name);
                object obj = this.db.ExecuteScalar();
                if (Convert.ToInt32(obj) == 0)
                {
                    this.db.ClearParameters();
                    this.db.CommandText = string.Format("CREATE SEQUENCE \"{0}\" MINVALUE 1 MAXVALUE 9999999999999999999999999999 START WITH 1 INCREMENT BY 1 NOCACHE", sq_name);
                    count += this.db.ExecuteNonQuery();
                }

                string tr_name = this.GetTriggerName(table, column.Name);
                this.db.ClearParameters();
                this.db.CommandText = "SELECT COUNT(1) FROM ALL_TRIGGERS WHERE OWNER = :p_db AND TRIGGERING_EVENT = 'INSERT' AND TABLE_NAME = :p_tb AND TRIGGER_NAME = :tr_name";
                this.db.AddParameter("p_db", this.database);
                this.db.AddParameter("p_tb", table);
                this.db.AddParameter("tr_name", tr_name);
                obj = this.db.ExecuteScalar();
                if (Convert.ToInt32(obj) == 0)
                {
                    this.db.ClearParameters();
                    this.db.CommandText = string.Format("CREATE TRIGGER \"{0}\" BEFORE INSERT ON \"{1}\" FOR EACH ROW BEGIN SELECT \"{2}\".NEXTVAL INTO :NEW.\"{3}\" FROM DUAL; END;", tr_name, table, sq_name, column.Name);
                    count += db.ExecuteNonQuery();
                }
            }

            return count;
        }

        //public override bool DeleteTable(string table)
        //{
        //    this.db.ClearParameters();
        //    this.db.CommandText = string.Format("DROP TABLE \"{0}\"", table);
        //    int count = this.db.ExecuteNonQuery();
        //    return count != 0;
        //}
        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="column">列信息</param>
        /// <returns>是否成功</returns>
        public override bool AddColumn(string table, ColumnInfoModel column)
        {
            int count = 0;
            using (this.db.BeginTransaction())
            {
                this.db.ClearParameters();
                this.db.CommandText = string.Format("ALTER TABLE \"{0}\" ADD (\"{1}\" {2} {3} NULL)",
                    table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
                count = this.db.ExecuteNonQuery();
                if(column.IsAutoIncrement)
                {
                    count += this.AddAutoIncrement(table, column);
                }
                this.AddIndex(table, column);
                this.db.Commit();
            }

            return count != 0;
        }

        //public override bool DeleteColumn(string table, ColumnInfoModel column)
        //{
        //    int count = 0;
        //    using (var tx = this.db.BeginTransaction())
        //    {
        //        if (!string.IsNullOrEmpty(column.IndexName))
        //        {
        //            this.db.ClearParameters();
        //            this.db.CommandText = "SELECT COUNT(1) FROM ALL_INDEXES WHERE OWNER = :p_db AND TABLE_NAME = :p_tb AND TABLE_TYPE = 'TABLE' AND INDEX_NAME = :index_name";
        //            this.db.AddParameter("p_db", this.database);
        //            this.db.AddParameter("p_tb", table);
        //            this.db.AddParameter("index_name", column.IndexName);
        //            object obj = db.ExecuteScalar();
        //            if (Convert.ToInt32(obj) > 0)
        //            {
        //                this.db.ClearParameters();
        //                this.db.CommandText = string.Format("DROP INDEX \"{1}\"", table, column.IndexName);
        //                count += this.db.ExecuteNonQuery();
        //            }
        //        }
        //        if (column.IsAutoIncrement)
        //        {
        //            string sq_name = this.GetSequenceName(table, column.Name);
        //            this.db.ClearParameters();
        //            this.db.CommandText = "SELECT COUNT(1) FROM ALL_SEQUENCES WHERE SEQUENCE_OWNER = :p_db AND SEQUENCE_NAME = :sq_name";
        //            this.db.AddParameter("p_db", this.database);
        //            this.db.AddParameter("sq_name", sq_name);
        //            object obj = db.ExecuteScalar();
        //            if (Convert.ToInt32(obj) > 0)
        //            {
        //                this.db.ClearParameters();
        //                db.CommandText = string.Format("DROP SEQUENCE \"{0}\"", sq_name);
        //                count += db.ExecuteNonQuery();
        //            }

        //            string tr_name = this.GetTriggerName(table, column.Name);
        //            this.db.ClearParameters();
        //            this.db.CommandText = "SELECT COUNT(1) FROM ALL_TRIGGERS WHERE OWNER = :p_db AND TRIGGERING_EVENT = 'INSERT'AND TABLE_NAME = :p_tb AND TRIGGER_NAME = :tr_name";
        //            this.db.AddParameter("p_db", this.database);
        //            this.db.AddParameter("p_tb", table);
        //            this.db.AddParameter("tr_name", tr_name);
        //            obj = db.ExecuteScalar();
        //            if (Convert.ToInt32(obj) > 0)
        //            {
        //                this.db.ClearParameters();
        //                db.CommandText = string.Format("DROP TRIGGER \"{0}\"", tr_name);
        //                count += db.ExecuteNonQuery();
        //            }
        //        }
        //        this.db.ClearParameters();
        //        this.db.CommandText = string.Format("ALTER TABLE \"{0}\" DROP COLUMN \"{1}\"", table, column.Name);
        //        count = this.db.ExecuteNonQuery();

        //        tx.Commit();
        //    }

        //    return count != 0;
        //}

        //public override bool AlterColumn(string table, ColumnInfoModel column)
        //{
        //    //this.db.ClearParameters();
        //    //this.db.CommandText = string.Format("ALTER TABLE \"{0}\" MODIFY (\"{1}\" {2} {3} NULL)", table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
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
        /// <summary>
        /// 获取表列信息
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        public override List<ColumnInfoModel> GetTableColumns(string table)
        {
            List<ColumnInfoModel> list = new List<ColumnInfoModel>();
            this.db.ClearParameters();
            // column
            this.db.CommandText = "SELECT COLUMN_ID, COLUMN_NAME, DATA_TYPE, DATA_LENGTH, CHAR_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE FROM ALL_TAB_COLUMNS WHERE OWNER = :p_db AND TABLE_NAME = :p_tb ORDER BY COLUMN_ID";
            this.db.AddParameter("p_db", this.database);
            this.db.AddParameter("p_tb", table);
            using (DataTable dt = new DataTable())
            {
                this.db.Fill(dt);
                // tr
                this.db.CommandText = "SELECT TRIGGER_NAME, TRIGGER_BODY FROM ALL_TRIGGERS WHERE OWNER = :p_db AND TRIGGERING_EVENT = 'INSERT' AND TABLE_NAME = :p_tb";// AND TABLE_OWNER = :p_db
                using (DataTable tr_dt = new DataTable())
                {
                    this.db.Fill(tr_dt);
                    // key
                    this.db.CommandText = "SELECT a.COLUMN_NAME FROM ALL_CONS_COLUMNS a INNER JOIN ALL_CONSTRAINTS b ON a.OWNER=b.OWNER AND a.CONSTRAINT_NAME = b.CONSTRAINT_NAME AND a.TABLE_NAME=b.TABLE_NAME WHERE b.OWNER = :p_db AND b.TABLE_NAME = :p_tb AND b.CONSTRAINT_TYPE = 'P'";
                    using (DataTable key_dt = new DataTable())
                    {
                        this.db.Fill(key_dt);
                        // index
                        this.db.CommandText = "SELECT a.INDEX_NAME, a.COLUMN_NAME, b.UNIQUENESS FROM ALL_IND_COLUMNS a INNER JOIN ALL_INDEXES b ON a.TABLE_OWNER = b.TABLE_OWNER AND a.TABLE_NAME = b.TABLE_NAME AND a.INDEX_NAME = b.INDEX_NAME WHERE a.TABLE_OWNER = :p_db AND a.TABLE_NAME = :p_tb AND  b.TABLE_TYPE = 'TABLE' ORDER BY a.INDEX_NAME, a.COLUMN_POSITION";
                        using (DataTable in_dt = new DataTable())
                        {
                            this.db.Fill(in_dt);
                            this.db.ClearParameters();
                            // sq
                            this.db.CommandText = "SELECT SEQUENCE_NAME FROM ALL_SEQUENCES WHERE SEQUENCE_OWNER = :p_db";
                            this.db.AddParameter("p_db", this.database);
                            using (DataTable sq_dt = new DataTable())
                            {
                                this.db.Fill(sq_dt);
                                foreach (DataRow row in dt.Rows)
                                {
                                    ColumnInfoModel m = new ColumnInfoModel();
                                    list.Add(m);
                                    m.Name = row["COLUMN_NAME"].ToString();
                                    m.IsNullable = string.Compare(row["NULLABLE"].ToString(), "Y", true) == 0;
                                    m.Order = Convert.ToInt32(row["COLUMN_ID"]);
                                    m.IsKey = false;
                                    foreach(DataRow r in key_dt.Rows)
                                    {
                                        if(string.Compare(r["COLUMN_NAME"].ToString(), m.Name, true) == 0)
                                        {
                                            m.IsKey = true;
                                            break;
                                        }
                                    }

                                    m.IsAutoIncrement = false;
                                    foreach (DataRow r in tr_dt.Rows)
                                    {
                                        string tr = this.GetTriggerName(table, m.Name);
                                        if (string.Compare(r["TRIGGER_NAME"].ToString(), tr, true) == 0)
                                        {
                                            string tr_body = r["TRIGGER_BODY"].ToString().ToLower();
                                            foreach (DataRow rq in sq_dt.Rows)
                                            {
                                                string sq = this.GetSequenceName(table, m.Name);
                                                if (string.Compare(r["SEQUENCE_NAME"].ToString(), sq, true) == 0)
                                                {
                                                    m.IsAutoIncrement = tr_body.Contains(sq.ToLower()) && tr_body.Contains(m.Name.ToLower());
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    }

                                    m.IsUnique = false;
                                    if (!m.IsKey)
                                    {
                                        foreach (DataRow r in in_dt.Rows)
                                        {
                                            if (string.Compare(r["COLUMN_NAME"].ToString(), m.Name, true) == 0)
                                            {
                                                m.IndexName = r["INDEX_NAME"].ToString();
                                                m.IsUnique = string.Compare(r["INDEX_NAME"].ToString(), "UNIQUE", true) == 0;
                                                break;
                                            }
                                        }
                                    }

                                    int len = 0;
                                    string type = row["DATA_TYPE"].ToString();
                                    if (string.Compare(type, "NUMBER", true) == 0)
                                    {
                                        int.TryParse(row["DATA_PRECISION"].ToString(), out len);
                                        m.MaxLength = len;
                                        int.TryParse(row["DATA_SCALE"].ToString(), out len);
                                        m.MinLength = len;
                                    }
                                    else
                                    {
                                        string s = "CHAR_LENGTH";
                                        if (string.Compare(type, "RAW", true) == 0)
                                            s = "DATA_LENGTH";
                                        int.TryParse(row[s].ToString(), out len);
                                        m.MaxLength = len;
                                    }
                                    m.DataType = type;
                                    
                                }
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
            else if (typeof(byte[]) == propertyType)
            {
                if (maxLength > 2000 || maxLength == 0)
                    type = "blob";
            }
            else if (typeof(string) == propertyType)
            {
                if (maxLength > 1024)
                    type = "NCLOB";
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
        static OracleTableSchema()
        {
            dic.Add(typeof(int), "NUMBER(11, 0)");
            dic.Add(typeof(int?), "NUMBER(11, 0)");

            dic.Add(typeof(IntPtr), "NUMBER(11, 0)");
            dic.Add(typeof(IntPtr?), "NUMBER(11, 0)");

            dic.Add(typeof(long), "NUMBER(19, 0)");
            dic.Add(typeof(long?), "NUMBER(19, 0)");

            dic.Add(typeof(short), "NUMBER(5, 0)");
            dic.Add(typeof(short?), "NUMBER(5, 0)");

            dic.Add(typeof(byte), "NUMBER(3, 0)");
            dic.Add(typeof(byte?), "NUMBER(3, 0)");

            dic.Add(typeof(bool), "NUMBER(1,0)");
            dic.Add(typeof(bool?), "NUMBER(1,0)");

            dic.Add(typeof(char), "CHAR(1)");
            dic.Add(typeof(char?), "CHAR(1)");
            dic.Add(typeof(char[]), "CHAR({0})");

            dic.Add(typeof(decimal), "NUMBER({0},{1})");
            dic.Add(typeof(decimal?), "NUMBER({0},{1})");

            dic.Add(typeof(float), "BINARY_FLOAT");
            dic.Add(typeof(float?), "BINARY_FLOAT");

            dic.Add(typeof(double), "BINARY_DOUBLE");
            dic.Add(typeof(double?), "BINARY_DOUBLE");

            dic.Add(typeof(DateTime), "TIMESTAMP(6)");//TIMESTAMP(6)
            dic.Add(typeof(DateTime?), "TIMESTAMP(6)");//TIMESTAMP(6)

            dic.Add(typeof(DateTimeOffset), "TIMESTAMP (7) WITH TIME ZONE");
            dic.Add(typeof(DateTimeOffset?), "TIMESTAMP (7) WITH TIME ZONE");

            dic.Add(typeof(TimeSpan), "TIMESTAMP(6)");//
            dic.Add(typeof(TimeSpan?), "TIMESTAMP(6)");//

            dic.Add(typeof(Guid), "VARCHAR2(50)");
            dic.Add(typeof(Guid?), "VARCHAR2(50)");

            dic.Add(typeof(string), "NVARCHAR2({0})");//1024
           // dic.Add("text", "NCLOB");//4GB

            dic.Add(typeof(byte[]), "RAW({0})");//2000
            //dic.Add("blob", "BLOB");//4GB
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;
using Afx.Data;
using Afx.Data.Entity.Schema;

namespace Afx.Data.MySql.Entity.Schema
{
    /// <summary>
    /// 获取数据库结构信息
    /// </summary>
    public sealed class MySqlDatabaseSchema : DatabaseSchema
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
        public MySqlDatabaseSchema(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
            //Server=127.0.0.1;Port=3306;Database=FileSystem;User Id=root;Password=mycsv.cn;CharacterSet=UTF8;Pooling=True;MinPoolSize=1;MaxPoolSize=100;ConnectionLifeTime=30;Keepalive=30
            var _connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
            var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
            this.database = connectionStringBuilder.Database;
            connectionStringBuilder.Database = "mysql";
            connectionStringBuilder.Remove("MinPoolSize");
            connectionStringBuilder.Remove("MaxPoolSize");
            connectionStringBuilder.Remove("ConnectionLifeTime");
            connectionStringBuilder.Remove("Keepalive");
            connectionStringBuilder.Pooling = false;

            this.db = new MySqlDatabase(connectionStringBuilder.ConnectionString);
        }
        /// <summary>
        /// 是否存在数据库
        /// </summary>
        /// <returns>true：存在，false：不存在</returns>
        public override bool Exist()
        {
            this.db.ClearParameters();
            this.db.CommandText = "SELECT COUNT(1) FROM `information_schema`.`SCHEMATA` WHERE schema_name=?name";
            this.db.AddParameter("?name", this.database);
            object obj = this.db.ExecuteScalar();
            int count = Convert.ToInt32(obj);

            return count > 0;
        }
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <returns>true：创建成功，false：创建失败</returns>
        public override bool CreateDatabase()
        {
            this.db.ClearParameters();
            this.db.CommandText = string.Format("CREATE DATABASE `{0}` CHARACTER SET utf8 COLLATE utf8_general_ci;",
                this.database);
            int count = this.db.ExecuteNonQuery();

            return count > 0;
        }
        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <returns>true：删除成功，false：删除失败</returns>
        public override bool DeleteDatabase()
        {
            this.db.ClearParameters();
            this.db.CommandText = string.Format("DROP DATABASE `{0}`;",
                this.database);
            int count = this.db.ExecuteNonQuery();

            return count > 0;
        }
        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <returns>数据库名称</returns>
        public override string GetDatabase()
        {
           return this.database;
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
    }
}

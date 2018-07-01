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
    /// 获取数据库结构信息
    /// </summary>
    public sealed class MsSqlDatabaseSchema : DatabaseSchema
    {
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

        private Database db;
        private string database;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionString"></param>
        public MsSqlDatabaseSchema(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
            //Data Source=127.0.0.1;Initial Catalog=master;User ID=sa;Password=123;Pooling=False;Min Pool Size=1;Max Pool Size=100;Load Balance Timeout=30;Application Name=Afx.Entity
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            database = connectionStringBuilder.InitialCatalog;
            connectionStringBuilder.InitialCatalog = "master";
            connectionStringBuilder.Remove("Min Pool Size");
            connectionStringBuilder.Remove("Max Pool Size");
            connectionStringBuilder.Remove("Load Balance Timeoute");
            connectionStringBuilder.Pooling = false;
            
            this.db = new MsSqlDatabase(connectionStringBuilder.ConnectionString);
        }
        /// <summary>
        /// 是否存在数据库
        /// </summary>
        /// <returns></returns>
        public override bool Exist()
        {
            this.db.ClearParameters();
            this.db.CommandText = @"SELECT COUNT(1) FROM sys.databases WHERE name=@name";
            this.db.AddParameter("name", database);
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
//            this.db.CommandText = @"CREATE DATABASE [ssss] ON  PRIMARY 
//( NAME = N'ssss', FILENAME = N'D:\Program Files\Microsoft SQL Server\DATA\ssss.mdf' , SIZE = 3072KB , FILEGROWTH = 1024KB )
// LOG ON 
//( NAME = N'ssss_log', FILENAME = N'D:\Program Files\Microsoft SQL Server\DATA\ssss_log.ldf' , SIZE = 1024KB , FILEGROWTH = 10%)";
            this.db.CommandText = string.Format("CREATE DATABASE [{0}]", database);
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
            this.db.CommandText = string.Format("DROP DATABASE [{0}]", database);
            int count = this.db.ExecuteNonQuery();

            return count > 0;
        }
        /// <summary>
        /// 释放所有资源
        /// </summary>
        public override void Dispose()
        {
            if (this.db != null) db.Dispose();
            this.db = null;
            base.Dispose();
        }
        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <returns>数据库名称</returns>
        public override string GetDatabase()
        {
            return this.database;
        }
    }
}

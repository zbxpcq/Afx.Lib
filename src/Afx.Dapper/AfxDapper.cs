using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Dapper;

namespace Afx.Dapper
{
    public class AfxDapper : IAfxDapper
    {
        class CloseConnection: IDisposable
        {
            private AfxDapper db;
            public CloseConnection(AfxDapper db)
            {
                this.db = db;
            }

            public void Dispose()
            {
                if(this.db != null)
                {
                    this.db.Close();
                    this.db = null;
                }
            }
        }

        private DbTransaction transaction = null;
        private bool isOwnsConnection = true;
        /// <summary>
        /// 执行sql logs
        /// </summary>
        public Action<string> Log { get; set; }
        /// <summary>
        /// 执行sql logs
        /// </summary>
        protected virtual void OnLog(string sql)
        {
            if (this.Log != null)
            {
                try { this.Log(sql ?? ""); }
                catch { }
            }
        }

        /// <summary>
        /// 资源是否释放
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return this.Connection != null ? this.Connection.ConnectionString : null;
            }
        }

        /// <summary>
        /// DB 提供程序Factory
        /// </summary>
        public DbProviderFactory ProviderFactory { get; private set; }

        /// <summary>
        /// 相关的参数
        /// </summary>
        public object Parameters { get; set; }

        /// <summary>
        /// 获取或设置针对数据源运行的文本命令。
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// 指示或指定如何解释 CommandText 属性
        /// </summary>
        public CommandType? CommandType { get; set; }

        /// <summary>
        /// （以秒为单位）
        /// </summary>
        public int? CommandTimeout { get; set; }

        /// <summary>
        /// DbConnection
        /// </summary>
        public DbConnection Connection { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="isOwnsConnection"></param>
        public AfxDapper(DbConnection connection, bool isOwnsConnection = true)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            this.Connection = connection;
            var t = this.Connection.GetType();
            var p = t.GetProperty("DbProviderFactory", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            this.ProviderFactory = p.GetValue(this.Connection, null) as System.Data.Common.DbProviderFactory;
            this.isOwnsConnection = isOwnsConnection;
            this.IsDisposed = false;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="providerFactory"></param>
        public AfxDapper(string ConnectionString, DbProviderFactory providerFactory)
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentNullException("ConnectionString");
            if (providerFactory == null) throw new ArgumentNullException("providerFactory");
            this.ProviderFactory = providerFactory;
            this.Connection = this.CreateConnection();
            this.Connection.ConnectionString = ConnectionString;
            this.IsDisposed = false;
        }

        /// <summary>
        /// 创建全新 DbConnection
        /// </summary>
        /// <returns>DbConnection</returns>
        public virtual DbConnection CreateConnection()
        {
            var result = this.ProviderFactory.CreateConnection();
            if (result == null) throw new InvalidConstraintException("ProviderFactory.CreateConnection is null.");

            return result;
        }

        private CloseConnection Open()
        {
            if (ConnectionState.Open != this.Connection.State)
            {
                this.Connection.Open();
                this.OnLog("-- Connection Open");
            }

            return new CloseConnection(this);
        }

        internal void Close()
        {
            if (null == this.transaction && ConnectionState.Closed != this.Connection.State)
            {
                this.Connection.Close();
                this.OnLog("-- Connection Close");
            }
        }

        private CommandDefinition GetCommandDefinition()
        {
            if (this.Log != null)
            {
                if (this.CommandType.HasValue)
                {
                    this.OnLog("-- CommandType = " + this.CommandType.Value.ToString());
                }
                if (this.CommandTimeout.HasValue)
                {
                    this.OnLog("-- CommandTimeout = " + this.CommandTimeout.Value.ToString());
                }
                if (this.Parameters != null)
                {
                    if (this.Parameters is IEnumerable<KeyValuePair<string, object>>)
                    {
                        var dic = this.Parameters as IEnumerable<KeyValuePair<string, object>>;
                        foreach(KeyValuePair<string, object> kv in dic)
                        {
                            this.OnLog(string.Format("-- {0} = {1}", kv.Key, (kv.Value == null || kv.Value == DBNull.Value) ? "null" : kv.Value.ToString()));
                        }
                    }
                    else
                    {
                        var ps = this.Parameters.GetType().GetProperties();
                        foreach (var p in ps)
                        {
                            var val = p.GetValue(this.Parameters, null);
                            this.OnLog(string.Format("-- {0} = {1}", p.Name, (val == null || val == DBNull.Value) ? "null" : val.ToString()));
                        }
                    }
                }
            }
            return new CommandDefinition(this.CommandText, this.Parameters, this.transaction, this.CommandTimeout, this.CommandType);
        }

        /// <summary>
        /// 执行并返回查询结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ExecuteScalar<T>()
        {
            using (this.Open())
            {
                T result = this.Connection.ExecuteScalar<T>(this.GetCommandDefinition());
                this.OnLog("-- ExecuteScalar");
                return result;
            }
        }

        /// <summary>
        /// 对连接对象执行 SQL 语句。
        /// </summary>
        /// <returns>受影响的行数。</returns>
        public int ExecuteNonQuery()
        {
            using (this.Open())
            {
                int result = this.Connection.Execute(this.GetCommandDefinition());
                this.OnLog("-- ExecuteNonQuery");
                return result;
            }
        }

        public IEnumerable<T> Query<T>()
        {
            using (this.Open())
            {
                var result = this.Connection.Query<T>(this.GetCommandDefinition());
                this.OnLog("-- Query");
                return result;
            }
        }

        #region 事务
        /// <summary>
        /// 是否开启事务
        /// </summary>
        public bool IsTransaction
        {
            get { return null != this.transaction; }
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        public AfxTransaction BeginTransaction()
        {
            if (null != this.transaction) throw new InvalidOperationException("已开启事务，不能重复开启！");
            this.Open();
            this.transaction = this.Connection.BeginTransaction();
            this.OnLog("-- BeginTransaction");
            return new AfxTransaction(this);
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="isolationLevel">事务级别</param>
        public AfxTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (null != this.transaction) throw new InvalidOperationException("已开启事务，不能重复开启！");
            this.Open();
            this.transaction = this.Connection.BeginTransaction(isolationLevel);
            this.OnLog("-- BeginTransaction " + isolationLevel.ToString());
            return new AfxTransaction(this);
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            if (null == this.transaction) throw new InvalidOperationException("未开启事务，不能提交！");
            AfxTransaction.ClearCurrent();
            this.transaction.Commit();
            this.transaction.Dispose();
            this.transaction = null;
            this.OnLog("-- Commit");
            this.Close();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            AfxTransaction.ClearCurrent();
            if (null != this.transaction)
            {
                this.transaction.Rollback();
                this.transaction.Dispose();
                this.transaction = null;
                this.OnLog("-- Rollback");
                this.Close();
            }
        }
        #endregion

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            if (false == this.IsDisposed)
            {
                this.IsDisposed = true;
                this.Rollback();
                this.Close();

                if (this.Connection != null && isOwnsConnection) this.Connection.Dispose();
                this.Connection = null;
                this.ProviderFactory = null;
                this.Parameters = null;
                this.Log = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Afx.Data
{
    /// <summary>
    /// 数据库访问基类
    /// </summary>
    public abstract class Database : IDatabase
    {
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
        /// 获取或设置针对数据源运行的文本命令。
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// 指示或指定如何解释 CommandText 属性
        /// </summary>
        public CommandType? CommandType { get; set; }

        /// <summary>
        /// DbConnection
        /// </summary>
        public DbConnection Connection { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="isOwnsConnection"></param>
        protected Database(DbConnection connection, bool isOwnsConnection = true)
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
        protected Database(string ConnectionString, DbProviderFactory providerFactory)
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentNullException("ConnectionString");
            if (providerFactory == null) throw new ArgumentNullException("providerFactory");
            this.ProviderFactory = providerFactory;
            this.Connection = this.CreateConnection();
            this.Connection.ConnectionString = ConnectionString;
            this.IsDisposed = false;
        }

        private DbCommand GetCommand()
        {
            var command = this.CreateCommand();
            command.Connection = this.Connection;
            command.CommandText = this.CommandText;
            this.OnLog(this.CommandText);
            if (this.CommandType.HasValue)
            {
                command.CommandType = this.CommandType.Value;
                this.OnLog("-- CommandType = " + this.CommandType.Value.ToString());
            }
            if (this.parameters.Count > 0)
            {
                foreach (var p in parameters)
                {
                    if (p.Value == null) p.Value = DBNull.Value;
                    else if (p.Value.GetType().IsEnum) p.Value = (int)p.Value;
                    command.Parameters.Add(p);
                    this.OnLog(string.Format("-- {0} = {1}", p.ParameterName, p.Value == DBNull.Value ? "null" : p.Value.ToString()));
                }
            }
            if (this.transaction != null)
            {
                command.Transaction = transaction;
            }

            return command;
        }

        

        #region
        /// <summary>
        /// 添加 DbParameter 
        /// </summary>
        /// <param name="name">要添加到集合中的 DbParameter 的 DbParameter.ParameterName。</param>
        /// <param name="value">要添加到集合中的 DbParameter 的 DbParameter.Value。</param>
        public void AddParameter(string name, object value)
        {
            DbParameter param = this.CreateParameter(name, value ?? DBNull.Value);
            this.parameters.Add(param);
        }

        /// <summary>
        /// 添加 DbParameter 
        /// </summary>
        /// <param name="parameter">DbParameter</param>
        public void AddParameter(DbParameter parameter)
        {
            if (parameter != null)
            {
                this.parameters.Add(parameter);
            }
        }

        /// <summary>
        /// 添加 DbParameter[]
        /// </summary>
        /// <param name="parameters">DbParameter[]</param>
        public void AddParameter(DbParameter[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                this.parameters.AddRange(parameters);
            }
        }

        /// <summary>
        /// 移除所有 DbParameter
        /// </summary>
        public void ClearParameters()
        {
            this.parameters.Clear();
        }

        private List<DbParameter> parameters = new List<DbParameter>();
        /// <summary>
        /// 相关的参数集合
        /// </summary>
        public List<DbParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }
        #endregion

        private void Open()
        {
            if (ConnectionState.Open != this.Connection.State)
            {
                this.Connection.Open();
                this.OnLog("-- Connection Open");
            }
        }

        internal void Close()
        {
            if (null == this.transaction && ConnectionState.Closed != this.Connection.State)
            {
                this.Connection.Close();
                this.OnLog("-- Connection Close");
            }
        }

        /// <summary>
        /// 执行并返回 DbDataReader
        /// </summary>
        /// <returns>DbDataReader</returns>
        public DbDataReader ExecuteReader()
        {
            try
            {
                using (var command = this.GetCommand())
                {
                    this.Open();
                    var result = command.ExecuteReader();
                    this.OnLog("-- ExecuteReader");
                    return new AfxDataReader(this, result);
                }
            }
            catch (Exception ex)
            {
                this.Close();
                throw ex;
            }
        }

        /// <summary>
        /// 执行并返回 DbDataReader
        /// </summary>
        /// <param name="behavior">System.Data.CommandBehavior 值之一。</param>
        /// <returns>DbDataReader</returns>
        public DbDataReader ExecuteReader(CommandBehavior behavior)
        {
            try
            {
                using (var command = this.GetCommand())
                {
                    this.Open();
                    var result = command.ExecuteReader(behavior);
                    this.OnLog("-- ExecuteReader " + behavior.ToString());
                    return new AfxDataReader(this, result);
                }
            }
            catch (Exception ex)
            {
                this.Close();
                throw ex;
            }
        }

        /// <summary>
        /// 执行并返回查询结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <returns>object</returns>
        public object ExecuteScalar()
        {
            object obj = null;
            try
            {
                using (var command = this.GetCommand())
                {
                    this.Open();
                    obj = command.ExecuteScalar();
                    this.OnLog("-- ExecuteScalar");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Close();
            }

            return obj;
        }

        /// <summary>
        /// 对连接对象执行 SQL 语句。
        /// </summary>
        /// <returns>受影响的行数。</returns>
        public int ExecuteNonQuery()
        {
            int num = 0;
            try
            {
                using (var command = this.GetCommand())
                {
                    this.Open();
                    num = command.ExecuteNonQuery();
                    this.OnLog("-- ExecuteNonQuery");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Close();
            }

            return num;
        }

        /// <summary>
        /// 在 DataTable 的指定范围中添加或刷新行
        /// </summary>
        /// <param name="dt">用于表映射的 DataTable 的名称</param>
        /// <returns>已在 DataTable 中成功添加或刷新的行数。</returns>
        public int Fill(DataTable dt)
        {
            int num = 0;
            try
            {
                using (DbDataAdapter adapter = this.CreateDataAdapter())
                {
                    using (var command = this.GetCommand())
                    {
                        adapter.SelectCommand = command;
                        this.Open();
                        num = adapter.Fill(dt);
                        this.OnLog("-- Fill DataTable");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Close();
            }

            return num;
        }

        /// <summary>
        /// 在 System.Data.DataSet 中添加或刷新行。
        /// </summary>
        /// <param name="ds">要用记录和架构（如有必要）填充的 DataSet</param>
        /// <returns>已在 DataSet 中成功添加或刷新的行数</returns>
        public int Fill(DataSet ds)
        {
            int num = 0;
            try
            {
                using (DbDataAdapter adapter = this.CreateDataAdapter())
                {
                    using (var command = this.GetCommand())
                    {
                        adapter.SelectCommand = command;
                        this.Open();
                        num = adapter.Fill(ds);
                        this.OnLog("-- Fill DataSet");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Close();
            }
            return num;
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
        public Transaction BeginTransaction()
        {
            if (null == this.transaction)
            {
                this.Open();
                this.transaction = this.Connection.BeginTransaction();
                this.OnLog("-- BeginTransaction");
                return new Transaction(this);
            }

            return Transaction.Current;
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="isolationLevel">事务级别</param>
        public Transaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (null == this.transaction)
            {
                this.Open();
                this.transaction = this.Connection.BeginTransaction(isolationLevel);
                this.OnLog("-- BeginTransaction " + isolationLevel.ToString());
                return new Transaction(this);
            }

            return Transaction.Current;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            Transaction.ClearCurrent();
            if (null != this.transaction)
            {
                this.transaction.Commit();
                this.transaction.Dispose();
                this.transaction = null;
                this.OnLog("-- Commit");
                this.Close();
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            Transaction.ClearCurrent();
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
                this.ProviderFactory= null;
                this.parameters.Clear();
                this.Log = null;
            }
        }

        #region
        /// <summary>
        /// 创建全新 DbConnection
        /// </summary>
        /// <returns>DbConnection</returns>
        public DbConnection CreateConnection()
        {
            return this.ProviderFactory.CreateConnection();
        }

        /// <summary>
        /// 创建全新 DbCommand
        /// </summary>
        /// <returns>DbCommand</returns>
        public DbCommand CreateCommand()
        {
            return this.ProviderFactory.CreateCommand();
        }

        /// <summary>
        /// 创建全新 DbParameter
        /// </summary>
        /// <returns>DbParameter</returns>
        public DbParameter CreateParameter()
        {
            return this.ProviderFactory.CreateParameter();
        }

        /// <summary>
        /// 创建全新 DbParameter
        /// </summary>
        /// <param name="name">ParameterName</param>
        /// <param name="value">Value</param>
        /// <returns>DbParameter</returns>
        public DbParameter CreateParameter(string name, object value)
        {
            var parameter = this.ProviderFactory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }

        /// <summary>
        /// 创建全新 DbDataAdapter
        /// </summary>
        /// <returns>DbDataAdapter</returns>
        public DbDataAdapter CreateDataAdapter()
        {
            return this.ProviderFactory.CreateDataAdapter();
        }
        #endregion

        /// <summary>
        /// 参数化查询名称加前缀
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract string EncodeParameterName(string name);

        /// <summary>
        /// 列名转义
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public abstract string EncodeColumn(string column);

        /// <summary>
        /// 获取当前local时间
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetLocalNow()
        {
            return DateTime.Now;
        }
        /// <summary>
        /// 获取当前utc时间
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetUtcNow()
        {
            return DateTime.Now.ToUniversalTime();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;

namespace Afx.Data
{
    /// <summary>
    /// 数据库访问接口
    /// </summary>
    public interface IDatabase: IDisposable
    {
        /// <summary>
        /// 执行sql logs
        /// </summary>
        Action<string> Log { get; set; }

        /// <summary>
        /// 资源是否释放
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// 获取或设置针对数据源运行的文本命令。
        /// </summary>
        string CommandText { get; set; }

        /// <summary>
        /// 相关的参数集合
        /// </summary>
        List<DbParameter> Parameters { get; }

        /// <summary>
        /// 指示或指定如何解释 CommandText 属性
        /// </summary>
        CommandType? CommandType { get; set; }

        /// <summary>
        /// （以秒为单位）
        /// </summary>
        int? CommandTimeout { get; set; }

        /// <summary>
        /// DbConnection
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// 添加 DbParameter 
        /// </summary>
        /// <param name="name">要添加到集合中的 DbParameter 的 DbParameter.ParameterName。</param>
        /// <param name="value">要添加到集合中的 DbParameter 的 DbParameter.Value。</param>
        void AddParameter(string name, object value, DbType? dbType = null);

        /// <summary>
        /// 添加 DbParameter 
        /// </summary>
        /// <param name="parameter">DbParameter</param>
        void AddParameter(DbParameter parameter);

        /// <summary>
        /// 添加 DbParameter[]
        /// </summary>
        /// <param name="parameters">DbParameter[]</param>
        void AddParameter(DbParameter[] parameters);

        /// <summary>
        /// 移除所有 DbParameter
        /// </summary>
        void ClearParameters();

        /// <summary>
        /// 执行并返回 DbDataReader
        /// </summary>
        /// <returns>DbDataReader</returns>
        DbDataReader ExecuteReader();

        /// <summary>
        /// 执行并返回 DbDataReader
        /// </summary>
        /// <param name="behavior">System.Data.CommandBehavior 值之一。</param>
        /// <returns>DbDataReader</returns>
        DbDataReader ExecuteReader(CommandBehavior behavior);

        /// <summary>
        /// 执行并返回查询结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <returns>object</returns>
        object ExecuteScalar();

        /// <summary>
        /// 对连接对象执行 SQL 语句。
        /// </summary>
        /// <returns>受影响的行数。</returns>
        int ExecuteNonQuery();

        /// <summary>
        /// 在 DataTable 的指定范围中添加或刷新行
        /// </summary>
        /// <param name="dt">用于表映射的 DataTable 的名称</param>
        /// <returns>已在 DataTable 中成功添加或刷新的行数。</returns>
        int Fill(DataTable dt);

        /// <summary>
        /// 在 System.Data.DataSet 中添加或刷新行。
        /// </summary>
        /// <param name="ds">要用记录和架构（如有必要）填充的 DataSet</param>
        /// <returns>已在 DataSet 中成功添加或刷新的行数</returns>
        int Fill(DataSet ds);

        #region 事务
        /// <summary>
        /// 是否开启事务
        /// </summary>
        bool IsTransaction { get; }

        /// <summary>
        /// 开启事务
        /// </summary>
        AfxTransaction BeginTransaction();

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="isolationLevel">事务级别</param>
        AfxTransaction BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();
        #endregion

        /// <summary>
        /// DB 提供程序Factory
        /// </summary>
        DbProviderFactory ProviderFactory { get; }

        /// <summary>
        /// 创建全新 DbConnection
        /// </summary>
        /// <returns>DbConnection</returns>
        DbConnection CreateConnection();

        /// <summary>
        /// 创建全新 DbCommand
        /// </summary>
        /// <returns>DbCommand</returns>
        DbCommand CreateCommand();

        /// <summary>
        /// 创建全新 DbParameter
        /// </summary>
        /// <returns>DbParameter</returns>
        DbParameter CreateParameter();

        /// <summary>
        /// 创建全新 DbParameter
        /// </summary>
        /// <param name="name">ParameterName</param>
        /// <param name="value">Value</param>
        /// <returns>DbParameter</returns>
        DbParameter CreateParameter(string name, object value);

        /// <summary>
        /// 创建全新 DbDataAdapter
        /// </summary>
        /// <returns>DbDataAdapter</returns>
        DbDataAdapter CreateDataAdapter();

        /// <summary>
        /// 参数化查询名称加前缀
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string EncodeParameterName(string name);

        /// <summary>
        /// 列名转义
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        string EncodeColumn(string column);

        /// <summary>
        /// 获取当前local时间
        /// </summary>
        /// <returns></returns>
        DateTime GetLocalNow();
        /// <summary>
        /// 获取当前utc时间
        /// </summary>
        /// <returns></returns>
        DateTime GetUtcNow();
    }
}

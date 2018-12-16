using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Afx.Dapper
{
    public interface IAfxDapper : IDisposable
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
        /// 相关的参数
        /// </summary>
        object Parameters { get; set; }

        /// <summary>
        /// 指示或指定如何解释 CommandText 属性
        /// </summary>
        CommandType? CommandType { get; set; }

        /// <summary>
        /// DbConnection
        /// </summary>
        DbConnection Connection { get; }
        
        /// <summary>
        /// 执行并返回查询结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ExecuteScalar<T>();

        /// <summary>
        /// 对连接对象执行 SQL 语句。
        /// </summary>
        /// <returns>受影响的行数。</returns>
        int ExecuteNonQuery();

        IEnumerable<T> Query<T>();

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
    }
}

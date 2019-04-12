using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace XBCX.Data
{
    public enum DbLikeType
    {
        Left = 1,
        Right = 2,
        All = 3
    }

    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// 资源是否释放
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 转移列名、表名
        /// </summary>
        /// <param name="name">列名、表名</param>
        /// <returns></returns>
        string EscapeName(string name);

        #region Transaction
        /// <summary>
        /// 是否开启事务
        /// </summary>
        bool IsTransaction { get; }

        /// <summary>
        /// 开启事务
        /// </summary>
        IDisposable BeginTransaction();

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        IDisposable BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();
        #endregion

        #region Get

        /// <summary>
        /// 查询表数据
        /// </summary>
        /// <typeparam name="T">数据库表对应model</typeparam>
        /// <param name="whereParam">new { id=10, name= "1"}<</param>
        /// <returns></returns>
        List<T> GetList<T>(object whereParam) where T : class, new();

        /// <summary>
        /// 查询表数据
        /// </summary>
        /// <typeparam name="T">数据库表对应model</typeparam>
        /// <param name="whereSql">whereSql: id = @id OR name = @name </param>
        /// <param name="whereParam">new { id=10, name= "1"}</param>
        /// <returns></returns>
        List<T> GetList<T>(string whereSql, object whereParam) where T : class, new();

        /// <summary>
        /// 查询表一行数据
        /// </summary>
        /// <typeparam name="T">数据库表对应model</typeparam>
        /// <param name="whereParam">不能为空， new { id=10, name= "1"}</param>
        /// <returns></returns>
        T Get<T>(object whereParam) where T : class, new();

        /// <summary>
        /// 查询表一行数据
        /// </summary>
        /// <typeparam name="T">数据库表对应model</typeparam>
        /// <param name="whereSql">不能为空，whereSql: id = @id OR name </param>
        /// <param name="whereParam">new { id=10, name= "1"}</param>
        /// <returns></returns>
        T Get<T>(string whereSql, object whereParam) where T : class, new();
        #endregion

        #region add

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <typeparam name="T">插入表</typeparam>
        /// <param name="m">参数</param>
        /// <param name="ignore">忽略插入列</param>
        /// <returns></returns>
        int Add<T>(T m, params string[] ignore) where T : class;
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="table">插入表</param>
        /// <param name="param"></param>
        /// <returns></returns>
        int Add(string table, object param);

        #endregion

        #region update

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="table">数据表</param>
        /// <param name="setParam">set 参数</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Update(string table, object setParam, object whereParam);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">数据表</typeparam>
        /// <param name="setParam">set 参数</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Update<T>(object setParam, object whereParam) where T : class;

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="table">数据表</param>
        /// <param name="setParam">set 参数</param>
        /// <param name="whereSql">where sql</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Update(string table, object setParam, string whereSql, object whereParam);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">数据表</typeparam>
        /// <param name="setParam">set 参数</param>
        /// <param name="whereSql">where sql</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Update<T>(object setParam, string whereSql, object whereParam) where T : class;

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="table">数据表</param>
        /// <param name="setSql">set sql</param>
        /// <param name="setParam">set 参数</param>
        /// <param name="whereSql">where sql</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Update(string table, string setSql, object setParam, string whereSql, object whereParam);
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">数据表</typeparam>
        /// <param name="setSql">set sql</param>
        /// <param name="setParam">set 参数</param>
        /// <param name="whereSql">where sql</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Update<T>(string setSql, object setParam, string whereSql, object whereParam) where T : class;

        #endregion

        #region delete

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="table">数据表</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Delete(string table, object whereParam);
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">数据表</typeparam>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Delete<T>(object whereParam) where T : class;
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="table">数据表</param>
        /// <param name="whereSql">where sql</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Delete(string table, string whereSql, object whereParam);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">数据表</typeparam>
        /// <param name="whereSql">where sql</param>
        /// <param name="whereParam">where参数</param>
        /// <returns></returns>
        int Delete<T>(string whereSql, object whereParam) where T : class;

        #endregion

        #region common
        /// <summary>
        /// 执行sql，返回影响行数
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="param">sql参数，model or dictionary string object</param>
        /// <returns></returns>
        int Execute(string sql, object param = null);

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param">sql参数，model or dictionary string object</param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object param = null);

        /// <summary>
        /// 执行sql，返回第一行的第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        object ExecuteScalar(string sql, object param = null);

        /// <summary>
        /// 执行sql，返回第一行的第一列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        T ExecuteScalar<T>(string sql, object param = null);
        #endregion

        /// <summary>
        /// 添加匹配符%
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetLikeValue(string value, DbLikeType type = DbLikeType.All);

        /// <summary>
        /// 获取最终排序, order by id, name
        /// </summary>
        /// <typeparam name="T">排序model</typeparam>
        /// <param name="orderby">排序: id desc, name asc</param>
        /// <param name="defaultOrderby">默认排序: id desc</param>
        /// <param name="tb"></param>
        /// <returns></returns>
        string GetOrderby<T>(string orderby, string defaultOrderby, string tb = null) where T : class;

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">查询</typeparam>
        /// <param name="selectCountSql"></param>
        /// <param name="selectDataSql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        (int totalCount, List<T> data) GetPageData<T>(string selectCountSql, string selectDataSql, object param = null);
    }
}

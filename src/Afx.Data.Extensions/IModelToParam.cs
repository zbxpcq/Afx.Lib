using System;

namespace Afx.Data.Extensions
{
    /// <summary>
    /// sql 参数 class 转换接口
    /// </summary>
    public interface IModelToParam
    {
        /// <summary>
        /// sql 参数 class 转换
        /// </summary>
        /// <param name="db">IDatabase</param>
        /// <param name="sql">sql</param>
        /// <param name="obj">sql 参数</param>
        /// <returns></returns>
        string To(IDatabase db, string sql, object obj);
    }
}

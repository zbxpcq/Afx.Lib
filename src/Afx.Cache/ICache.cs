using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Cache
{
    /// <summary>
    /// 缓存接口
    /// </summary>
    public interface ICache : IDisposable
    {
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <returns>不存在返回default(T)</returns>
        T Get<T>(int db, string key);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="value">缓存对象</param>
        bool Set<T>(int db, string key, T value);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="value">缓存对象</param>
        /// <param name="expireIn">过期时间</param>
        bool Set<T>(int db, string key, T value, TimeSpan? expireIn);

        /// <summary>
        /// 延长缓存过期时间
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="expireIn">过期时间</param>
        /// <returns>true：成功，false：失败</returns>
        bool Expire(int db, string key, TimeSpan? expireIn);

        /// <summary>
        /// 是否存在缓存key
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <returns>true：存在，false：不存在</returns>
        bool ContainsKey(int db, string key);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        bool Remove(int db, string key);

        /// <summary>
        /// 批量移除缓存key
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="keys">缓存key list</param>
        bool Remove(int db, List<string> keys);

        /// <summary>
        /// 移除指定db所有缓存
        /// </summary>
        /// <param name="db">缓存所在db</param>
        bool FlushDb(int db);

        /// <summary>
        /// 移除所有缓存
        /// </summary>
        bool FlushAll();

    }
}

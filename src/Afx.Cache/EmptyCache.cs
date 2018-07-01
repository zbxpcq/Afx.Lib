using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Cache
{
    /// <summary>
    /// 空缓存类
    /// </summary>
    public class EmptyCache : ICache
    {
        private static EmptyCache _default;
        /// <summary>
        /// 默认对象
        /// </summary>
        public static EmptyCache Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new EmptyCache();
                }

                return _default;
            }
        }


        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型,始终返回default(T)</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <returns></returns>
        public T Get<T>(int db, string key)
        {
            return default(T);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="value">缓存对象</param>
        public bool Set<T>(int db, string key, T value)
        {

            return true;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="value">缓存对象</param>
        /// <param name="expiresIn">过期时间</param>
        public bool Set<T>(int db, string key, T value, TimeSpan? expiresIn)
        {

            return true;
        }

        /// <summary>
        /// 延长缓存过期时间
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="expireIn">过期时间</param>
        /// <returns>始终返回true</returns>
        public bool Expire(int db, string key, TimeSpan? expireIn)
        {
            return true;
        }

        /// <summary>
        /// 是否存在缓存key
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <returns>始终返回false</returns>
        public bool ContainsKey(int db, string key)
        {
            return false;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        public bool Remove(int db, string key)
        {

            return true;
        }

        /// <summary>
        /// 批量移除缓存key
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="keys">缓存key list</param>
        public bool Remove(int db, List<string> keys)
        {
            return true;
        }

        /// <summary>
        /// 移除指定db所有缓存
        /// </summary>
        /// <param name="db">缓存所在db</param>
        public bool FlushDb(int db)
        {
            return true;
        }

        /// <summary>
        /// 移除所有缓存
        /// </summary>
        public bool FlushAll()
        {
            return true;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}

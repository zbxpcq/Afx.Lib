using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using StackExchange.Redis;

namespace Afx.Cache
{
    /// <summary>
    /// redis缓存对象
    /// </summary>
    public class RedisCache : ICache
    {
        private IConnectionMultiplexer redisMultiplexer;
        /// <summary>
        /// RedisCache
        /// </summary>
        /// <param name="redisMultiplexer"></param>
        public RedisCache(IConnectionMultiplexer redisMultiplexer)
        {
            if (redisMultiplexer == null) throw new ArgumentNullException("redisMultiplexer");
            this.redisMultiplexer = redisMultiplexer;
        }

        private static byte[] ToByteArray<T>(T obj)
        {
            if (obj == null)
                return null;

            if(typeof(T) == typeof(byte[]))
            {
                return obj as byte[];
            }

            if (typeof(T) == typeof(string))
            {
                return Encoding.UTF8.GetBytes(obj as string); 
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        private static T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            if (typeof(T) == typeof(byte[]))
            {
                return (T)((object)data);
            }
            
            string json = Encoding.UTF8.GetString(data);

            if (typeof(T) == typeof(string))
            {
                return (T)((object)json);
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <returns>不存在返回default(T)</returns>
        public virtual T Get<T>(int db, string key)
        {
            T result = default(T);
            if (!string.IsNullOrEmpty(key) && db >= 0)
            {
                var database = this.redisMultiplexer.GetDatabase(db);
                if (database != null)
                {
                    var val = database.StringGet(key);
                    result = FromByteArray<T>(val);
                }
            }

            return result;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="value">缓存对象</param>
        public virtual bool Set<T>(int db, string key, T value)
        {
            return this.Set(db, key, value, null);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="value">缓存对象</param>
        /// <param name="expireIn">过期时间</param>
        public virtual bool Set<T>(int db, string key, T value, TimeSpan? expireIn)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key) && value != null && db >= 0)
            {
                var database = this.redisMultiplexer.GetDatabase(db);
                if (database != null)
                {
                    var data = ToByteArray(value);
                    result = database.StringSet(key, data);
                    if (expireIn.HasValue) result = database.StringSet(key, data, expireIn);
                    else result = database.StringSet(key, data);
                }
            }

            return result;
        }

        /// <summary>
        /// 延长缓存过期时间
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <param name="expireIn">过期时间</param>
        /// <returns>true：成功，false：失败</returns>
        public virtual bool Expire(int db, string key, TimeSpan? expireIn)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key) && db >= 0)
            {
                var database = this.redisMultiplexer.GetDatabase(db);
                if (database != null)
                {
                    result = database.KeyExpire(key, expireIn);
                }
            }

            return result;
        }


        /// <summary>
        /// 是否存在缓存key
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <returns>true：存在，false：不存在</returns>
        public virtual bool ContainsKey(int db, string key)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key) && db >= 0)
            {
                var database = this.redisMultiplexer.GetDatabase(db);
                if (database != null)
                {
                    result = database.KeyExists(key);
                }
            }

            return result;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        public virtual bool Remove(int db, string key)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key) && db >= 0)
            {
                var database = this.redisMultiplexer.GetDatabase(db);
                if (database != null)
                {
                    result = database.KeyDelete(key);
                }
            }

            return result;
        }

        /// <summary>
        /// 批量移除缓存key
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="keys">缓存key list</param>
        public virtual bool Remove(int db, List<string> keys)
        {
            bool result = false;
            if (keys != null && keys.Count > 0 && db >= 0)
            {
                var database = this.redisMultiplexer.GetDatabase(db);
                if (database != null)
                {
                    keys.ForEach(k => { database.KeyDelete(k); });
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 移除指定db所有缓存
        /// </summary>
        /// <param name="db">缓存所在db</param>
        public virtual bool FlushDb(int db)
        {
            bool result = false;
            if (db >= 0)
            {
                var endpoints = this.redisMultiplexer.GetEndPoints(true);
                foreach (var endpoint in endpoints)
                {
                    var server = this.redisMultiplexer.GetServer(endpoint);
                    server.FlushDatabase(db);
                }
                result = true;
            }

            return result;
        }
        
        /// <summary>
        /// 移除所有缓存
        /// </summary>
        public virtual bool FlushAll()
        {
            bool result = false;
            if (!this.IsDisposed)
            {
                    var endpoints = this.redisMultiplexer.GetEndPoints(true);
                    foreach (var endpoint in endpoints)
                    {
                        var server = this.redisMultiplexer.GetServer(endpoint);
                        server.FlushAllDatabases();
                    }
                    result = true;
            }

            return result;
        }
        
        /// <summary>
        /// 是否释放释放对象
        /// </summary>
        public virtual bool IsDisposed { get; private set; }

        /// <summary>
        /// 释放对象，并清除所有缓存
        /// </summary>
        public virtual void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.IsDisposed = true;
                this.redisMultiplexer = null;
            }
        }
        
    }
}

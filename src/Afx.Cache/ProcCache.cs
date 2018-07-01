using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Afx.Cache
{
    /// <summary>
    /// 进程内缓存对象
    /// </summary>
    public class ProcCache : ICache
    {
        class CacheValueModel
        {
            public byte[] data;
            public DateTime? expire;
        }

        private ConcurrentDictionary<string, CacheValueModel>[] dbArray;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="dbCount"></param>
        /// <param name="autoClearTime"></param>
        public ProcCache(int dbCount = 16, int autoClearTime = 30)
        {
            if (dbCount <= 1) dbCount = 1;
            dbArray = new ConcurrentDictionary<string, CacheValueModel>[dbCount];
            for (int i = 0; i < dbArray.Length; i++)
                this.dbArray[i] = new ConcurrentDictionary<string, CacheValueModel>();
            this.IsDisposed = false;
            if (autoClearTime > 0)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(this.ClearExpire, autoClearTime);
            }
        }

        private void ClearExpire(object o)
        {
            int clearTime = ((int)o) * 1000;
            while (true)
            {
                int time = 0;
                while (time < clearTime)
                {
                    System.Threading.Thread.Sleep(50);
                    time += 50;
                    if (this.IsDisposed) return;
                }
                if (this.IsDisposed) return;
                try
                {
                    if (this.dbArray != null && !this.IsDisposed)
                    {
                        var now = DateTime.Now;
                        for (int i = 0; i < dbArray.Length; i++)
                        {
                            var dbCache = this.dbArray[i];
                            if (this.IsDisposed) return;
                            var arr = dbCache.Where(q => q.Value.expire.HasValue && q.Value.expire.Value < now);
                            foreach (var kv in arr)
                            {
                                if (this.IsDisposed) return;
                                CacheValueModel m;
                                dbCache.TryRemove(kv.Key, out m);
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private ConcurrentDictionary<string, CacheValueModel> GetDbCache(int db)
        {
            ConcurrentDictionary<string, CacheValueModel> dbCache = null;
            if (0 <= db && db < this.dbArray.Length)
            {
                dbCache = this.dbArray[db];
            }

            return dbCache;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        /// <returns>不存在返回default(T)</returns>
        public T Get<T>(int db, string key)
        {
            T result = default(T);
            if (!string.IsNullOrEmpty(key))
            {
                var dbCache = this.GetDbCache(db);
                if (dbCache != null)
                {
                    CacheValueModel m;
                    if (dbCache.TryGetValue(key, out m))
                    {
                        if(m.expire.HasValue && m.expire.Value > DateTime.Now)
                        {
                            dbCache.TryRemove(key, out m);
                            return result;
                        }
                        if (m.data != null)
                        {
                            if (typeof(T) == typeof(byte[]))
                            {
                                result = (T)((object)m.data);
                            }
                            else if (m.data != null && m.data.Length > 0)
                            {
                                string s = Encoding.UTF8.GetString(m.data);
                                if (typeof(T) == typeof(string))
                                {
                                    result = (T)((object)s);
                                }
                                else
                                {
                                    result = JsonConvert.DeserializeObject<T>(s);
                                }
                            }
                        }
                    }
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
        public bool Set<T>(int db, string key, T value)
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
        public bool Set<T>(int db, string key, T value, TimeSpan? expireIn)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key) && value != null)
            {
                byte[] buffer = null;
                if (value is byte[])
                {
                    buffer = (byte[])((object)value);
                }
                else if (value is string)
                {
                    buffer = Encoding.UTF8.GetBytes((string)((object)value));
                }
                else
                {
                    string json = JsonConvert.SerializeObject(value);
                    buffer = Encoding.UTF8.GetBytes(json);
                }

                var dbCache = this.GetDbCache(db);
                if (dbCache != null)
                {
                    CacheValueModel m;
                    bool isadd = false;
                    if (!dbCache.TryGetValue(key, out m))
                    {
                        m = new CacheValueModel();
                        isadd = true;
                    }
                    m.data = buffer;
                    if (expireIn.HasValue) m.expire = DateTime.Now.Add(expireIn.Value);
                    if(isadd) dbCache[key] = m;
                    result = true;
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
        public bool Expire(int db, string key, TimeSpan? expireIn)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key))
            {
                var dbCache = this.GetDbCache(db);
                if (dbCache != null)
                {
                    CacheValueModel m;
                    if (dbCache.TryGetValue(key, out m))
                    {
                        if (!expireIn.HasValue) m.expire = null;
                        else m.expire = DateTime.Now.Add(expireIn.Value);
                        result = true;
                    }
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
        public bool ContainsKey(int db, string key)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key))
            {
                var dbCache = this.GetDbCache(db);
                if (dbCache != null)
                {
                    result = dbCache.ContainsKey(key);
                }
            }

            return result;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="key">缓存key</param>
        public bool Remove(int db, string key)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key))
            {
                var dbCache = this.GetDbCache(db);
                if (dbCache != null)
                {
                    CacheValueModel m;
                    dbCache.TryRemove(key, out m);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 批量移除缓存key
        /// </summary>
        /// <param name="db">缓存所在db</param>
        /// <param name="keys">缓存key list</param>
        public bool Remove(int db, List<string> keys)
        {
            bool result = false;
            if (keys != null && keys.Count > 0)
            {
                var dbCache = this.GetDbCache(db);
                if (dbCache != null)
                {
                    if (dbCache.Count > 0)
                    {
                        CacheValueModel m;
                        keys.ForEach(item => dbCache.TryRemove(item, out m));
                    }
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 移除指定db所有缓存
        /// </summary>
        /// <param name="db">缓存所在db</param>
        public bool FlushDb(int db)
        {
            bool result = false;
            var dbCache = this.GetDbCache(db);
            if (dbCache != null)
            {
                dbCache.Clear();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 移除所有缓存
        /// </summary>
        public bool FlushAll()
        {
            bool result = false;
            if (!this.IsDisposed)
            {
                for (var i = 0; i < this.dbArray.Length; i++)
                {
                    this.dbArray[i].Clear();
                }
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 是否释放释放对象
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 释放对象，并清除所有缓存
        /// </summary>
        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.IsDisposed = true;
                if (this.dbArray != null)
                {
                    for (int i = 0; i < this.dbArray.Length; i++)
                    {
                        if (this.dbArray[i] != null) this.dbArray[i].Clear();
                        this.dbArray[i] = null;
                    }
                }
                this.dbArray = null;
             }
        }

        /// <summary>
        /// Close
        /// </summary>
        public void Close()
        {
            
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Afx.Threading;

namespace Afx.Collections
{
    /// <summary>
    /// 多线程安全的 Dictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class SafeDictionary<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> m_dic;
        private ReadWriteLock m_readWriteLock;
        /// <summary>
        /// SafeDictionary
        /// </summary>
        public SafeDictionary()
            : this(-1, null)
        {
        }
        /// <summary>
        /// SafeDictionary
        /// </summary>
        /// <param name="capacity"></param>
        public SafeDictionary(int capacity)
            : this(capacity, null)
        {
        }
        /// <summary>
        /// SafeDictionary
        /// </summary>
        /// <param name="comparer"></param>
        public SafeDictionary(IEqualityComparer<TKey> comparer)
            : this(-1, comparer)
        {
        }
        /// <summary>
        /// SafeDictionary
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="comparer"></param>
        public SafeDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (comparer != null)
            {
                this.m_dic = capacity >= 0
                    ? new Dictionary<TKey, TValue>(capacity, comparer)
                    : new Dictionary<TKey, TValue>(comparer);
            }
            else
            {
                this.m_dic = capacity >= 0
                    ? new Dictionary<TKey, TValue>(capacity)
                    : new Dictionary<TKey, TValue>();
            }

            this.m_readWriteLock = new ReadWriteLock();
            this.IsDisposed = false;
        }
        /// <summary>
        /// Add
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(TKey key, TValue value)
        {
            return this.TryAdd(key, value);
        }
        /// <summary>
        /// ContainsKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            bool result = false;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_dic.ContainsKey(key);
            }

            return result;
        }
        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            bool result = false;
            using (this.m_readWriteLock.GetWriteLock())
            {
                result = this.m_dic.Remove(key);
            }

            return result;
        }
        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue GetValue(TKey key)
        {
            TValue val = default(TValue);
            using (this.m_readWriteLock.GetReadLock())
            {
                this.m_dic.TryGetValue(key, out val);
            }

            return val;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            bool result = false;
            using (this.m_readWriteLock.GetWriteLock())
            {
                if (!this.m_dic.ContainsKey(key))
                {
                    this.m_dic.Add(key, value);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// TryGetValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result = false;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_dic.TryGetValue(key, out value);
            }

            return result;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(TKey key, TValue value)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_dic[key] = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                return this.GetValue(key);
            }
            set
            {
                this.SetValue(key, value);
            }
        }
        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_dic.Clear();
            }
        }
        /// <summary>
        /// Count
        /// </summary>
        public int Count
        {
            get
            {
                int count = this.m_dic.Count;

                return count;
            }
        }
        /// <summary>
        /// Keys
        /// </summary>
        public List<TKey> Keys
        {
            get
            {
                List<TKey> list = null;
                using (this.m_readWriteLock.GetReadLock())
                {
                    list = new List<TKey>(this.m_dic.Keys.Count);
                    foreach (var k in this.m_dic.Keys)
                    {
                        list.Add(k);
                    }
                }

                return list;
            }
        }
        /// <summary>
        /// GetAll
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<TKey, TValue>[] GetAll()
        {
            KeyValuePair<TKey, TValue>[] arr = null;
            using (this.m_readWriteLock.GetReadLock())
            {
                arr = new KeyValuePair<TKey, TValue>[this.m_dic.Count];
                int index = 0;
                foreach (var kv in this.m_dic)
                {
                    arr[index++] = kv;
                }
            }

            return arr;
        }
        /// <summary>
        /// IsDisposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.IsDisposed = true;
                var _dic = this.m_dic;
                var _readerWriterLock = this.m_readWriteLock;

                this.m_dic = null;
                this.m_readWriteLock = null;

                try { if (this.m_dic != null) this.m_dic.Clear(); }
                catch { }
                try { if (this.m_readWriteLock != null) this.m_readWriteLock.Dispose(); }
                catch { }
            }
        }
        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }
        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private KeyValuePair<TKey, TValue>[] arr;
            private KeyValuePair<TKey, TValue> current;
            private int index = 0;

            public Enumerator(SafeDictionary<TKey, TValue> dic)
            {
                this.arr = dic.GetAll();
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get { return this.current; }
            }

            public void Dispose()
            {
                arr = null;
            }

            object IEnumerator.Current
            {
                get { return this.current; }
            }

            public bool MoveNext()
            {
                if (this.index < this.arr.Length)
                {
                    this.current = this.arr[this.index++];
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                this.index = 0;
                this.current = default(KeyValuePair<TKey, TValue>);
            }
        }
    }

}

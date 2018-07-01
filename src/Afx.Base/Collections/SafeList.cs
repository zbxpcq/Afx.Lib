using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Afx.Threading;

namespace Afx.Collections
{
    /// <summary>
    /// 多线程安全的list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SafeList<T> : IDisposable, IEnumerable<T>, IList<T>
    {
        private List<T> m_list;
        private ReadWriteLock m_readWriteLock;
        /// <summary>
        /// SafeList
        /// </summary>
        public SafeList()
            : this(null, -1)
        {
        }
        /// <summary>
        /// SafeList
        /// </summary>
        /// <param name="capacity"></param>
        public SafeList(int capacity)
            : this(null, capacity)
        {
        }
        /// <summary>
        /// SafeList
        /// </summary>
        /// <param name="collection"></param>
        public SafeList(IEnumerable<T> collection)
            : this(collection, -1)
        {
        }
        /// <summary>
        /// SafeList
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="capacity"></param>
        private SafeList(IEnumerable<T> collection, int capacity)
        {
            if (collection != null)
            {
                this.m_list = new List<T>(collection);
            }
            else if (capacity >= 0)
            {
                this.m_list = new List<T>(capacity);
            }
            else
            {
                this.m_list = new List<T>();
            }
            this.m_readWriteLock = new ReadWriteLock();
            this.IsDisposed = false;
        }

        /// <summary>
        /// 实际包含的元素数。
        /// </summary>
        public int Count { get { return this.m_list.Count; } }

        /// <summary>
        /// 获取或设置该内部数据结构在不调整大小的情况下能够容纳的元素总数, 返回结果:在需要调整大小之前 System.Collections.Generic.List 能够容纳的元素的数目。 
        /// </summary>
        public int Capacity
        {
            get
            {
                int capacity = 0;
                using (this.m_readWriteLock.GetReadLock())
                {
                    capacity = this.m_list.Capacity;
                }
                return capacity;
            }
            set
            {
                using (this.m_readWriteLock.GetWriteLock())
                {
                    this.m_list.Capacity = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置位于指定索引处的元素。
        /// </summary>
        /// <param name="index">要获得或设置的元素从零开始的索引。</param>
        /// <returns>位于指定索引处的元素。</returns>
        public T this[int index]
        {
            get
            {
                T val = default(T);
                using (this.m_readWriteLock.GetReadLock())
                {
                    val = this.m_list[index];
                }
                return val;
            }
            set
            {
                using (this.m_readWriteLock.GetWriteLock())
                {
                    this.m_list[index] = value;
                }
            }
        }

        /// <summary>
        /// 将对象添加的结尾处
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Add(item);
            }
        }

        /// <summary>
        /// 将对象集合添加的结尾处
        /// </summary>
        /// <param name="collection"></param>
        public void AddRange(IEnumerable<T> collection)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.AddRange(collection);
            }
        }
        /// <summary>
        /// BinarySearch
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int BinarySearch(T item)
        {
            int index = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                index = this.m_list.BinarySearch(item);
            }

            return index;
        }
        /// <summary>
        /// BinarySearch
        /// </summary>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            int index = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                index = this.m_list.BinarySearch(item, comparer);
            }

            return index;
        }
        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Clear();
            }
        }
        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            bool result = false;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.Contains(item);
            }

            return result;
        }
        /// <summary>
        /// CopyTo
        /// </summary>
        /// <param name="array"></param>
        public void CopyTo(T[] array)
        {
            using (this.m_readWriteLock.GetReadLock())
            {
                this.m_list.CopyTo(array);
            }
        }
        /// <summary>
        /// CopyTo
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (this.m_readWriteLock.GetReadLock())
            {
                this.m_list.CopyTo(array, arrayIndex);
            }
        }
        /// <summary>
        /// CopyTo
        /// </summary>
        /// <param name="index"></param>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <param name="count"></param>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            using (this.m_readWriteLock.GetReadLock())
            {
                this.m_list.CopyTo(index, array, arrayIndex, count);
            }
        }
        /// <summary>
        /// Exists
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool Exists(Predicate<T> match)
        {
            bool result = false;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.Exists(match);
            }

            return result;
        }
        /// <summary>
        /// Find
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T Find(Predicate<T> match)
        {
            T result = default(T);
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.Find(match);
            }

            return result;
        }
        /// <summary>
        /// FindAll
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<T> FindAll(Predicate<T> match)
        {
            List<T> result = null;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.FindAll(match);
            }

            return result;
        }
        /// <summary>
        /// FindIndex
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(Predicate<T> match)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.FindIndex(match);
            }

            return result;
        }
        /// <summary>
        /// FindIndex
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.FindIndex(startIndex, match);
            }

            return result;
        }
        /// <summary>
        /// FindIndex
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.FindIndex(startIndex, count, match);
            }

            return result;
        }
        /// <summary>
        /// FindLast
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T FindLast(Predicate<T> match)
        {
            T result = default(T);
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.FindLast(match);
            }

            return result;
        }
        /// <summary>
        /// FindLastIndex
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(Predicate<T> match)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.FindLastIndex(match);
            }

            return result;
        }
        /// <summary>
        /// FindLastIndex
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.FindLastIndex(startIndex, match);
            }

            return result;
        }
        /// <summary>
        /// FindLastIndex
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.FindLastIndex(startIndex, count, match);
            }

            return result;
        }
        /// <summary>
        /// ForEach
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<T> action)
        {
            T[] arr = this.GetAll();
            foreach (var item in arr)
            {
                action(item);
            }
        }
        /// <summary>
        /// GetRange
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> GetRange(int index, int count)
        {
            List<T> result = null;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.GetRange(index, count);
            }

            return result;
        }
        /// <summary>
        /// IndexOf
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.IndexOf(item);
            }

            return result;
        }
        /// <summary>
        /// IndexOf
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int IndexOf(T item, int index)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.IndexOf(item, index);
            }

            return result;
        }
        /// <summary>
        /// IndexOf
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int IndexOf(T item, int index, int count)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.IndexOf(item, index, count);
            }

            return result;
        }
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Insert(index, item);
            }
        }
        /// <summary>
        /// InsertRange
        /// </summary>
        /// <param name="index"></param>
        /// <param name="collection"></param>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.InsertRange(index, collection);
            }
        }
        /// <summary>
        /// LastIndexOf
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int LastIndexOf(T item)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.LastIndexOf(item);
            }

            return result;
        }
        /// <summary>
        /// LastIndexOf
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int LastIndexOf(T item, int index)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.LastIndexOf(item, index);
            }

            return result;
        }
        /// <summary>
        /// LastIndexOf
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int LastIndexOf(T item, int index, int count)
        {
            int result = -1;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.LastIndexOf(item, index, count);
            }

            return result;
        }
        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            bool result = false;
            using (this.m_readWriteLock.GetWriteLock())
            {
               result= this.m_list.Remove(item);
            }
            return result;
        }
        /// <summary>
        /// RemoveAll
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int RemoveAll(Predicate<T> match)
        {
            int result = 0;
            using (this.m_readWriteLock.GetWriteLock())
            {
                result = this.m_list.RemoveAll(match);
            }

            return result;
        }
        /// <summary>
        /// RemoveAt
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.RemoveAt(index);
            }
        }
        /// <summary>
        /// RemoveRange
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void RemoveRange(int index, int count)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.RemoveRange(index, count);
            }
        }
        /// <summary>
        /// Reverse
        /// </summary>
        public void Reverse()
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Reverse();
            }
        }
        /// <summary>
        /// Reverse
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void Reverse(int index, int count)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Reverse(index, count);
            }
        }
        /// <summary>
        /// Sort
        /// </summary>
        public void Sort()
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Sort();
            }
        }
        /// <summary>
        /// Sort
        /// </summary>
        /// <param name="comparison"></param>
        public void Sort(Comparison<T> comparison)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Sort(comparison);
            }
        }
        /// <summary>
        /// Sort
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(IComparer<T> comparer)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Sort(comparer);
            }
        }
        /// <summary>
        /// Sort
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="comparer"></param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.Sort(index, count,comparer);
            }
        }
        /// <summary>
        /// ToArray
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            T[] arr = null;
            using (this.m_readWriteLock.GetReadLock())
            {
                arr = this.m_list.ToArray();
            }

            return arr;
        }
        /// <summary>
        /// TrimExcess
        /// </summary>
        public void TrimExcess()
        {
            using (this.m_readWriteLock.GetWriteLock())
            {
                this.m_list.TrimExcess();
            }
        }
        /// <summary>
        /// TrueForAll
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool TrueForAll(Predicate<T> match)
        {
            bool result = false;
            using (this.m_readWriteLock.GetReadLock())
            {
                result = this.m_list.TrueForAll(match);
            }

            return result;
        }
        /// <summary>
        /// GetAll
        /// </summary>
        /// <returns></returns>
        public T[] GetAll()
        {
            T[] arr = null;
            using (this.m_readWriteLock.GetReadLock())
            {
                arr = this.m_list.ToArray();
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
                try { this.m_list.Clear(); }
                catch { }
                try { this.m_readWriteLock.Dispose(); }
                catch { }
                this.m_list = null;
                this.m_readWriteLock = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        /// <summary>
        /// 
        /// </summary>
        class Enumerator : IEnumerator<T>
        {
            private T[] arr;
            private T current;
            private int index = 0;

            public Enumerator(SafeList<T> list)
            {
                this.arr = list.GetAll();
            }

            public T Current
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
                this.current = default(T);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}

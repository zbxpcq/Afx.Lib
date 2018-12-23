using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Afx.Threading;

namespace Afx.Collections
{
    public struct ForeachResult
    {
        public bool IsStop;
        public bool IsDelete;

        public ForeachResult(bool isStop) : this(isStop, false) { }

        public ForeachResult(bool isStop, bool isDelete)
        {
            this.IsStop = isStop;
            this.IsDelete = isDelete;
        }

        public static implicit operator ForeachResult(bool isStop)
        {
            return new ForeachResult(isStop);
        }
    }

    public delegate ForeachResult NodeFunc<T>(T item, object state);

    /// <summary>
    /// CAS锁单链表
    ///  add by jerrylai@aliyun.com
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeList<T>
    {
        private int m_count = 0;
        private Node<T> m_head;

        public int Count { get { return m_count; } }

        /// <summary>
        /// 以原子方式添加到第一个
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            Node<T> node = new Node<T>(value, this.m_head);
            var old = node.Next;
            while (Interlocked.CompareExchange(ref this.m_head, node, old) != old)
            {
                Thread.SpinWait(5);
                node.Next = this.m_head;
                old = node.Next;
            }
            Interlocked.Increment(ref this.m_count);
        }

        /// <summary>
        /// 以原子方式遍历， func返回: true.删除当前value,false.不做任何操作
        /// </summary>
        /// <param name="func"></param>
        /// <param name="state"></param>
        public void Foreach(NodeFunc<T> func, object state = null)
        {
            if (func == null) throw new ArgumentNullException("func");
            Exception ex = null;
            Node<T> prev = this.m_head;
            var curr = prev;
            ForeachResult result = false;
            while (!result.IsStop && curr != null)
            {
                try
                {
                    result = func(curr.Data, state);
                }
                catch (Exception e)
                {
                    ex = e;
                    break;
                }

                if (result.IsDelete)
                {
                    if (prev == curr)
                    {
                        if (Interlocked.CompareExchange(ref this.m_head, curr.Next, prev) == prev)
                        {
                            Interlocked.Decrement(ref this.m_count);
                            prev = curr.Next;
                        }
                    }
                    else
                    {
                        var old = prev.Next;
                        if (Interlocked.CompareExchange(ref prev.Next, curr.Next, old) == old)
                        {
                            Interlocked.Decrement(ref this.m_count);
                        }
                    }
                }
                else
                {
                    if (prev != curr) prev = curr;
                }
                curr = curr.Next;
            }
            if (ex != null) throw ex;
        }

        public List<T> GetAll()
        {
            List<T> list = new List<T>(this.Count);
            this.Foreach((item, state) =>
            {
                list.Add(item);

                return false;
            });
            return list;
        }
    }
}

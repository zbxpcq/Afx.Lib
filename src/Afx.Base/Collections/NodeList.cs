using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
    /// 无锁栈
    ///  add by jerrylai@aliyun.com
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeList<T>
    {
        class Node
        {
            public T Data;
            public Node Next;

            public Node() { }

            public Node(T data) : this(data, null) { }

            public Node(T data, Node next)
            {
                this.Data = data;
                this.Next = next;
            }
        }

        private int m_count = 0;
        private Node m_head;

        public int Count { get { return m_count; } }

        /// <summary>
        /// 以原子方式添加到第一个
        /// </summary>
        /// <param name="value"></param>
        public void Push(T value)
        {
            Node node = new Node(value);
            node.Next = this.m_head;
            while (Interlocked.CompareExchange(ref this.m_head, node, node.Next) != node.Next)
            {
                node.Next = this.m_head;
            }
            Interlocked.Increment(ref this.m_count);
        }

        /// <summary>
        /// 以原子方式遍历
        /// </summary>
        /// <param name="func"></param>
        /// <param name="state"></param>
        public void Foreach(NodeFunc<T> func, object state = null)
        {
            if (func == null) throw new ArgumentNullException("func");
            Node prev = this.m_head;
            var curr = prev;
            ForeachResult result = false;
            while (curr != null)
            {
                if ((result = func(curr.Data, state)).IsDelete)
                {
                    if (prev == curr)
                    {
                        if (Interlocked.CompareExchange(ref this.m_head, curr.Next, prev) == prev)
                        {
                            Interlocked.Decrement(ref this.m_count);
                            prev = curr.Next;
                        }
                        else
                        {
                            prev = this.GetPrevNode(curr);
                            if (prev != null)
                            {
                                if (Interlocked.CompareExchange(ref prev.Next, curr.Next, curr) == curr)
                                {
                                    Interlocked.Decrement(ref this.m_count);
                                }
                            }
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
                else if (prev != curr)
                {
                    prev = curr;
                }

                if (result.IsStop) break;
                curr = curr.Next;
            }
        }

        private Node GetPrevNode(Node node)
        {
            Node prev = null;
            var curr = this.m_head;
            while (curr != null)
            {
                if (curr == node) return prev;
                prev = curr;
                curr = curr.Next;
            }

            return null;
        }

        public List<T> GetAll()
        {
            List<T> list = new List<T>(this.Count);
            Node curr = this.m_head;
            while (curr != null)
            {
                list.Add(curr.Data);
                curr = curr.Next;
            }
            return list;
        }
    }
}

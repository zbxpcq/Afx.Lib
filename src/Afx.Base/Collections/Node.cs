using System;
using System.Collections.Generic;
using System.Text;
using Afx.Threading;

namespace Afx.Collections
{
    internal class Node<T>
    {
        public T Data;
        public Node<T> Next;

        public Node() { }

        public Node(T data) : this(data, null) { }

        public Node(T data, Node<T> next)
        {
            this.Data = data;
            this.Next = next;
        }
    }
}

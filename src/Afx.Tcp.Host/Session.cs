using System;
using System.Collections.Generic;
using System.Text;

using Afx.Tcp.Protocols;
using Afx.Sockets;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// Session
    /// </summary>
    public class Session : IDisposable
    {
        /// <summary>
        /// Sid
        /// </summary>
        public virtual string Sid { get; private set; }
        /// <summary>
        /// client Address
        /// </summary>
        public virtual string Address { get; private set; }

        internal ITcpClientAsync Client { get; set; }
        /// <summary>
        /// IsConnected
        /// </summary>
        public virtual bool IsConnected
        {
            get
            {
                return this.Client != null && this.Client.IsConnected;
            }
        }

        /// <summary>
        /// 最后接收数据时间
        /// </summary>
        public virtual DateTime LastReceiveTime { get; internal set; }

        private Dictionary<string, object> dic;

        /// <summary>
        /// Session数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object this[string key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                this.Set(key, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object Get(string key)
        {
            object o = null;
            this.dic.TryGetValue(key, out o);
            return o;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public virtual void Set(string key, object obj)
        {
            if (!string.IsNullOrEmpty(key))
            {
                this.dic[key] = obj;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Clear()
        {
            this.dic.Clear();
        }

        internal Action<MsgData, Session> SendCall;
        /// <summary>
        /// 向client发送消息
        /// </summary>
        /// <param name="msg"></param>
        public virtual void Send(MsgData msg)
        {
            if(this.SendCall != null)
            {
                this.SendCall(msg, this);
            }
        }

        internal Action<Session> CloseCall;
        /// <summary>
        /// 关闭client
        /// </summary>
        public virtual void Close()
        {
            if (this.CloseCall != null)
            {
                this.CloseCall(this);
            }
        }

        internal Session(ITcpClientAsync client)
        {
            client.SetKeepAlive(15 * 1000, 15 * 1000);
            string address = client.RemoteEndPoint.ToString();
            int index = address.IndexOf(':');
            if (index > 0) address = address.Substring(0, index);

            this.Client = client;
            this.Sid = Guid.NewGuid().ToString("n");
            this.Address = address;
            this.dic = new Dictionary<string, object>();
            this.LastReceiveTime = DateTime.Now;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            this.Sid = "";
            this.Clear();
            this.dic = null;
            this.Client = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

namespace Afx.Sockets
{
    /// <summary>
    /// tcp server
    /// </summary>
    public sealed class TcpServer : IDisposable
    {
        private Socket socket = null;
        private volatile bool isAccept = false;
        private volatile bool isDisposed = false;
        
        /// <summary>
        /// 监听客户端回调
        /// </summary>
        public event TcpAcceptEvent AcceptEvent = null;
        private void OnAcceptEvent(TcpSocketAsync client)
        {
            if(this.AcceptEvent != null)
            {
                this.AcceptEvent(client);
            }
        }
        /// <summary>
        /// 异常回调
        /// </summary>
        public event TcpServerErrorEvent ServerErrorEvent = null;
        private void OnServerErrorEvent(Exception ex)
        {
            if (this.ServerErrorEvent != null)
            {
                this.ServerErrorEvent(this, ex);
            }
        }
        /// <summary>
        /// 客户端接收数据回调
        /// </summary>
        public event TcpReceiveEvent ClientReceiveEvent;
        private void OnClientReceiveEvent(TcpSocketAsync client, byte[] data, int length)
        {
            if (this.ClientReceiveEvent != null)
            {
                this.ClientReceiveEvent(client, data, length);
            }
        }
        /// <summary>
        /// 客户端异常回调
        /// </summary>
        public event TcpErrorEvent ClientErrorEvent;
        private void OnClientErrorEvent(TcpSocketAsync client, Exception ex)
        {
            if (this.ClientErrorEvent != null)
            {
                this.ClientErrorEvent(client, ex);
            }
        }
        /// <summary>
        /// 客户端正在接收数据回调
        /// </summary>
        public event TcpReadingEvent ClientReadingEvent;
        private void OnClientReadingEvent(TcpSocketAsync client, int position, int length)
        {
            if (this.ClientReadingEvent != null)
            {
                this.ClientReadingEvent(client, position, length);
            }
        }

        /// <summary>
        /// 是否在监听
        /// </summary>
        public bool IsAccept
        {
            get
            {
                return this.isAccept;
            }
        }

        /// <summary>
        /// 是否释放
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return this.isDisposed;
            }
        }

        //private bool exclusiveAddressUse = true;
        ///// <summary>
        ///// 指定 Socket 是否仅允许一个进程绑定到端口
        ///// </summary>
        //public bool ExclusiveAddressUse
        //{
        //    get
        //    {
        //        return this.exclusiveAddressUse;
        //    }
        //    set
        //    {
        //        this.exclusiveAddressUse = value;
        //        if (isAccept && this.socket != null)
        //            this.socket.ExclusiveAddressUse = value;
        //    }
        //}

        /// <summary>
        /// 监听 socket Handle，未调用Start之前值为IntPtr.Zero
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                if (this.socket != null)
                    return this.socket.Handle;

                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 监听 socket，未调用Start之前值为null
        /// </summary>
        public Socket Socket
        {
            get
            {
                return this.socket;
            }
        }

        /// <summary>
        /// 发送Buffer缓存大小
        /// </summary>
        public int SendBufferSize { get; private set; }
        /// <summary>
        /// 接收数据Buffer缓存大小
        /// </summary>
        public int ReceiveBufferSize { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sendBufferSize">16 &lt;=  sendBufferSize &lt;= 8 * 1024</param>
        /// <param name="receiveBufferSize">16 &lt;=  receiveBufferSize &lt;= 8 * 1024</param>
        public TcpServer(int sendBufferSize = 8 * 1024, int receiveBufferSize = 8 * 1024)
        {
            if (sendBufferSize <= 0) throw new ArgumentException("sendBufferSize is error!");
            if (receiveBufferSize <= 0) throw new ArgumentException("receiveBufferSize is error!");
            int max = 8 * 1024;
            int min = 16;
            this.SendBufferSize = sendBufferSize < min ? min : (sendBufferSize > max ? max : sendBufferSize);
            this.ReceiveBufferSize = receiveBufferSize < min ? min : (receiveBufferSize > max ? max : receiveBufferSize);
        }

        /// <summary>
        /// 启动监听，端口占用异常
        /// </summary>
        /// <param name="port">本地端口号</param>
        public void Start(int port)
        {
            this.Start(IPAddress.Any, port);
        }
        /// <summary>
        /// 启动监听，端口占用异常
        /// </summary>
        /// <param name="ipAddress">监听ip</param>
        /// <param name="port">本地端口号</param>
        public void Start(IPAddress ipAddress, int port)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //this.socket.ExclusiveAddressUse = this.exclusiveAddressUse;
            //this.socket.IOControl(IOControlCode.KeepAliveValues, Helper.GetTcpKeepAlive(60 * 1000, 60 * 1000), null);
            EndPoint local = new IPEndPoint(ipAddress, port);
            this.socket.Bind(local);
            this.socket.Listen(ushort.MaxValue);
            this.isAccept = true;
            ThreadPool.QueueUserWorkItem(this.BeginAccept);
        }
        
        private void BeginAccept(object obj)
        {
            try
            {
                while (this.isAccept)
                {
                    Socket s = this.socket.Accept();
                    var client = new TcpSocketAsync(s, this.SendBufferSize, this.ReceiveBufferSize);
                    client.ErrorEvent += this.OnClientErrorEvent;
                    client.ReceiveEvent += this.OnClientReceiveEvent;
                    client.ReadingEvent += this.OnClientReadingEvent;
                    this.OnAcceptEvent(client);
                    client.BeginReceive();
                }
            }
            catch (Exception ex)
            {
                this.OnServerErrorEvent(ex);
            }
        }
        
        /// <summary>
        /// 关闭服务端监听
        /// </summary>
        public void Close()
        {
            if (this.isAccept)
            {
                this.isAccept = false;
                if (this.socket != null)// && this.socket.Connected)
                {
                    this.socket.Close();
                }
            }
        }
        
        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.Close();
                this.socket = null;
                this.AcceptEvent = null;
                this.ClientErrorEvent = null;
                this.ClientReadingEvent = null;
                this.ClientReceiveEvent = null;
                this.ServerErrorEvent = null;
            }
        }
    }
}

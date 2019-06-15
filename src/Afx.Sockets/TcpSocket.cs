using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Afx.Sockets.Common;

namespace Afx.Sockets
{
    /// <summary>
    /// TcpSocket
    /// </summary>
    public sealed class TcpSocket : IDisposable
    {
        private Socket socket = null;
        private volatile bool isDisposed = false;

        /// <summary>
        /// 用户定义的对象
        /// </summary>
        public object UserState { get; set; }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.socket != null && this.socket.Connected;
            }
        }

        /// <summary>
        /// socket Handle，未调用Connect之前值为IntPtr.Zero
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// LocalEndPoint
        /// </summary>
        public EndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// RemoteEndPoint
        /// </summary>
        public EndPoint RemoteEndPoint { get; private set; }

        /// <summary>
        /// SendTimeout（毫秒）
        /// </summary>
        public int SendTimeout { get; private set; }

        /// <summary>
        /// ReceiveTimeout（毫秒）
        /// </summary>
        public int ReceiveTimeout { get; private set; }

        /// <summary>
        /// Socket
        /// </summary>
        public Socket Socket
        {
            get
            {
                return this.socket;
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
        //        if (this.socket != null)
        //            this.socket.ExclusiveAddressUse = value;
        //    }
        //}

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

        /// <summary>
        /// SendBufferSize 
        /// </summary>
        public int SendBufferSize { get; private set; }

        /// <summary>
        /// ReceiveBufferSize
        /// </summary>
        public int ReceiveBufferSize { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sendBufferSize"></param>
        /// <param name="receiveBufferSize"></param>
        /// <param name="sendTimeout">（毫秒）</param>
        /// <param name="receiveTimeout">（毫秒）</param>
        public TcpSocket(int sendBufferSize = 8 * 1024, int receiveBufferSize = 8 * 1024,
            int sendTimeout = 0, int receiveTimeout = 0)
        {
            if (sendBufferSize < 0) throw new ArgumentException("sendBufferSize is error!");
            if (receiveBufferSize < 0) throw new ArgumentException("receiveBufferSize is error!");
            if (sendTimeout < 0) throw new ArgumentException("sendTimeout is error!");
            if (receiveTimeout < 0) throw new ArgumentException("receiveTimeout is error!");
            int max = 10 * 1024 * 1024;
            int min = 16;
            this.SendBufferSize = sendBufferSize < min ? min : (sendBufferSize > max ? max : sendBufferSize);
            this.ReceiveBufferSize = receiveBufferSize < min ? min : (receiveBufferSize > max ? max : receiveBufferSize);
            this.SendTimeout = sendTimeout < -1 ? -1 : sendTimeout;
            this.ReceiveTimeout = receiveTimeout < -1 ? -1 : receiveTimeout;
        }

        private volatile bool isStartConnect = false;
        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="host">服务端ip或域名</param>
        /// <param name="port">服务端端口</param>
        /// <param name="millisecondsTimeout">（毫秒）</param>
        public bool Connect(string host, int port, int millisecondsTimeout = 0)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if (millisecondsTimeout < 0) throw new ArgumentException("millisecondsTimeout is error!");
            if (port < IPEndPoint.MinPort || port >= IPEndPoint.MaxPort) throw new ArgumentException("port is error!");
            bool result = !this.IsDisposed && this.socket != null && this.socket.Connected;
            if (!this.IsDisposed && (this.socket == null || !this.socket.Connected))
            {
                try
                {
                    this.isStartConnect = true;
                    this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.socket.ReceiveTimeout = this.ReceiveTimeout;
                    this.socket.SendTimeout = this.SendTimeout;
                    this.socket.SendBufferSize = this.SendBufferSize;
                    this.socket.ReceiveBufferSize = this.ReceiveBufferSize;
                    //this.socket.ExclusiveAddressUse = this.exclusiveAddressUse;
                    if (millisecondsTimeout > 0)
                    {
                        var asyncResult = this.socket.BeginConnect(host, port, new AsyncCallback((o) =>
                        {
                            try
                            {
                                this.socket.EndConnect(o);
                                if (this.IsConnected)
                                {
                                    ConnectSucess();
                                }
                            }
                            catch { }
                            this.isStartConnect = false;
                        }), null);

                        if (asyncResult.AsyncWaitHandle.WaitOne(millisecondsTimeout < 500 ? 500 : millisecondsTimeout))
                        {
                            result = this.IsConnected;
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                    else
                    {
                        this.socket.Connect(host, port);
                        ConnectSucess();
                        result = true;
                    }
                }
                catch //(Exception ex)
                {
                    this.isStartConnect = false;
                }
            }

            return result;
        }

        private void ConnectSucess()
        {
            this.isStartConnect = false;
            this.Handle = this.socket.Handle;
            this.LocalEndPoint = this.socket.LocalEndPoint;
            this.RemoteEndPoint = this.socket.RemoteEndPoint;
        }

        /// <summary>
        /// KeepAlive
        /// </summary>
        /// <param name="keepAliveTime">连接多长时间（毫秒）没有数据就开始发送心跳包，有数据传递的时候不发送心跳包</param>
        /// <param name="keepAliveInterval">每隔多长时间（毫秒）发送一个心跳包，发5次（系统默认值）</param>
        /// <returns>The number of bytes in the optionOutValue parameter.</returns>
        public int SetKeepAlive(int keepAliveTime, int keepAliveInterval)
        {
            int result = 0;
            if (SocketHelper.IsWindows() && this.IsConnected)
            {
                result = this.socket.IOControl(IOControlCode.KeepAliveValues, SocketHelper.GetTcpKeepAlive(keepAliveTime, keepAliveInterval), null);
            }
            return result;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        public bool Send(byte[] data)
        {
            if (data != null || data.Length > 0 && this.IsConnected)
            {
                byte[] buffer = SocketHelper.ToSendData(data);
                int count = 0;
                while (count < buffer.Length)
                {
                    count += this.socket.Send(buffer, count, buffer.Length - count, SocketFlags.None);
                }

                return true;
            }


            return false;
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <returns></returns>
        public byte[] Receive()
        {
            byte[] buffer = new byte[SocketHelper.PREFIX_LENGTH];
            int count = 0;
            while (count < SocketHelper.PREFIX_LENGTH)
            {
                count += this.socket.Receive(buffer, count, SocketHelper.PREFIX_LENGTH - count, SocketFlags.None);
            }
            int length = SocketHelper.ToPrefixLength(buffer);
            byte[] data = new byte[length];
            count = 0;
            while (count < length)
            {
                count += this.socket.Receive(data, count, length - count, SocketFlags.None);
            }

            return data;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (this.IsConnected || this.isStartConnect)
            {
                try { this.socket.Shutdown(SocketShutdown.Both); }
                catch { }
                this.socket.Close();
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
                this.UserState = null;
            }
        }
    }
}

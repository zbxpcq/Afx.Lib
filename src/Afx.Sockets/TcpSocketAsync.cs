using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Afx.Sockets
{
    /// <summary>
    /// TcpClientAsync
    /// </summary>
    public sealed class TcpSocketAsync : IDisposable
    {
        /// <summary>
        /// 发送队列大小
        /// </summary>
        public const int SEND_QUEUE_SIZE = 50; 

        private Socket socket = null;
        private BufferModel _buffer = null;
        private CacheModel _cache = null;
        private volatile bool isClose = true;
        private volatile bool isDisposed = false;
        private volatile bool isSend = false;
        private object sendLock = new object();
        private Queue<byte[]> sendQueue = new Queue<byte[]>(SEND_QUEUE_SIZE);
        
        /// <summary>
        /// KeepAlive
        /// </summary>
        /// <param name="keepAliveTime">连接多长时间（毫秒）没有数据就开始发送心跳包，有数据传递的时候不发送心跳包</param>
        /// <param name="keepAliveInterval">每隔多长时间（毫秒）发送一个心跳包，发5次（系统默认值）</param>
        /// <returns></returns>
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
        /// 连接成功回调
        /// </summary>
        public event TcpAsyncConnectEvent AsyncConnectEvent;
        private void OnAsyncConnectEvent(bool isSuccess)
        {
            if(this.AsyncConnectEvent != null)
            {
                this.AsyncConnectEvent(this, isSuccess);
            }
        }
        /// <summary>
        /// 接收数据回调
        /// </summary>
        public event TcpReceiveEvent ReceiveEvent;
        private void OnReceiveEvent(byte[] data, int length)
        {
            if (this.ReceiveEvent != null)
            {
                try { this.ReceiveEvent(this, data, length); }
                catch(Exception ex)
                {
                    this.OnErrorEvent(ex);
                }
            }
        }
        /// <summary>
        /// 异常回调
        /// </summary>
        public event TcpErrorEvent ErrorEvent;
        private void OnErrorEvent(Exception ex)
        {
            bool flag = !this.isClose;
            this.Close();
            if (this.isStartConnect)
            {
                this.isStartConnect = false;
                this.OnAsyncConnectEvent(false);
            }
            else if (flag && this.ErrorEvent != null)
            {
                this.ErrorEvent(this, ex);
            }
        }
        /// <summary>
        /// 正在接收数据回调
        /// </summary>
        public event TcpReadingEvent ReadingEvent;
        private void OnReadingEvent(int position, int length)
        {
            if (this.ReadingEvent != null)
            {
                try { this.ReadingEvent(this, position, length); }
                catch(Exception ex)
                {
                    this.OnErrorEvent(ex);
                }
            }
        }

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
        /// SendBufferSize
        /// </summary>
        public int SendBufferSize { get; private set; }
        /// <summary>
        /// ReceiveBufferSize
        /// </summary>
        public int ReceiveBufferSize { get; private set; }
        /// <summary>
        /// Socket
        /// </summary>
        public Socket Client
        {
            get
            {
                return this.socket;
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

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpSocketAsync(int sendBufferSize = 8 * 1024, int receiveBufferSize = 8 * 1024)
        {
            int max = 8 * 1024;
            int min = 16;
            this.SendBufferSize = sendBufferSize < min ? min : (sendBufferSize > max ? max : sendBufferSize);
            this.ReceiveBufferSize = receiveBufferSize < min ? min : (receiveBufferSize > max ? max : receiveBufferSize);
            this._buffer = new BufferModel(this.ReceiveBufferSize);
            this._cache = new CacheModel();
        }

        internal TcpSocketAsync(Socket socket, int sendBufferSize = 8 * 1024, int receiveBufferSize = 8 * 1024)
        {
            int max = 8 * 1024;
            int min = 16;
            this.SendBufferSize = sendBufferSize < min ? min : (sendBufferSize > max ? max : sendBufferSize);
            this.ReceiveBufferSize = receiveBufferSize < min ? min : (receiveBufferSize > max ? max : receiveBufferSize);
            this._buffer = new BufferModel(this.ReceiveBufferSize);
            this._cache = new CacheModel();

            this.socket = socket;
            this.isClose = false;
            this.isSend = false;

            this.sendQueue.Clear();
            this.Handle = this.socket.Handle;
            this.LocalEndPoint = this.socket.LocalEndPoint;
            this.RemoteEndPoint = this.socket.RemoteEndPoint;
            //this.SetKeepAlive(60 * 1000, 60 * 1000);

            this.socket.SendBufferSize = this.SendBufferSize;
            this.socket.ReceiveBufferSize = this.ReceiveBufferSize;
        }
        #endregion

        internal void BeginReceive()
        {
            try
            {
                this._buffer.Clear();
                this._cache.Clear();
                this.socket.BeginReceive(this._buffer.Data, this._buffer.Position, this._buffer.Size,
                    SocketFlags.None, this.OnReceive, null);
            }
            catch (Exception ex)
            {
                this.OnErrorEvent(ex);
            }
        }

        private volatile bool isStartConnect = false;
        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="host">服务端ip或域名</param>
        /// <param name="port">服务端端口</param>
        public void AsyncConnect(string host, int port)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if (port < IPEndPoint.MinPort || port >= IPEndPoint.MaxPort) throw new ArgumentException("port is error!");
            if (!this.IsDisposed && (this.socket == null || !this.socket.Connected))
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.SendBufferSize = this.SendBufferSize;
                this.socket.ReceiveBufferSize = this.ReceiveBufferSize;
                this.isClose = false;
                this.isStartConnect = true;
                try { this.socket.BeginConnect(host, port, this.OnConnect, null); }
                catch (Exception ex) { this.OnErrorEvent(ex); }
            }
        }

        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="host">服务端ip或域名</param>
        /// <param name="port">服务端端口</param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public bool Connect(string host, int port, int millisecondsTimeout = 0)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if (millisecondsTimeout < 0) throw new ArgumentException("millisecondsTimeout is error!");
            if (port < IPEndPoint.MinPort || port >= IPEndPoint.MaxPort) throw new ArgumentException("port is error!");
            bool result = !this.IsDisposed && this.socket != null && this.socket.Connected;
            if (!this.IsDisposed && (this.socket == null || !this.socket.Connected))
            {
                this.isStartConnect = true;
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.SendBufferSize = this.SendBufferSize;
                this.socket.ReceiveBufferSize = this.ReceiveBufferSize;
                if (millisecondsTimeout <= 0)
                {
                    try
                    {
                        this.socket.Connect(host, port);
                        this.BeginReceive();
                        result = true;
                    }
                    catch //(Exception ex)
                    {
                        this.Close();
                    }
                    this.isStartConnect = false;
                }
                else
                {
                    var asyncResult = this.socket.BeginConnect(host, port, new AsyncCallback((o) =>
                    {
                        try
                        {
                            this.socket.EndConnect(o);
                        }
                        catch { }
                    }), null);

                    if (asyncResult.AsyncWaitHandle.WaitOne(millisecondsTimeout < 500 ? 500 : millisecondsTimeout))
                    {
                        result = this.IsConnected;
                        if (this.IsConnected) this.BeginReceive();
                    }
                    else
                    {
                        this.Close();
                    }
                    this.isStartConnect = false;
                }
            }

            return result;
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                this.socket.EndConnect(ar);
                this.isStartConnect = false;
                if (!this.isClose)
                {
                    this.sendQueue.Clear();
                    this.isSend = false;
                    this.Handle = this.socket.Handle;
                    this.LocalEndPoint = this.socket.LocalEndPoint;
                    this.RemoteEndPoint = this.socket.RemoteEndPoint;
                    this.SetKeepAlive(60 * 1000, 60 * 1000);
                    this.OnAsyncConnectEvent(true);
                    
                    this.BeginReceive();
                }
                else
                {
                    this.socket.Close();
                }
            }
            catch (Exception ex)
            {
                this.OnErrorEvent(ex);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        public bool Send(byte[] data)
        {
            if (data != null && data.Length > 0 && this.IsConnected)
            {
                lock (this.sendLock)
                {
                    if (this.sendQueue.Count < SEND_QUEUE_SIZE * 2)
                    {
                        this.sendQueue.Enqueue(data);
                        this.BeginSend();
                        return true;
                    }
                }
            }

            
            return false;
        }

        private void BeginSend()
        {
            if (!this.isSend && this.IsConnected && sendQueue.Count > 0)
            {
                this.isSend = true;
                byte[] data = this.sendQueue.Dequeue();
                
                TcpSendData state = new TcpSendData();
                state.Buffer = SocketHelper.ToSendData(data);
                state.BufferIndex = 0;
                try { this.socket.BeginSend(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, this.OnSend, state); }
                catch (Exception ex) { this.OnErrorEvent(ex); }
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                int sendLength = this.socket.EndSend(ar);
                TcpSendData state = ar.AsyncState as TcpSendData;
                state.BufferIndex += sendLength;
                if (state.BufferIndex < state.Buffer.Length)
                {
                    this.socket.BeginSend(state.Buffer, state.BufferIndex, state.Buffer.Length - state.BufferIndex, SocketFlags.None,
                        this.OnSend, state);
                }
                else
                {
                    this.isSend = false;
                    this.BeginSend();
                }
            }
            catch (Exception ex)
            {
                this.OnErrorEvent(ex);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            this.isClose = true;
            if (this.IsConnected || this.isStartConnect)
            {
                this.isStartConnect = false;
                try { this.socket.Shutdown(SocketShutdown.Both); }
                catch { }
                this.socket.Close();
            }
        }
        
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int readLength = this.socket.EndReceive(ar);

                if (readLength == 0)
                {
                    this.OnErrorEvent(new Exception("close!"));
                    return;
                }

                this._buffer.Position = this._buffer.Position + readLength;

                if (this._cache.Size == 0 && this._buffer.Position >= SocketHelper.PREFIX_LENGTH)
                {
                    byte[] perfixBytes = new byte[SocketHelper.PREFIX_LENGTH];
                    Array.Copy(this._buffer.Data, 0, perfixBytes, 0, perfixBytes.Length);
                    this._cache.Size = SocketHelper.ToPrefixLength(perfixBytes);
                    this._cache.Data = new byte[this._cache.Size];
                    this._cache.Position = this._buffer.Position - SocketHelper.PREFIX_LENGTH;
                    Array.Copy(this._buffer.Data, SocketHelper.PREFIX_LENGTH, this._cache.Data, 0, this._cache.Position);
                    this._buffer.Clear();
                }
                else if (this._cache.Size > 0)
                {
                    Array.Copy(this._buffer.Data, 0, this._cache.Data, this._cache.Position, this._buffer.Position);
                    this._cache.Position = this._cache.Position + this._buffer.Position;
                    this._buffer.Clear();
                }

                this.OnReadingEvent(this._cache.Position, this._cache.Size);

                if (this._cache.Size > 0 && this._cache.Position == this._cache.Size)
                {
                    this.OnReceiveEvent(this._cache.Data, this._cache.Size); 
                    this._cache.Clear();
                }

                this.socket.BeginReceive(this._buffer.Data, this._buffer.Position, this._buffer.Size - this._buffer.Position,
                    SocketFlags.None, this.OnReceive, null);
            }
            catch (Exception ex)
            {
                this.OnErrorEvent(ex);
            }
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            this.Close();
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.socket = null;
                this.isSend = false;
                if (this.sendQueue != null)
                    this.sendQueue.Clear();
                this.sendQueue = null;
                this.UserState = null;
                this.AsyncConnectEvent = null;
                this.ErrorEvent = null;
                this.ReadingEvent = null;
                this._buffer = null;
                this._cache = null;
                this.ReceiveEvent = null;
                this.sendLock = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Afx.Sockets.Models;
using Afx.Sockets.Common;

namespace Afx.Sockets
{
    /// <summary>
    /// TcpBaseClientAsync
    /// </summary>
    public abstract class TcpBaseClientAsync : ITcpClientAsync
    {
        /// <summary>
        /// 发送队列大小
        /// </summary>
        public const int SEND_QUEUE_SIZE = 50; 

        private Socket m_socket;
        private NetworkStream networkStream;
        private BufferModel m_buffer;
        private CacheModel m_cache = new CacheModel();
        private volatile bool isClose = true;
        private volatile bool isDisposed = false;
        private volatile bool isSend = false;
        private object sendLock = new object();
        private Queue<byte[]> sendQueue = new Queue<byte[]>(SEND_QUEUE_SIZE);


        /// <summary>
        /// 接收数据回调
        /// </summary>
        public event TcpReceiveEvent ReceiveEvent;
        protected virtual void OnReceiveEvent(List<byte[]> data)
        {
            if (this.ReceiveEvent != null)
            {
                try { this.ReceiveEvent(this, data); }
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
        protected virtual void OnErrorEvent(Exception ex)
        {
            bool flag = !this.isClose;
            this.Close();
            if (flag && this.ErrorEvent != null)
            {
                this.ErrorEvent(this, ex);
            }
        }
        /// <summary>
        /// 正在接收数据回调
        /// </summary>
        public event TcpReadingEvent ReadingEvent;
        protected virtual void OnReadingEvent(int length)
        {
            if (this.ReadingEvent != null)
            {
                try { this.ReadingEvent(this, length); }
                catch(Exception ex)
                {
                    this.OnErrorEvent(ex);
                }
            }
        }

        /// <summary>
        /// 连接成功回调
        /// </summary>
        public event TcpAsyncConnectEvent AsyncConnectEvent;
        protected virtual void OnAsyncConnectEvent(bool isSuccess)
        {
            if (this.AsyncConnectEvent != null)
            {
                this.AsyncConnectEvent(this, isSuccess);
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
                return this.m_socket != null && this.m_socket.Connected;
            }
        }

        /// <summary>
        /// socket Handle，未调用Connect之前值为IntPtr.Zero
        /// </summary>
        public IntPtr Handle { get { return this.m_socket!= null ? this.m_socket.Handle : IntPtr.Zero; } }
        /// <summary>
        /// LocalEndPoint
        /// </summary>
        public EndPoint LocalEndPoint { get { return this.m_socket != null ? this.m_socket.LocalEndPoint : null; } }
        /// <summary>
        /// RemoteEndPoint
        /// </summary>
        public EndPoint RemoteEndPoint { get { return this.m_socket != null ? this.m_socket.RemoteEndPoint : null; } }
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
                return this.m_socket;
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
        public TcpBaseClientAsync(int sendBufferSize = 8 * 1024, int receiveBufferSize = 8 * 1024)
        {
            int max = 8 * 1024;
            int min = 16;
            this.SendBufferSize = sendBufferSize < min ? min : (sendBufferSize > max ? max : sendBufferSize);
            this.ReceiveBufferSize = receiveBufferSize < min ? min : (receiveBufferSize > max ? max : receiveBufferSize);
            this.m_buffer = new BufferModel(this.ReceiveBufferSize);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected TcpBaseClientAsync(Socket socket, int sendBufferSize = 8 * 1024, int receiveBufferSize = 8 * 1024)
        {
            int max = 8 * 1024;
            int min = 16;
            this.SendBufferSize = sendBufferSize < min ? min : (sendBufferSize > max ? max : sendBufferSize);
            this.ReceiveBufferSize = receiveBufferSize < min ? min : (receiveBufferSize > max ? max : receiveBufferSize);
            this.m_buffer = new BufferModel(this.ReceiveBufferSize);

            this.m_socket = socket;
            this.isClose = false;
            this.isSend = false;

            this.m_socket.SendBufferSize = this.SendBufferSize;
            this.m_socket.ReceiveBufferSize = this.ReceiveBufferSize;
        }


        #endregion

        /// <summary>
        /// KeepAlive
        /// </summary>
        /// <param name="keepAliveTime">连接多长时间（毫秒）没有数据就开始发送心跳包，有数据传递的时候不发送心跳包</param>
        /// <param name="keepAliveInterval">每隔多长时间（毫秒）发送一个心跳包，发5次（系统默认值）</param>
        /// <returns></returns>
        public virtual int SetKeepAlive(int keepAliveTime, int keepAliveInterval)
        {
            int result = 0;
            if (SocketHelper.IsWindows() && this.IsConnected)
            {
                result = this.m_socket.IOControl(IOControlCode.KeepAliveValues, SocketHelper.GetTcpKeepAlive(keepAliveTime, keepAliveInterval), null);
            }
            return result;
        }

        private volatile bool isStartConnect = false;
        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="host">服务端ip或域名</param>
        /// <param name="port">服务端端口</param>
        public virtual void AsyncConnect(string host, int port)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if (port < IPEndPoint.MinPort || port >= IPEndPoint.MaxPort) throw new ArgumentException("port is error!");
            if (!this.IsDisposed && (this.m_socket == null || !this.m_socket.Connected))
            {
                this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.m_socket.SendBufferSize = this.SendBufferSize;
                this.m_socket.ReceiveBufferSize = this.ReceiveBufferSize;
                this.isClose = false;
                this.isStartConnect = true;
                try { this.m_socket.BeginConnect(host, port, this.OnConnect, null); }
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
        public virtual bool Connect(string host, int port, int millisecondsTimeout = 0)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if (millisecondsTimeout < 0) throw new ArgumentException("millisecondsTimeout is error!");
            if (port < IPEndPoint.MinPort || port >= IPEndPoint.MaxPort) throw new ArgumentException("port is error!");
            bool result = !this.IsDisposed && this.m_socket != null && this.m_socket.Connected;
            if (!this.IsDisposed && (this.m_socket == null || !this.m_socket.Connected))
            {
                this.isStartConnect = true;
                this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.m_socket.SendBufferSize = this.SendBufferSize;
                this.m_socket.ReceiveBufferSize = this.ReceiveBufferSize;
                if (millisecondsTimeout <= 0)
                {
                    try
                    {
                        this.m_socket.Connect(host, port);
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
                    var asyncResult = this.m_socket.BeginConnect(host, port, new AsyncCallback((o) =>
                    {
                        try
                        {
                            this.m_socket.EndConnect(o);
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
                this.m_socket.EndConnect(ar);
                this.isStartConnect = false;
                if (!this.isClose)
                {
                    this.sendQueue.Clear();
                    this.isSend = false;
                    this.SetKeepAlive(60 * 1000, 60 * 1000);
                    this.OnAsyncConnectEvent(true);
                    this.isStartReceive = true;
                    this.BeginReceive();
                }
                else
                {
                    this.m_socket.Close();
                }
            }
            catch (Exception ex)
            {
                this.OnErrorEvent(ex);
                this.OnAsyncConnectEvent(false);
            }
        }

        private bool isStartReceive = false;
        internal void StartReceive()
        {
            if (this.isStartReceive) return;
            this.isStartReceive = true;
            BeginReceive();
        }

        private void BeginReceive()
        {
            try
            {
                this.m_buffer.Clear();
                this.m_cache.Clear();
                this.networkStream = new NetworkStream(this.m_socket);
                this.networkStream.BeginRead(this.m_buffer.Data, this.m_buffer.Position, this.m_buffer.Size, this.OnReceive, null);
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
        public virtual bool Send(byte[] data)
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
                state.Buffer = data;
                state.BufferIndex = 0;
                try { this.networkStream.BeginWrite(state.Buffer, 0, state.Buffer.Length, this.OnSend, state); }
                catch (Exception ex) { this.OnErrorEvent(ex); }
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                this.networkStream.EndWrite(ar);
                this.isSend = false;
                this.BeginSend();

                //int sendLength = this.m_socket.EndSend(ar);
                //TcpSendData state = ar.AsyncState as TcpSendData;
                //state.BufferIndex += sendLength;
                //if (state.BufferIndex < state.Buffer.Length)
                //{
                //    this.m_socket.BeginSend(state.Buffer, state.BufferIndex, state.Buffer.Length - state.BufferIndex, SocketFlags.None,
                //        this.OnSend, state);
                //}
                //else
                //{
                //    this.isSend = false;
                //    this.BeginSend();
                //}
            }
            catch (Exception ex)
            {
                this.OnErrorEvent(ex);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public virtual void Close()
        {
            this.isStartReceive = false;
            this.isClose = true;
            if (this.IsConnected || this.isStartConnect)
            {
                this.isStartConnect = false;
                if(this.networkStream != null) try { this.networkStream.Close(); } catch { }
                try { this.m_socket.Shutdown(SocketShutdown.Both); }
                catch { }
                this.m_socket.Close();
                this.networkStream = null;
            }
        }

        protected abstract bool ReceiveData(BufferModel buffer, CacheModel cache, out List<byte[]> data);

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int readLength = this.networkStream.EndRead(ar);

                if (readLength == 0)
                {
                    this.OnErrorEvent(new Exception("close!"));
                    return;
                }

                this.m_buffer.Position = this.m_buffer.Position + readLength;

                this.OnReadingEvent(readLength);

                List<byte[]> data;
                if (this.ReceiveData(this.m_buffer, this.m_cache, out data))
                {
                    if (data != null) this.OnReceiveEvent(data);
                    this.networkStream.BeginRead(this.m_buffer.Data, this.m_buffer.Position, this.m_buffer.Size - this.m_buffer.Position,
                        this.OnReceive, null);
                }
                else
                {
                    this.OnErrorEvent(new Exception("data is error!"));
                }
            }
            catch (Exception ex)
            {
                this.OnErrorEvent(ex);
            }
        }

        protected void Dispose(bool dispose)
        {
            if (dispose)
            {
                this.Close();
                this.isDisposed = true;
                this.m_socket = null;
                this.isSend = false;
                if (this.sendQueue != null)
                    this.sendQueue.Clear();
                this.sendQueue = null;
                this.UserState = null;
                this.AsyncConnectEvent = null;
                this.ErrorEvent = null;
                this.ReadingEvent = null;
                this.m_buffer = null;
                this.m_cache = null;
                this.ReceiveEvent = null;
                this.sendLock = null;
            }
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(true);
        }
    }
}

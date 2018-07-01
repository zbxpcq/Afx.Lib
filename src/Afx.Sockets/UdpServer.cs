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
    /// UdpSocketServer
    /// </summary>
    public sealed class UdpServer : IDisposable
    {
        private Socket socket;
        private byte[] receiveBuffer;
        private volatile bool isAccept;
        private volatile bool isDisposed;
        private volatile bool isSend;
        private Queue<UdpSendData> sendQueue;

        /// <summary>
        /// 接收数据回调
        /// </summary>
        public event UdpReceiveEvent ReceiveEvent;
        private void OnReceiveEvent(EndPoint remoteEP, byte[] data, int length)
        {
            if(this.ReceiveEvent != null)
            {
                this.ReceiveEvent(remoteEP, data, length);
            }
        }
        /// <summary>
        /// 异常回调
        /// </summary>
        public event UdpServerErrorEvent ServerErrorEvent;
        private void OnErrorEvent(EndPoint remoteEP, Exception ex)
        {
            if (this.isAccept && this.ServerErrorEvent != null)
            {
                this.ServerErrorEvent(remoteEP, ex);
            }
        }

        /// <summary>
        /// 是否监听
        /// </summary>
        public bool IsAccept
        {
            get
            {
                return isAccept;
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

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="receiveSize">接收数据最大长度</param>
        public UdpServer(int receiveSize)
        {
            this.receiveBuffer = new byte[receiveSize];
            this.isAccept = false;
            this.isDisposed = false;
            this.isSend = false;
            this.sendQueue = new Queue<UdpSendData>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpServer() : this(8 * 1024) { }

        /// <summary>
        /// 绑定本地端口，端口占用异常
        /// </summary>
        /// <param name="port">本地端口</param>
        public void Start(int port)
        {
            if (!this.isAccept)
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint localEP = new IPEndPoint(IPAddress.Any, port);
                this.socket.Bind(localEP);
                this.isAccept = true;
                this.isSend = false;
                this.sendQueue.Clear();
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                this.socket.BeginReceiveFrom(this.receiveBuffer, 0, this.receiveBuffer.Length, SocketFlags.None, ref remoteEP, this.OnReceive, null);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="remoteEP">远程终结点</param>
        /// <param name="data">数据</param>
        public void Send(EndPoint remoteEP, byte[] data)
        {
            if (data != null && data.Length > 0 && this.isAccept)
            {
                UdpSendData state = new UdpSendData();
                state.RemoteEP = remoteEP;
                state.Buffer = data;
                this.sendQueue.Enqueue(state);
                ThreadPool.QueueUserWorkItem(this.BeginSend);
            }
        }

        private void BeginSend(object obj)
        {
            if (!this.isSend && this.isAccept && this.sendQueue.Count > 0)
            {
                this.isSend = true;
                UdpSendData state = this.sendQueue.Dequeue();
                try
                {
                    this.socket.BeginSendTo(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, state.RemoteEP, this.OnSend, state.RemoteEP);
                }
                catch (Exception ex)
                {
                    this.OnErrorEvent(state.RemoteEP, ex);
                }
            }
        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        public void Close()
        {
            if (this.isAccept)
            {
                this.isAccept = false;
                if (this.socket != null)
                {
                    if (this.socket.Connected)
                    {
                        try { this.socket.Shutdown(SocketShutdown.Both); }
                        catch { }
                    }
                    try { this.socket.Close(); }
                    catch { }
                }
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            EndPoint readEP = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                int readLength = this.socket.EndReceiveFrom(ar, ref readEP);
                if (readLength > 0)
                {
                    byte[] data = new byte[readLength];
                    Array.Copy(this.receiveBuffer, 0, data, 0, data.Length);

                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    this.socket.BeginReceiveFrom(this.receiveBuffer, 0, this.receiveBuffer.Length, SocketFlags.None, ref remoteEP, this.OnReceive, null);

                    this.OnReceiveEvent(readEP, data, readLength);
                }
                else if(this.IsAccept)
                {
                    this.OnErrorEvent(readEP, new Exception("read data length is 0!"));
                }
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                {
                    SocketException se = ex as SocketException;
                    if (this.isAccept && se.SocketErrorCode == SocketError.MessageSize)
                    {
                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        this.socket.BeginReceiveFrom(this.receiveBuffer, 0, this.receiveBuffer.Length, SocketFlags.None, ref remoteEP, this.OnReceive, null);
                    }
                }
                this.OnErrorEvent(readEP, ex);
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            EndPoint remoteEP = ar.AsyncState as EndPoint;
            try
            {
               int sendLength = this.socket.EndSendTo(ar);
               this.isSend = false;
               this.BeginSend(null);
            }
            catch (Exception ex)
            {
                this.OnErrorEvent(remoteEP, ex);
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
                this.isSend = false;
                if (this.sendQueue != null)
                {
                    while (this.sendQueue.Count > 0)
                        this.sendQueue.Dequeue().Dispose();
                }
                this.sendQueue = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;

using Afx.Sockets;

namespace Afx.Tcp.Protocols
{
    /// <summary>
    /// MsgClient
    /// </summary>
    public class MsgClient : IMsgClient
    {
        private TcpSocketAsync tcpClient = null;
        /// <summary>
        /// 服务端host
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// 服务端port
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 加密回调
        /// </summary>
        public Func<byte[], byte[]> Encrypt { get; set; }
        /// <summary>
        /// 加密回调
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected virtual byte[] OnEncrypt(byte[] buffer)
        {
            if (this.Encrypt != null)
            {
                buffer = this.Encrypt(buffer);
            }

            return buffer;
        }

        /// <summary>
        /// 解密回调
        /// </summary>
        public Func<byte[], byte[]> Decrypt { get; set; }
        /// <summary>
        /// 解密回调
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected virtual byte[] OnDecrypt(byte[] buffer)
        {
            if (this.Decrypt != null)
            {
                buffer = this.Decrypt(buffer);
            }

            return buffer;
        }

        /// <summary>
        /// 连接成功本地ip
        /// </summary>
        public string LocalIpAddress
        {
            get
            {
                string s = "";
                if (this.tcpClient != null && !this.tcpClient.IsDisposed && this.tcpClient.IsConnected)
                {
                    s = this.tcpClient.LocalEndPoint.ToString();
                    int i = s.IndexOf(":");
                    if (i > 0) s = s.Substring(0, i);
                }

                return s;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public MsgClient()
        {
            this.tcpClient = new TcpSocketAsync();
            this.tcpClient.AsyncConnectEvent += OnAsyncConnectEvent;
            this.tcpClient.ErrorEvent += OnErrorEvent;
            this.tcpClient.ReceiveEvent += ReceiveEvent;
            this.tcpClient.ReadingEvent += ReadingEvent;
        }

        /// <summary>
        /// 正在接收数据回调
        /// </summary>
        public MsgClientReadingCall ReadingCall;
        /// <summary>
        /// 正在接收数据回调
        /// </summary>
        /// <param name="position">当前读取位置</param>
        /// <param name="length">当前信息长度</param>
        protected virtual void OnReadingCall(int position, int length)
        {
            if (this.ReadingCall != null)
            {
                this.ReadingCall(this, position, length);
            }
        }
        
        private void ReadingEvent(TcpSocketAsync client, int position, int length)
        {
            this.OnReadingCall(position, length);
        }
        /// <summary>
        /// IsConnected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.tcpClient.IsConnected;
            }
        }

        private static int _msgId = 0;
        /// <summary>
        /// 获取msg id
        /// </summary>
        /// <returns></returns>
        protected virtual int GetMsgId()
        {
            int val = Interlocked.Increment(ref _msgId);
            if (val >= int.MaxValue - 1)
            {
                Interlocked.Exchange(ref _msgId, 0);
            }

            return val;
        }

        private MsgClientConnectedCall connectCall;
        private object connectState;
        /// <summary>
        /// 连接成功回调
        /// </summary>
        /// <param name="isSuccess"></param>
        protected virtual void OnConnectCall(bool isSuccess)
        {
            if (this.connectCall != null)
            {
                this.connectCall(this, isSuccess, this.connectState);
                this.connectState = null;
            }
        }

        /// <summary>
        /// 异步连接服务端
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="call"></param>
        /// <param name="state"></param>
        public virtual void ConnectAsync(string host, int port, MsgClientConnectedCall call, object state = null)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if (IPEndPoint.MinPort >= port || port >= IPEndPoint.MaxPort) throw new ArgumentException("port is error!");
          this.connectCall = call;
            this.connectState = state;
            this.Close();
            this.Host = host;
            this.Port = port;
            this.tcpClient.AsyncConnect(this.Host, this.Port);
        }
        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="hostAndPort"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public virtual bool Connect(string hostAndPort, int millisecondsTimeout = 5000)
        {
            if (string.IsNullOrEmpty(hostAndPort)) throw new ArgumentNullException("hostAndPort");
            if(!hostAndPort.Contains(":")) throw new ArgumentException("hostAndPort is error!");
            bool result = false;
            this.Close();
            if (!string.IsNullOrEmpty(hostAndPort))
            {
                string[] arr = hostAndPort.Split(';');
                foreach (var s in arr)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        string[] hh = s.Split(':');
                        int port =0;
                        if (!string.IsNullOrEmpty(hh[0]) && hh.Length > 1 && int.TryParse(hh[1], out port)
                            && IPEndPoint.MinPort < port && port < IPEndPoint.MaxPort)
                        {
                            this.Host = hh[0];
                            this.Port = port;
                            if (this.tcpClient.Connect(this.Host, this.Port, millisecondsTimeout))
                            {
                                result = true;
                                break;
                            }
                        }
                        else
                        {
                            throw new ArgumentException(s  +" hostAndPort is error!");
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public virtual void Close()
        {
            this.tcpClient.Close();
        }

        /// <summary>
        /// 关闭连接，重置所有状态
        /// </summary>
        public virtual void Reset()
        {
            this.Close();
            MsgCmdCallDic.Clear();
            MsgIdCallDic.Clear();
        }

#if NET20
        private Afx.Collections.SafeDictionary<int, MsgCmdCallModel> MsgCmdCallDic = new Afx.Collections.SafeDictionary<int, MsgCmdCallModel>();
#else
        private System.Collections.Concurrent.ConcurrentDictionary<int, MsgCmdCallModel> MsgCmdCallDic = new System.Collections.Concurrent.ConcurrentDictionary<int, MsgCmdCallModel>();
#endif
        private MsgCmdCallModel GetCmdCall(int cmd)
        {
            var callInfo = this.MsgCmdCallDic[cmd];

            return callInfo;
        }
        /// <summary>
        /// 添加 cmd call
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="call"></param>
        /// <param name="state"></param>
        public virtual void AddCmdCall(int cmd, MsgDataCall call, object state = null)
        {
            MsgCmdCallModel callInfo = new MsgCmdCallModel()
            { Cmd = cmd, Call = call, State = state };
            this.MsgCmdCallDic[cmd] = callInfo;
        }
        /// <summary>
        /// 移除 cmd call
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void RemoveCmdCall(int cmd)
        {
            MsgCmdCallModel callInfo;
            this.MsgCmdCallDic.TryRemove(cmd, out callInfo);
        }

#if NET20
        private Afx.Collections.SafeDictionary<int, MsgIdCallModel> MsgIdCallDic = new Afx.Collections.SafeDictionary<int, MsgIdCallModel>();
#else
        private System.Collections.Concurrent.ConcurrentDictionary<int, MsgIdCallModel> MsgIdCallDic = new System.Collections.Concurrent.ConcurrentDictionary<int, MsgIdCallModel>();
#endif
        private MsgIdCallModel GetMsgIdCall(int msgId)
        {
            MsgIdCallModel callInfo;
            this.MsgIdCallDic.TryRemove(msgId, out callInfo);

            return callInfo;
        }
        /// <summary>
        /// AddMsgIdCall
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="call"></param>
        /// <param name="state"></param>
        public virtual void AddMsgIdCall(int msgId, MsgDataCall call, object state = null)
        {
            MsgIdCallModel callInfo = new MsgIdCallModel() { Id = msgId, Call = call, State = state };
            this.MsgIdCallDic[msgId] = callInfo;
        }
        /// <summary>
        /// RemoveMsgIdCall
        /// </summary>
        /// <param name="msgId"></param>
        public virtual void RemoveMsgIdCall(int msgId)
        {
            MsgIdCallModel callInfo;
            this.MsgIdCallDic.TryRemove(msgId, out callInfo);
        }
        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void SendAsync(int cmd)
        {
            this.SendAsync<object>(cmd, null, null);
        }
        /// <summary>
        /// SendAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        public virtual void SendAsync<T>(int cmd, T data)
        {
            this.SendAsync(cmd, data, null);
        }

        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="cmd">需要发送的消息</param>
        /// <param name="data"></param>
        /// <param name="call">回调函数</param>
        /// <param name="state">传入数据</param>
        /// <returns>msgId</returns>
        public virtual int SendAsync<T>(int cmd, T data, MsgDataCall call, object state = null)
        {
            int msgId = this.GetMsgId();
            MsgData msg = new MsgData()
            {
                Id = msgId,
                Cmd = cmd
            };
            if (data != null) msg.SetData(data);
            if (call != null)
            {
                this.AddMsgIdCall(msgId, call, state);
            }
            var buffer = msg.Serialize();
            buffer = OnEncrypt(buffer);
            this.tcpClient.Send(buffer);

            return msgId;
        }
        /// <summary>
        /// Send
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual MsgData Send(int cmd)
        {
            return this.Send(cmd, -1);
        }
        /// <summary>
        /// Send
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public virtual MsgData Send(int cmd, int millisecondsTimeout)
        {
            return this.Send<object>(cmd, null, millisecondsTimeout);
        }
        /// <summary>
        /// Send
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual MsgData Send<T>(int cmd, T data)
        {
            return this.Send(cmd, data, -1);
        }

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data">消息内容</param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public virtual MsgData Send<T>(int cmd, T data, int millisecondsTimeout)
        {
            MsgData result = null;
            using (ManualResetEvent manualResetEvent = new ManualResetEvent(false))
            {
                int msgId = this.SendAsync(cmd, data, (client, m, o) =>
                {
                    try
                    {
                        result = m;
                        manualResetEvent.Set();
                    }
                    catch { }
                });

                if (!manualResetEvent.WaitOne(millisecondsTimeout))
                {
                    this.RemoveMsgIdCall(msgId);
                }
            }

            return result;
        }

        private void OnAsyncConnectEvent(TcpSocketAsync client, bool isSuccess)
        {
            if (isSuccess)
            {
                this.tcpClient.SetKeepAlive(10000, 10000);
            }

            this.OnConnectCall(isSuccess);
        }

        /// <summary>
        /// 断线回调
        /// </summary>
        public MsgClientClosedCall ClosedCall { get; set; }
        /// <summary>
        /// 断线回调
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnClosedCall(Exception ex)
        {
            if (this.ClosedCall != null)
            {
                this.ClosedCall(this, ex);
            }
        }

        private void OnErrorEvent(TcpSocketAsync client, Exception ex)
        {
            this.Close();
            this.OnClosedCall(ex);
        }

        private void ReceiveEvent(TcpSocketAsync client, byte[] data, int length)
        {
            if (data == null || data.Length == 0) return;
            ThreadPool.QueueUserWorkItem((o) =>
            {
                var buffer = o as byte[];
                buffer = OnDecrypt(buffer);
                MsgData msg = MsgData.Deserialize(buffer);
                if (msg != null)
                {
                    if (msg.Id != 0)
                    {
                        var callModel = this.GetMsgIdCall(msg.Id);
                        if (callModel != null)
                        {
                            this.OnMsgIdCall(msg, callModel);
                            return;
                        }
                    }

                    var call = this.GetCmdCall(msg.Cmd);
                    this.OnMsgCmdCall(msg, call);
                }
            }, data);
        }

        private void OnMsgIdCall(MsgData msg, MsgIdCallModel callInfo)
        {
            if (callInfo == null || callInfo.Call == null)
                return;

            callInfo.Call(this, msg, callInfo.State); 
            callInfo.State = null;
        }

        private void OnMsgCmdCall(MsgData msg, MsgCmdCallModel callInfo)
        {
            if (callInfo == null || callInfo.Call == null)
                return;

            callInfo.Call(this, msg, callInfo.State);
            callInfo.State = null;
        }


        private bool isDisposed = false;
        /// <summary>
        /// IsDisposed
        /// </summary>
        public bool IsDisposed { get { return this.isDisposed; } }
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.Reset();
                this.MsgCmdCallDic.Clear();
                this.MsgIdCallDic.Clear();
                this.tcpClient.Dispose();
                this.MsgCmdCallDic = null;
                this.MsgIdCallDic = null;
                this.tcpClient = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Tcp.Protocols
{
    /// <summary>
    /// IMsgClient
    /// </summary>
    public interface IMsgClient : IDisposable
    {
        /// <summary>
        /// IsConnected
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// IsDisposed
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// 连接成功的本地ip，未连接返回""
        /// </summary>
        string LocalIpAddress { get; }
        /// <summary>
        /// 服务端host
        /// </summary>
        string Host { get; }
        /// <summary>
        /// 服务端port
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 加密回调
        /// </summary>
        Func<byte[], byte[]> Encrypt { get; set; }

        /// <summary>
        /// 解密回调
        /// </summary>
        Func<byte[], byte[]> Decrypt { get; set; }

        /// <summary>
        /// 断线回调
        /// </summary>
        MsgClientClosedCall ClosedCall { get; set; }

        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="hostAndPort"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool Connect(string hostAndPort, int timeout);

        /// <summary>
        /// 异步连接服务端
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="call"></param>
        /// <param name="state"></param>
        void ConnectAsync(string host, int port, MsgClientConnectedCall call, object state = null);

        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="cmd"></param>
        void SendAsync(int cmd);
        /// <summary>
        /// SendAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        void SendAsync<T>(int cmd, T data);

        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="cmd">需要发送的消息</param>
        /// <param name="data"></param>
        /// <param name="call">回调函数</param>
        /// <param name="state">传入数据</param>
        /// <returns>msgId</returns>
        int SendAsync<T>(int cmd, T data, MsgDataCall call, object state = null);
        
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        MsgData Send(int cmd);
        /// <summary>
        /// Send
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        MsgData Send(int cmd, int millisecondsTimeout);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        MsgData Send<T>(int cmd, T data);

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data">消息内容</param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        MsgData Send<T>(int cmd, T data, int millisecondsTimeout);
        

        /// <summary>
        /// AddMsgIdCall
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="call"></param>
        /// <param name="state"></param>
        void AddMsgIdCall(int msgId, MsgDataCall call, object state = null);
        /// <summary>
        /// RemoveMsgIdCall
        /// </summary>
        /// <param name="msgId"></param>
        void RemoveMsgIdCall(int msgId);

        /// <summary>
        /// 添加 cmd call
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="call"></param>
        /// <param name="state"></param>
        void AddCmdCall(int cmd, MsgDataCall call, object state = null);
        /// <summary>
        /// 移除 cmd call
        /// </summary>
        /// <param name="cmd"></param>
        void RemoveCmdCall(int cmd);

        /// <summary>
        /// 关闭连接，重置所有状态
        /// </summary>
        void Reset();

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
    }
}

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
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        MsgData Send<T>(int cmd, T data);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        MsgData Send(int cmd);
        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="hostAndPort"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool Connect(string hostAndPort, int timeout);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
    }
}

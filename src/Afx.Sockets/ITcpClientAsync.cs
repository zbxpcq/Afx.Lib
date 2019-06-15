using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Afx.Sockets
{
    public interface ITcpClientAsync : IDisposable
    {
        /// <summary>
        /// 接收数据回调
        /// </summary>
        event TcpReceiveEvent ReceiveEvent;

        /// <summary>
        /// 异常回调
        /// </summary>
        event TcpErrorEvent ErrorEvent;

        /// <summary>
        /// 正在接收数据回调
        /// </summary>
        event TcpReadingEvent ReadingEvent;
        /// <summary>
        /// 连接成功回调
        /// </summary>
        event TcpAsyncConnectEvent AsyncConnectEvent;
        /// <summary>
        /// 用户定义的对象
        /// </summary>
        object UserState { get; set; }

        /// <summary>
        /// 是否连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// socket Handle，未调用Connect之前值为IntPtr.Zero
        /// </summary>
        IntPtr Handle { get; }
        /// <summary>
        /// LocalEndPoint
        /// </summary>
        EndPoint LocalEndPoint { get; }
        /// <summary>
        /// RemoteEndPoint
        /// </summary>
        EndPoint RemoteEndPoint { get; }
        /// <summary>
        /// SendBufferSize
        /// </summary>
        int SendBufferSize { get;  }
        /// <summary>
        /// ReceiveBufferSize
        /// </summary>
        int ReceiveBufferSize { get;  }
        /// <summary>
        /// Socket
        /// </summary>
        Socket Client { get; }

        /// <summary>
        /// 是否释放
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        bool Send(byte[] data);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

        /// <summary>
        /// KeepAlive
        /// </summary>
        /// <param name="keepAliveTime">连接多长时间（毫秒）没有数据就开始发送心跳包，有数据传递的时候不发送心跳包</param>
        /// <param name="keepAliveInterval">每隔多长时间（毫秒）发送一个心跳包，发5次（系统默认值）</param>
        /// <returns></returns>
        int SetKeepAlive(int keepAliveTime, int keepAliveInterval);

        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="host">服务端ip或域名</param>
        /// <param name="port">服务端端口</param>
        void AsyncConnect(string host, int port);

        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="host">服务端ip或域名</param>
        /// <param name="port">服务端端口</param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        bool Connect(string host, int port, int millisecondsTimeout = 0);
    }
}

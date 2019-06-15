using System;
using System.Net;
using System.Net.Sockets;

namespace Afx.Sockets
{
    /// <summary>
    /// tcp server
    /// </summary>
    public interface ITcpServer : IDisposable
    {
        /// <summary>
        /// 监听客户端回调
        /// </summary>
        event TcpAcceptEvent AcceptEvent;

        /// <summary>
        /// 异常回调
        /// </summary>
        event TcpServerErrorEvent ServerErrorEvent;

        /// <summary>
        /// 客户端接收数据回调
        /// </summary>
        event TcpReceiveEvent ClientReceiveEvent;

        /// <summary>
        /// 客户端异常回调
        /// </summary>
        event TcpErrorEvent ClientErrorEvent;

        /// <summary>
        /// 客户端正在接收数据回调
        /// </summary>
        event TcpReadingEvent ClientReadingEvent;

        /// <summary>
        /// 是否在监听
        /// </summary>
        bool IsAccept { get; }

        /// <summary>
        /// 是否释放
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// 监听 socket Handle，未调用Start之前值为IntPtr.Zero
        /// </summary>
        IntPtr Handle { get; }

        /// <summary>
        /// 监听 socket，未调用Start之前值为null
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// 发送Buffer缓存大小
        /// </summary>
        int SendBufferSize { get; }

        /// <summary>
        /// 接收数据Buffer缓存大小
        /// </summary>
        int ReceiveBufferSize { get; }

        /// <summary>
        /// 启动监听，端口占用异常
        /// </summary>
        /// <param name="port">本地端口号</param>
        /// <param name="isBackground">是否后台监听连接</param>
        void Start(int port, bool isBackground = true);

        /// <summary>
        /// 启动监听，端口占用异常
        /// </summary>
        /// <param name="ipAddress">监听ip</param>
        /// <param name="port">本地端口号</param>
        /// <param name="isBackground">是否后台监听连接</param>
        void Start(IPAddress ipAddress, int port, bool isBackground = true);

        /// <summary>
        /// 关闭服务端监听
        /// </summary>
        void Close();

    }
}

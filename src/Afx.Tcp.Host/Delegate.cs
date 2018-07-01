using System;
using System.Collections.Generic;
using System.Text;

using Afx.Tcp.Protocols;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// host 发生错误回调
    /// </summary>
    /// <param name="server"></param>
    /// <param name="ex"></param>
    public delegate void MvcHostServerError(TcpHost server, Exception ex);

    /// <summary>
    /// client 链接成功回调
    /// </summary>
    /// <param name="server"></param>
    /// <param name="session"></param>
    public delegate void ClientConnected(TcpHost server, Session session);

    /// <summary>
    /// client closed 回调
    /// </summary>
    /// <param name="server"></param>
    /// <param name="session"></param>
    public delegate void ClientClosed(TcpHost server, Session session);

    /// <summary>
    /// cmd 执行前回调
    /// </summary>
    /// <param name="server"></param>
    /// <param name="session"></param>
    /// <param name="input"></param>
    public delegate void CmdExecuting(TcpHost server, Session session, MsgData input);

    /// <summary>
    /// cmd 执行后回调
    /// </summary>
    /// <param name="server"></param>
    /// <param name="session"></param>
    /// <param name="input"></param>
    /// <param name="output"></param>
    public delegate void CmdExecuted(TcpHost server, Session session, MsgData input, MsgData output);
}

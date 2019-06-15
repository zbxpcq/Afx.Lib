using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Tcp.Protocols
{
    /// <summary>
    /// 消息回调
    /// </summary>
    /// <param name="msgClient">client</param>
    /// <param name="msg">接收到的消息</param>
    /// <param name="state">回调 state</param>
    public delegate void MsgDataCall(MsgClient msgClient, MsgData msg, object state);

    /// <summary>
    /// 异步连接回调
    /// </summary>
    /// <param name="msgClient">client</param>
    /// <param name="isSuccess">true：连接成功，false：连接失败</param>
    /// <param name="state">回调 state</param>
    public delegate void MsgClientConnectedCall(MsgClient msgClient, bool isSuccess, object state);

    /// <summary>
    /// 连接关闭回调
    /// </summary>
    /// <param name="msgClient"></param>
    /// <param name="ex">关闭异常</param>
    public delegate void MsgClientClosedCall(MsgClient msgClient, Exception ex);

    /// <summary>
    /// 正在读取数据回调
    /// </summary>
    /// <param name="msgClient"></param>
    /// <param name="length"></param>
    public delegate void MsgClientReadingCall(MsgClient msgClient, long length);
}

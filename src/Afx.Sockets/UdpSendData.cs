using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Afx.Sockets
{
    /// <summary>
    /// udp 发送数据对象
    /// </summary>
    public sealed class UdpSendData : System.IDisposable
    {
        /// <summary>
        /// 接收端终结点
        /// </summary>
        public EndPoint RemoteEP { get; set; }

        /// <summary>
        /// 发送数据
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.RemoteEP = null;
            this.Buffer = null;
        }
    }
}

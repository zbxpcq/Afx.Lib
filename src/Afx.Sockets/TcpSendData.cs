using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Afx.Sockets.Models
{
    /// <summary>
    /// tcp 发送数据对象
    /// </summary>
    class TcpSendData : System.IDisposable
    {
        /// <summary>
        /// 发送数据
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// 发送数据起始位置
        /// </summary>
        public int BufferIndex { get; set; }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Buffer = null;
        }
    }
}

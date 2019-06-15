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
    /// tcp server
    /// </summary>
    public sealed class TcpServer : TcpBaseServer
    {
        protected override TcpBaseClientAsync Accept(Socket client, int sendBufferSize, int receiveBufferSize)
        {
            return new TcpSocketAsync(client, sendBufferSize, receiveBufferSize);
        }
    }
}

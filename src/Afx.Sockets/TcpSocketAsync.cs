using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Afx.Sockets.Models;
using Afx.Sockets.Common;

namespace Afx.Sockets
{
    /// <summary>
    /// TcpClientAsync
    /// </summary>
    public sealed class TcpSocketAsync : TcpBaseClientAsync
    {
        public TcpSocketAsync(int sendBufferSize = 8 * 1024, int receiveBufferSize = 8 * 1024)
            : base(sendBufferSize, receiveBufferSize)
        {

        }

        internal TcpSocketAsync(Socket socket, int sendBufferSize = 8 * 1024, int receiveBufferSize = 8 * 1024)
            : base(socket, sendBufferSize, receiveBufferSize)
        {

        }

        protected override bool ReceiveData(BufferModel buffer, CacheModel cache, out List<byte[]> data)
        {
            data = null;
            if (cache.Size == 0 && buffer.Position < SocketHelper.PREFIX_LENGTH)
            {
                return true;
            }
            else
            {
                int start = 0;
                while (start < cache.Position + buffer.Position)
                {
                    int len = 0;
                    if (cache.Position > 0) len = cache.Size;
                    else
                    {
                        byte[] arrlen = new byte[SocketHelper.PREFIX_LENGTH];
                        Array.Copy(buffer.Data, start, arrlen, 0, arrlen.Length);
                        len = SocketHelper.ToPrefixLength(arrlen);
                    }

                    if (len <= 0 || len > SocketHelper.MAX_PREFIX_LENGTH) return false;

                    if (start + len <= cache.Position + buffer.Position)
                    {
                        if (cache.Position > 0)
                        {
                            Array.Copy(buffer.Data, start, cache.Data, cache.Position, len - cache.Position);
                            if (data == null) data = new List<byte[]>(3);
                            data.Add(cache.Data);
                            start = len - cache.Position;
                            cache.Clear();
                        }
                        else
                        {
                            var temp = new byte[len];
                            Array.Copy(buffer.Data, start, temp, 0, temp.Length);
                            if (data == null) data = new List<byte[]>(3);
                            data.Add(temp);
                            start += len;
                        }

                        if (start == buffer.Position)
                        {
                            buffer.Clear();
                        }
                    }
                    else
                    {
                        if (cache.Position == 0)
                        {
                            cache.Data = new byte[len];
                            cache.Size = len;
                        }
                        Array.Copy(buffer.Data, start, cache.Data, cache.Position, buffer.Position - start);
                        cache.Position = cache.Position + buffer.Position - start;
                        start = cache.Position;
                        buffer.Clear();
                    }
                }
            }

            return true;
        }

        public override bool Send(byte[] data)
        {
            return base.Send(SocketHelper.ToSendData(data));
        }
    }
}

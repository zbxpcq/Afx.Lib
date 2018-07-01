using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Sockets
{
    /// <summary>
    /// UpstreamEndPoint
    /// </summary>
    public class UpstreamEndPoint
    {
        public UpstreamEndPoint(string host, int port, int weight = 1)
        {
            this.Host = host;
            this.Port = port;
            this.Weight = weight;
            this.IsAvailable = true;
        }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public int Weight { get; private set; }

        internal bool IsAvailable { get; set; }

        private int clientCount = 0;
        internal int ClientCount { get { return clientCount; } }

        internal int AddClient()
        {
            return System.Threading.Interlocked.Increment(ref clientCount);
        }

        internal int RemoveClient()
        {
            return System.Threading.Interlocked.Decrement(ref clientCount);
        }
    }
}

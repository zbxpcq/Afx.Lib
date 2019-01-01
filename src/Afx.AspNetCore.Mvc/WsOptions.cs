using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.AspNetCore.Mvc
{
    public class WsOptions
    {
        public WsMiddleware Middleware { get; private set; }
        public WebSocketOptions SocketOptions { get; private set; }

        public WsOptions()
        {
            this.Middleware = new WsMiddleware();
            this.SocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2),
                ReceiveBufferSize = 4 * 1024
            };
        }
    }
}

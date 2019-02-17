using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.AspNetCore.Mvc
{
    public class WsOptions
    {
        internal WsMiddleware Middleware { get; private set; }
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

        public WsOptions Add<T>(string path) where T : class, IWsHandler, new()
        {
            this.Middleware.Add<T>(path);

            return this;
        }
    }
}

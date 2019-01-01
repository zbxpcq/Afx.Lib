using System;
using System.Collections.Generic;
using System.Text;
using Afx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WsExtensions
    {
        private static WsOptions wsOptions;
        public static IApplicationBuilder UseWs(this IApplicationBuilder app, Action<WsOptions> func)
        {
            bool isconfig = false;
            if (wsOptions == null)
            {
                wsOptions = new WsOptions();
                isconfig = true;
            }
            func(wsOptions);
            if (isconfig)
            {
                app.UseWebSockets(wsOptions.SocketOptions);
                app.Use(wsOptions.Middleware.Invoke);
            }

            return app;
        }
    }
}

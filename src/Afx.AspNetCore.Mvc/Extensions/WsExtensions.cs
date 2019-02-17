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
        private static bool isconfig = true;
        public static IApplicationBuilder UseWs(this IApplicationBuilder app, Action<WsOptions> func)
        {
            if (wsOptions == null)
            {
                wsOptions = new WsOptions();
            }
            func(wsOptions);
            if (isconfig)
            {
                isconfig = false;
                app.UseWebSockets(wsOptions.SocketOptions);
                app.Use(wsOptions.Middleware.Invoke);
            }

            return app;
        }
    }
}

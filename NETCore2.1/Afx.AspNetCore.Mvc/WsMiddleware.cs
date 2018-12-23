using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;

namespace Afx.AspNetCore.Mvc
{
    public class WsMiddleware
    {
        private Dictionary<string, Type> actionDic;

        public void Add<T>(string path) where T: class, IWsHandler, new()
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            this.actionDic[path] = typeof(T);
        }

        public WsMiddleware()
        {
            this.actionDic = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            var path = context.Request.Path.ToString();
            Type t = null;
            if (this.actionDic.TryGetValue(path, out t))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    try
                    {
                        using (var hander = Activator.CreateInstance(t) as IWsHandler)
                        {
                            await hander.Invoke(context);
                        }
                    }
                    catch(Exception ex)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                await next();
            }
            await next();
        }
    }
}

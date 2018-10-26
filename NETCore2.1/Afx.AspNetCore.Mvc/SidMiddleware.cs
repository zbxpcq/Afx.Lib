using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Afx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Afx.AspNetCore.Mvc
{
    /// <summary>
    /// sid  Middleware
    /// </summary>
    public class SidMiddleware
    {
        private SidOption option;
        private RequestDelegate next;
        /// <summary>
        /// SidMiddleware
        /// </summary>
        /// <param name="next"></param>
        /// <param name="option"></param>
        public SidMiddleware(RequestDelegate next, SidOption option)
        {
            if (option == null) throw new ArgumentNullException("option");
            this.option = option;
            this.next = next;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            this.option.BeginRequestCallback?.Invoke(context);

            string sid = null;
            if (this.option.IsQueryString)
            {
                Microsoft.Extensions.Primitives.StringValues v;
                if (context.Request.Headers.TryGetValue(this.option.Name, out v) && v.Count > 0)
                {
                    sid = v.FirstOrDefault();
                }
            }

            if (string.IsNullOrEmpty(sid) && this.option.IsHeader)
            {
                Microsoft.Extensions.Primitives.StringValues v;
                if (context.Request.Headers.TryGetValue(this.option.Name, out v) && v.Count > 0)
                {
                    sid = v.FirstOrDefault();
                }
            }

            if (string.IsNullOrEmpty(sid) && this.option.IsCookie)
            {
                context.Request.Cookies.TryGetValue(this.option.Name, out sid);
            }

            if (string.IsNullOrEmpty(sid))
            {
                sid = Guid.NewGuid().ToString("n");
                context.Items["__CREATE_SID"] = true;
            }
            else
            {
                context.Items["__CREATE_SID"] = false;
            }

            context.Items[this.option.Name] = sid;
            
            context.Response.OnStarting((o) =>
            {
                var cont = o as HttpContext;
                if (cont != null)
                {
                    bool iswr = false;
                    var co = cont.Items["__CREATE_SID"];
                    if (co is bool) iswr = (bool)co;
                    if (iswr)
                    {
                        var newsid = cont.Items[this.option.Name] as string;
                        if (!string.IsNullOrEmpty(newsid))
                        {
                            if (this.option.IsCookie)
                            {
                                cont.Response.Cookies.Append(this.option.Name, newsid, this.option.Cookie);
                            }

                            if (this.option.IsHeader)
                            {
                                cont.Request.Headers.Add(this.option.Name, newsid);
                            }
                        }
                    }
                    this.option.EndRequestCallback?.Invoke(cont);
                }

                return Task.CompletedTask;
            }, context);

            return this.next.Invoke(context);
        }
    }
}

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

        private string OnEncryptSid(string value)
        {
            string s = value;
            if (this.option.EncryptSid != null && !string.IsNullOrEmpty(value))
            {
                s = this.option.EncryptSid(value);
            }

            return s;
        }

        private string OnDecryptSid(string value)
        {
            string s = value;
            if (this.option.DecryptSid != null && !string.IsNullOrEmpty(value))
            {
                s = this.option.DecryptSid(value);
            }

            return s;
        }
        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            this.option.BeginRequestCallback?.Invoke(context);

            string sid = null;
            if (this.option.IsQueryString)
            {
                Microsoft.Extensions.Primitives.StringValues v;
                if (context.Request.Headers.TryGetValue(this.option.Name, out v) && v.Count > 0)
                {
                    sid = this.OnDecryptSid(v.FirstOrDefault());
                }
            }

            if (string.IsNullOrEmpty(sid) && this.option.IsHeader)
            {
                Microsoft.Extensions.Primitives.StringValues v;
                if (context.Request.Headers.TryGetValue(this.option.Name, out v) && v.Count > 0)
                {
                    sid = this.OnDecryptSid(v.FirstOrDefault());
                }
            }

            if (string.IsNullOrEmpty(sid) && this.option.IsCookie)
            {
                string s = null;
                if (context.Request.Cookies.TryGetValue(this.option.Name, out s))
                {
                    sid = this.OnDecryptSid(s);
                }
            }

            this.option.RequestSidCallback?.Invoke(sid);

            context.Response.OnStarting((o) =>
            {
                var oldsid = o as string;
                var newsid = oldsid;
                if (this.option.ResponseSidCallback != null) newsid = this.option.ResponseSidCallback(oldsid);

                if (oldsid != newsid && !string.IsNullOrEmpty(newsid))
                {
                    var s = this.OnEncryptSid(newsid);
                    if (this.option.IsCookie)
                    {
                        context.Response.Cookies.Append(this.option.Name, s, this.option.Cookie);
                    }

                    if (this.option.IsHeader)
                    {
                        context.Request.Headers.Add(this.option.Name, s);
                    }
                }

                this.option.EndRequestCallback?.Invoke(context);

                return Task.CompletedTask;
            }, sid);

            await this.next.Invoke(context);
        }
    }
}

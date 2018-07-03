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

            string oldSid = null;
            string s = null;
            if (context.Request.Cookies.TryGetValue(this.option.Name, out s))
            {
                oldSid = this.OnDecryptSid(s);
            }

            if (this.option.IsHeader && string.IsNullOrEmpty(oldSid))
            {
                Microsoft.Extensions.Primitives.StringValues v;
                if (context.Request.Headers.TryGetValue(this.option.Name, out v) && v.Count > 0)
                {
                    oldSid = this.OnDecryptSid(v.FirstOrDefault());
                }
            }

            this.option.RequestSidCallback?.Invoke(oldSid);

            context.Response.OnStarting((o) =>
            {
                var newSid = o as string;
                if (this.option.ResponseSidCallback != null) newSid = this.option.ResponseSidCallback(oldSid);

                if (oldSid != newSid && !string.IsNullOrEmpty(newSid))
                {
                    s = this.OnEncryptSid(newSid);
                    context.Response.Cookies.Append(this.option.Name, s, this.option.Cookie);

                    if (this.option.IsHeader)
                    {
                        context.Request.Headers.Add(this.option.Name, s);
                    }
                }

                this.option.EndRequestCallback?.Invoke(context);

                return Task.CompletedTask;
            }, oldSid);

            await this.next.Invoke(context);
        }
    }
}

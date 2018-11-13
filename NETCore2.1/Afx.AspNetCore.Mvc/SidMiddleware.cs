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

        private string OnEncrypt(string val)
        {
            string result = val;
            if (!string.IsNullOrEmpty(val) && this.option.EncryptCallback != null)
            {
                try { result = this.option.EncryptCallback(val); }
                catch { }
            }

            return result;
        }

        private string OnDecrypt(string val)
        {
            string result = null;
            if (!string.IsNullOrEmpty(val) && this.option.DecryptCallback != null)
            {
                try { result = this.option.DecryptCallback(val); }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
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

            if(!string.IsNullOrEmpty(sid))
            {
                sid = this.OnDecrypt(sid);
            }

            bool iscreate = false;
            if (string.IsNullOrEmpty(sid))
            {
                sid = Guid.NewGuid().ToString("n");
                iscreate = true;
            }

            context.Items[this.option.Name] = sid;

            this.option.BeginRequestCallback?.Invoke(context, sid);

            context.Response.OnStarting((o) =>
            {
                var vt = ((HttpContext, string, bool))o;
                var oldsid = vt.Item2;
                var newsid = this.option.EndRequestCallback?.Invoke(vt.Item1, oldsid) ?? oldsid;
                if (!string.IsNullOrEmpty(newsid) && (oldsid != newsid || vt.Item3))
                {
                    newsid = this.OnEncrypt(newsid);
                    if (this.option.IsCookie)
                    {
                        vt.Item1.Response.Cookies.Append(this.option.Name, newsid, this.option.Cookie);
                    }

                    if (this.option.IsHeader)
                    {
                        vt.Item1.Request.Headers.Add(this.option.Name, newsid);
                    }
                }

                return Task.CompletedTask;
            }, (context, sid, iscreate));

            return this.next.Invoke(context);
        }
    }
}

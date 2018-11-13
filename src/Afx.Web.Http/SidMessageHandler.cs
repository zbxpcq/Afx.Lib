using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Afx.Web.Http
{
    /// <summary>
    /// 
    /// </summary>
    public class SidMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public SidMessageHandler()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerHandler"></param>
        public SidMessageHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {

        }

        private string name = "sid";
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("Name");
                name = value;
            }
        }

        private string cookiePath = "/";
        /// <summary>
        /// 
        /// </summary>
        public string CookiePath
        {
            get { return cookiePath; }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("cookiePath");
                cookiePath = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CookieDomain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool CookieHttpOnly { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public bool CookieSecure { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan? CookieMaxAge { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset? CookieExpires { get; set; }

        /// <summary>
        /// sid 是否存放 IsQueryString (level 1)
        /// </summary>
        public bool IsQueryString { get; set; } = false;

        /// <summary>
        /// sid 是否存放 header  (level 2)
        /// </summary>
        public bool IsHeader { get; set; } = false;

        /// <summary>
        /// sid 是否存放 Cookie  (level 3) 默认true
        /// </summary>
        public bool IsCookie { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public BeginRequestCallback BeginRequestCallback;
        /// <summary>
        /// 
        /// </summary>
        public EndRequestCallback EndRequestCallback;

        public Func<string, string> EncryptCallback;

        public Func<string, string> DecryptCallback;

        private string OnEncrypt(string val)
        {
            string result = val;
            if(!string.IsNullOrEmpty(val) && this.EncryptCallback != null)
            {
                try { result = this.EncryptCallback(val); }
                catch { }
            }

            return result;
        }

        private string OnDecrypt(string val)
        {
            string result = null;
            if (!string.IsNullOrEmpty(val) && this.DecryptCallback != null)
            {
                try { result = this.DecryptCallback(val); }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string sid = null;
            if(this.IsQueryString)
            {
               var queryString = request.RequestUri.ParseQueryString();
                sid = queryString?.GetValues(this.Name)?.FirstOrDefault();
            }

            if(this.IsHeader && string.IsNullOrEmpty(sid))
            {
                IEnumerable<string> v = null;
                if(request.Headers.TryGetValues(this.Name, out v))
                {
                    sid = v?.FirstOrDefault();
                }
            }

            if(this.IsCookie && string.IsNullOrEmpty(sid))
            {
                var cookie = request.Headers.GetCookies(Name).FirstOrDefault();
                if (cookie != null)
                {
                    sid = cookie[this.Name].Value;
                }
            }

            if(!string.IsNullOrEmpty(sid))
            {
                sid = this.OnDecrypt(sid);
            }

            var iscreate = false;
            if(string.IsNullOrEmpty(sid))
            {
                sid = Guid.NewGuid().ToString("n");
                iscreate = true;
            }

            this.BeginRequestCallback?.Invoke(request, sid);


            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            var newSid = this.EndRequestCallback?.Invoke(request, response, sid) ?? sid;

            if (!string.IsNullOrEmpty(newSid) && (sid != newSid || iscreate))
            {
                newSid = this.OnEncrypt(newSid);
                if (this.IsCookie)
                {
                    response.Headers.AddCookies(new CookieHeaderValue[]
                    {
                        new CookieHeaderValue(this.Name, newSid)
                        {
                            HttpOnly = this.CookieHttpOnly,
                            Path = this.CookiePath,
                            Domain = this.CookieDomain,
                            Secure = this.CookieSecure,
                            MaxAge = this.CookieMaxAge,
                            Expires = this.CookieExpires
                        }
                    });
                }

                if(this.IsHeader)
                {
                    response.Headers.Add(this.Name, newSid);
                }
            }
            
            return response;
        }
    }
}

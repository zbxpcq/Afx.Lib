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
        public Func<string, string> EncryptSid;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string OnEncryptSid(string value)
        {
            string s = value;
            if (this.EncryptSid != null && !string.IsNullOrEmpty(value))
            {
                s = this.EncryptSid(value);
            }

            return s;
        }
        /// <summary>
        /// 
        /// </summary>
        public Func<string, string> DecryptSid;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string OnDecryptSid(string value)
        {
            string s = value;
            if (this.DecryptSid != null && !string.IsNullOrEmpty(value))
            {
                s = this.DecryptSid(value);
            }

            return s;
        }
        /// <summary>
        /// 
        /// </summary>
        public RequestSidCallback RequestSidCallback;
        /// <summary>
        /// 
        /// </summary>
        public ResponseSidCallback ResponseSidCallback;
        /// <summary>
        /// 
        /// </summary>
        public BeginRequestCallback BeginRequestCallback;
        /// <summary>
        /// 
        /// </summary>
        public EndRequestCallback EndRequestCallback;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            this.BeginRequestCallback?.Invoke(request);

            string sid = null;
            if(this.IsQueryString)
            {
               var queryString = request.RequestUri.ParseQueryString();
                sid = this.OnDecryptSid(queryString?.GetValues(this.Name)?.FirstOrDefault());
            }

            if(this.IsHeader && string.IsNullOrEmpty(sid))
            {
                IEnumerable<string> v = null;
                if(request.Headers.TryGetValues(this.Name, out v))
                {
                    sid = this.OnDecryptSid(v?.FirstOrDefault());
                }
            }

            if(this.IsCookie && string.IsNullOrEmpty(sid))
            {
                var cookie = request.Headers.GetCookies(Name).FirstOrDefault();
                if (cookie != null)
                {
                    sid = this.OnDecryptSid(cookie[this.Name].Value);
                }
            }
            
            this.RequestSidCallback?.Invoke(sid);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            var newSid = sid;
            if (this.ResponseSidCallback != null) newSid = this.ResponseSidCallback(sid);

            if (sid != newSid && !string.IsNullOrEmpty(newSid))
            {
                var s = this.OnEncryptSid(newSid);
                if (this.IsCookie)
                {
                    response.Headers.AddCookies(new CookieHeaderValue[]
                    {
                    new CookieHeaderValue(this.Name, s)
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
                    response.Headers.Add(this.Name, s);
                }
            }
            
            this.EndRequestCallback?.Invoke(request, response);

            return response;
        }
    }
}

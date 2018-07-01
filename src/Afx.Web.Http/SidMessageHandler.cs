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

        private string cookieDomain = null;
        /// <summary>
        /// 
        /// </summary>
        public string CookieDomain
        {
            get { return cookieDomain; }
            set
            {
                cookieDomain = value;
            }
        }

        private bool cookieHttpOnly = true;
        /// <summary>
        /// 
        /// </summary>
        public bool CookieHttpOnly
        {
            get { return cookieHttpOnly; }
            set
            {
                cookieHttpOnly = value;
            }
        }

        private bool cookieSecure = false;
        /// <summary>
        /// 
        /// </summary>
        public bool CookieSecure
        {
            get { return cookieSecure; }
            set
            {
                cookieSecure = value;
            }
        }

        private TimeSpan? cookieMaxAge = null;
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan? CookieMaxAge
        {
            get { return cookieMaxAge; }
            set
            {
                cookieMaxAge = value;
            }
        }

        private DateTimeOffset? cookieExpires = null;
        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset? CookieExpires
        {
            get { return cookieExpires; }
            set
            {
                cookieExpires = value;
            }
        }

        private bool isHeader = false;
        /// <summary>
        /// 
        /// </summary>
        public bool IsHeader
        {
            get { return this.isHeader; }
            set { this.isHeader = value; }
        }
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

            string oldSid = null;
            var cookie = request.Headers.GetCookies(Name).FirstOrDefault();
            if (cookie != null)
            {
                oldSid = this.OnDecryptSid(cookie[this.Name].Value);
            }

            if(this.IsHeader && string.IsNullOrEmpty(oldSid))
            {
                IEnumerable<string> v = null;
                if(request.Headers.TryGetValues(this.Name, out v))
                {
                    oldSid = this.OnDecryptSid(v?.FirstOrDefault());
                }
            }
            
            this.RequestSidCallback?.Invoke(oldSid);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            var newSid = oldSid;
            if (this.ResponseSidCallback != null) newSid = this.ResponseSidCallback(oldSid);

            if (oldSid != newSid && !string.IsNullOrEmpty(newSid))
            {
                var s = this.OnEncryptSid(newSid);
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

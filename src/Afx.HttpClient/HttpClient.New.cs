using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// HttpClient
    /// </summary>
    public sealed class HttpClient : IDisposable
    {
        private HttpClientHandler handler;
        private System.Net.Http.HttpClient client;

        /// <summary>
        /// 获取或设置一个值，该值指示处理程序是否应跟随重定向响应。
        /// 如果处理器应按照重定向响应，则为 true；否则为 false。 默认值为 true。
        /// </summary>
        public bool AllowAutoRedirect
        {
            get { return this.handler.AllowAutoRedirect; }
            set { this.handler.AllowAutoRedirect = value; }
        }

        /// <summary>
        /// 获取或设置此处理程序使用的身份验证信息。默认值为 null。
        /// </summary>
        public ICredentials Credentials
        {
            get { return this.handler.Credentials; }
            set { this.handler.Credentials = value; }
        }

        /// <summary>
        /// 获取或设置一个值，该值控制默认凭据是否被处理程序随请求一起发送。
        ///  如果使用默认凭据，则为 true；否则为 false。 默认值为false。
        /// </summary>
        public bool UseDefaultCredentials
        {
            get { return this.handler.UseDefaultCredentials; }
            set { this.handler.UseDefaultCredentials = value; }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示处理程序是否随请求发送一个“身份验证”标头。
        /// 处理程序的 true 在发生身份验证之后随请求一起发送 HTTP 授权标头；否则为 false。 默认值为 false。
        /// </summary>
        public bool PreAuthenticate
        {
            get { return this.handler.PreAuthenticate; }
            set { this.handler.PreAuthenticate = value; }
        }

        /// <summary>
        /// 获取或设置处理程序使用的代理信息。
        /// 默认值为 null。
        /// </summary>
        public IWebProxy Proxy
        {
            get { return this.handler.Proxy; }
            set { this.handler.Proxy = value; }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示处理程序是否为请求使用代理。
        /// 如果该管理器应为请求使用代理项，则为 true；否则为 false。 默认值为 true。
        /// </summary>
        public bool UseProxy
        {
            get { return this.handler.UseProxy; }
            set { this.handler.UseProxy = value; }
        }

        /// <summary>
        /// 获取或设置处理程序用于实现 HTTP 内容响应的自动解压缩的解压缩方法。
        /// 默认值为 System.Net.DecompressionMethods.None。
        /// </summary>
        public DecompressionMethods AutomaticDecompression
        {
            get { return this.handler.AutomaticDecompression; }
            set { this.handler.AutomaticDecompression = value; }
        }

        /// <summary>
        /// 获取或设置与此处理程序关联的安全证书集合。
        /// </summary>
        public ClientCertificateOption ClientCertificateOptions
        {
            get { return this.handler.ClientCertificateOptions; }
            set { this.handler.ClientCertificateOptions = value; }
        }

        /// <summary>
        /// 获取或设置用于存储处理程序产生的服务器 Cookie 的 Cookie 容器。
        /// </summary>
        public CookieContainer CookieContainer
        {
            get { return this.handler.CookieContainer; }
            set { this.handler.CookieContainer = value; }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示发送请求时，处理程序是否使用CookieContainer来存储服务器 Cookie 并在发送请求时使用这些 Cookie，则为 true；否则为 false。默认值为 true。
        /// </summary>
        public bool UseCookies
        {
            get { return this.handler.UseCookies; }
            set { this.handler.UseCookies = value; }
        }

        /// <summary>
        /// 获取或设置将跟随的处理程序的重定向的最大数目。默认值为 50。
        /// </summary>
        public int MaxAutomaticRedirections
        {
            get { return this.handler.MaxAutomaticRedirections; }
            set { this.handler.MaxAutomaticRedirections = value; }
        }
        
        /// <summary>
        /// 获取或设置处理程序的使用的请求内容的最大缓冲区大小。
        /// 最大请求内容缓冲区大小（以字节为单位）。 默认值为 2 GB。
        /// </summary>
        public long MaxRequestContentBufferSize
        {
            get { return this.handler.MaxRequestContentBufferSize; }
            set { this.handler.MaxRequestContentBufferSize = value; }
        }

        /// <summary>
        /// 获取或设置发送请求时使用的 Internet 资源的统一资源标识符 (URI) 的基地址。
        /// </summary>
        public Uri BaseAddress
        {
            get { return this.client.BaseAddress; }
            set { this.client.BaseAddress = value; }
        }

        /// <summary>
        /// 获取与每个请求一起发送的标题。
        /// </summary>
        public HttpRequestHeaders DefaultRequestHeaders
        {
            get { return this.client.DefaultRequestHeaders; }
        }

        /// <summary>
        /// 获取或设置请求超时前等待的毫秒数。
        /// </summary>
        public TimeSpan Timeout
        {
            get { return this.client.Timeout; }
            set { this.client.Timeout = value; }
        }

        /// <summary>
        /// 获取或设置读取响应内容时要缓冲的最大字节数。此属性的默认值为 2 GB。
        /// </summary>
        public long MaxResponseContentBufferSize
        {
            get { return this.client.MaxResponseContentBufferSize; }
            set { this.client.MaxResponseContentBufferSize = value; }
        }

        /// <summary>
        /// HttpClient
        /// </summary>
        public HttpClient()
        {
            //SetServicePointManager();
            //this.Accept = "text/html, application/xhtml+xml, */*";
            //this.AcceptLanguage = "zh-CN";
            //this.AcceptCharset = "utf-8";


            //this.KeepAlive = false;

            //this.Cookies = new CookieContainer();
            //this.headersDic = new Dictionary<string, string>();
            //this.ClientCertificates = new X509CertificateCollection();

            this.handler = new HttpClientHandler();
            this.client = new System.Net.Http.HttpClient(handler);
            this.handler.UseDefaultCredentials = true;
            this.handler.Credentials = CredentialCache.DefaultCredentials;
            if (this.handler.CookieContainer == null) this.handler.CookieContainer = new CookieContainer();
            this.client.DefaultRequestHeaders.Add("Accept", "text/html, application/xhtml+xml, */*");
            this.client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh");
            this.client.DefaultRequestHeaders.Add("User-Agent", "Afx.HttpClient");

        }

        /// <summary>
        /// HttpClient
        /// </summary>
        /// <param name="baseAddress"></param>
        public HttpClient(string baseAddress)
            : this()
        {
            this.BaseAddress = baseAddress;
        }
        
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
#if NETCOREAPP || NETSTANDARD
            foreach (var cer in this.ClientCertificates) cer.Dispose();
#endif

            this.ClientCertificates.Clear();
            this.headersDic.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Afx.HttpClient
{
    /// <summary>
    /// WebApiClient
    /// </summary>
    public sealed class WebApiClient : IDisposable
    {
        private static HttpClientHandler m_handler;
        private static System.Net.Http.HttpClient m_client;

        /// <summary>
        /// 获取或设置一个值，该值指示处理程序是否应跟随重定向响应。
        /// 如果处理器应按照重定向响应，则为 true；否则为 false。 默认值为 true。
        /// </summary>
        public static bool AllowAutoRedirect
        {
            get { return m_handler.AllowAutoRedirect; }
            set { m_handler.AllowAutoRedirect = value; }
        }

        /// <summary>
        /// 获取或设置此处理程序使用的身份验证信息。默认值为 null。
        /// </summary>
        public static ICredentials Credentials
        {
            get { return m_handler.Credentials; }
            set { m_handler.Credentials = value; }
        }

        /// <summary>
        /// 获取或设置一个值，该值控制默认凭据是否被处理程序随请求一起发送。
        ///  如果使用默认凭据，则为 true；否则为 false。 默认值为false。
        /// </summary>
        public static bool UseDefaultCredentials
        {
            get { return m_handler.UseDefaultCredentials; }
            set { m_handler.UseDefaultCredentials = value; }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示处理程序是否随请求发送一个“身份验证”标头。
        /// 处理程序的 true 在发生身份验证之后随请求一起发送 HTTP 授权标头；否则为 false。 默认值为 false。
        /// </summary>
        public static bool PreAuthenticate
        {
            get { return m_handler.PreAuthenticate; }
            set { m_handler.PreAuthenticate = value; }
        }

        /// <summary>
        /// 获取或设置处理程序使用的代理信息。
        /// 默认值为 null。
        /// </summary>
        public static IWebProxy Proxy
        {
            get { return m_handler.Proxy; }
            set { m_handler.Proxy = value; }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示处理程序是否为请求使用代理。
        /// 如果该管理器应为请求使用代理项，则为 true；否则为 false。 默认值为 true。
        /// </summary>
        public static bool UseProxy
        {
            get { return m_handler.UseProxy; }
            set { m_handler.UseProxy = value; }
        }

        /// <summary>
        /// 获取或设置处理程序用于实现 HTTP 内容响应的自动解压缩的解压缩方法。
        /// 默认值为 System.Net.DecompressionMethods.None。
        /// </summary>
        public static DecompressionMethods AutomaticDecompression
        {
            get { return m_handler.AutomaticDecompression; }
            set { m_handler.AutomaticDecompression = value; }
        }

        /// <summary>
        /// 获取或设置与此处理程序关联的安全证书集合。
        /// </summary>
        public static ClientCertificateOption ClientCertificateOptions
        {
            get { return m_handler.ClientCertificateOptions; }
            set { m_handler.ClientCertificateOptions = value; }
        }

        /// <summary>
        /// 获取或设置用于存储处理程序产生的服务器 Cookie 的 Cookie 容器。
        /// </summary>
        public static CookieContainer CookieContainer
        {
            get { return m_handler.CookieContainer; }
            set { m_handler.CookieContainer = value; }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示发送请求时，处理程序是否使用CookieContainer来存储服务器 Cookie 并在发送请求时使用这些 Cookie，则为 true；否则为 false。默认值为 true。
        /// </summary>
        public static bool UseCookies
        {
            get { return m_handler.UseCookies; }
            set { m_handler.UseCookies = value; }
        }

        /// <summary>
        /// 获取或设置将跟随的处理程序的重定向的最大数目。默认值为 50。
        /// </summary>
        public static int MaxAutomaticRedirections
        {
            get { return m_handler.MaxAutomaticRedirections; }
            set { m_handler.MaxAutomaticRedirections = value; }
        }
        
        /// <summary>
        /// 获取或设置处理程序的使用的请求内容的最大缓冲区大小。
        /// 最大请求内容缓冲区大小（以字节为单位）。 默认值为 2 GB。
        /// </summary>
        public static long MaxRequestContentBufferSize
        {
            get { return m_handler.MaxRequestContentBufferSize; }
            set { m_handler.MaxRequestContentBufferSize = value; }
        }
        /// <summary>
        /// 使用默认（系统）代理时，获取或设置要提交到默认代理服务器进行身份验证的凭据。 只有在 UseProxy 设置为 true 且 Proxy 设置为 null 时才使用默认代理。
        /// </summary>
        public static ICredentials DefaultProxyCredentials
        {
            get { return m_handler.DefaultProxyCredentials; }
            set { m_handler.DefaultProxyCredentials = value; }
        }

        /// <summary>
        /// 获取与对服务器的请求相关联的安全证书集合。
        /// </summary>
        public static X509CertificateCollection ClientCertificates
        {
            get { return m_handler.ClientCertificates; }
        }

        /// <summary>
        /// 获取或设置 HttpClientHandler 对象管理的 HttpClient 对象所用的 TLS/SSL 协议,仅限 .NET Framework 4.7.1：不实现此属性。
        /// </summary>
        public static SslProtocols SslProtocols
        {
            get { return m_handler.SslProtocols; }
            set { m_handler.SslProtocols = value; }
        }
        /// <summary>
        /// 获取或设置响应标头的最大长度，以千字节（1024 字节）为单位。 例如，如果该值为 64，那么允许的最大响应标头长度为 65536 字节。
        /// </summary>
        public static int MaxResponseHeadersLength
        {
            get { return m_handler.MaxResponseHeadersLength; }
            set { m_handler.MaxResponseHeadersLength = value; }
        }
        /// <summary>
        /// 获取或设置使用 HttpClient 对象发出请求时允许的最大并发连接数（每个服务器终结点）。 请注意，该限制针对每个服务器终结点，例如，值为 256 表示允许 256 个到 http://www.adatum.com/ 的并发连接，以及另外 256 个到 http://www.adventure-works.com/ 的并发连接。
        /// </summary>
        public static int MaxConnectionsPerServer
        {
            get { return m_handler.MaxConnectionsPerServer; }
            set { m_handler.MaxConnectionsPerServer = value; }
        }
        /// <summary>
        /// 获取或设置一个值，该值指示是否根据证书颁发机构吊销列表检查证书。如果检查证书吊销列表，则为 true；否则为 false。
        /// 仅限 .NET Framework 4.7.1：不实现此属性。
        /// </summary>
        public static bool CheckCertificateRevocationList
        {
            get { return m_handler.CheckCertificateRevocationList; }
            set { m_handler.CheckCertificateRevocationList = value; }
        }

        public static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback
        {
            get { return m_handler.ServerCertificateCustomValidationCallback; }
            set { m_handler.ServerCertificateCustomValidationCallback = value; }
        }

        /// <summary>
        /// 获取与每个请求一起发送的标题。
        /// </summary>
        public static HttpRequestHeaders DefaultRequestHeaders
        {
            get { return m_client.DefaultRequestHeaders; }
        }

        /// <summary>
        /// 获取或设置请求超时前等待的毫秒数。
        /// </summary>
        public static TimeSpan Timeout
        {
            get { return m_client.Timeout; }
            set { m_client.Timeout = value; }
        }

        /// <summary>
        /// 获取或设置读取响应内容时要缓冲的最大字节数。此属性的默认值为 2 GB。
        /// </summary>
        public static long MaxResponseContentBufferSize
        {
            get { return m_client.MaxResponseContentBufferSize; }
            set { m_client.MaxResponseContentBufferSize = value; }
        }

        private static bool ServerCertificateValidation(HttpRequestMessage httpRequestMessage, X509Certificate2 x509Certificate2, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static WebApiClient()
        {
            m_handler = new HttpClientHandler();
            m_client = new System.Net.Http.HttpClient(m_handler);
            m_handler.UseDefaultCredentials = true;
            m_handler.Credentials = CredentialCache.DefaultCredentials;
            m_handler.ServerCertificateCustomValidationCallback = ServerCertificateValidation;

            //text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3
            m_client.DefaultRequestHeaders.Accept.Clear();
            m_client.DefaultRequestHeaders.Accept.TryParseAdd("text/html");
            m_client.DefaultRequestHeaders.Accept.TryParseAdd("*/*");
            m_client.DefaultRequestHeaders.AcceptLanguage.Clear();
            m_client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh-CN");
            m_client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh");
            m_client.DefaultRequestHeaders.AcceptCharset.Clear();
            m_client.DefaultRequestHeaders.AcceptCharset.TryParseAdd("utf-8");
            m_client.DefaultRequestHeaders.UserAgent.Clear();
            m_client.DefaultRequestHeaders.UserAgent.TryParseAdd("Afx.HttpClient");

            m_client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };

            try { m_handler.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12; }
            catch { }
        }

        /// <summary>
        /// Accept
        /// </summary>
        public string Accept { get; set; }
        /// <summary>
        /// AcceptLanguage
        /// </summary>
        public string AcceptLanguage { get; set; }
        /// <summary>
        /// AcceptCharset
        /// </summary>
        public string AcceptCharset { get; set; }
        /// <summary>
        /// UserAgent
        /// </summary>
        public string UserAgent { get; set; }

        public Version Version { get; set; }

        public string Host { get; set; }

        public string From { get; set; }

        public Uri Referrer { get; set; }

        public IDictionary<string, string> Headers { get; private set; }

        private string m_baseAddress = null;
        /// <summary>
        /// BaseAddress
        /// </summary>
        public string BaseAddress
        {
            get
            {
                return this.m_baseAddress;
            }
            set
            {
                if (string.IsNullOrEmpty(value)
                    || value.ToLower() == "http://"
                    || value.ToLower() == "https://"
                    || !(value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("BaseAddress is error!");
                }

                this.m_baseAddress = value;
            }
        }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// HttpClient
        /// </summary>
        public WebApiClient()
        {
            this.Init();
        }

        private void Init()
        {
            this.Headers = new Dictionary<string, string>();
            this.IsDisposed = false;
        }

        public WebApiClient(string baseAddress)
        {
            this.BaseAddress = baseAddress;
            this.Init();
        }

        private string BuildUrl(string url)
        {
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return url;

            if (string.IsNullOrEmpty(this.BaseAddress))
                throw new ArgumentException("Request BaseAddress is empty!");

            if (string.IsNullOrEmpty(url)) return this.BaseAddress;

            if (url.StartsWith("/") && this.BaseAddress.EndsWith("/"))
                url = this.BaseAddress + url.TrimStart('/');
            else if (!url.StartsWith("/") && !this.BaseAddress.EndsWith("/"))
                url = this.BaseAddress + "/" + url;
            else
                url = this.BaseAddress + url;

            return url;
        }

        private void SetDefault(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(this.Accept))  request.Headers.Accept.TryParseAdd(this.Accept);
            if (!string.IsNullOrEmpty(this.AcceptLanguage)) request.Headers.AcceptLanguage.TryParseAdd(this.AcceptLanguage);
            if (!string.IsNullOrEmpty(this.AcceptCharset)) request.Headers.AcceptCharset.TryParseAdd(this.AcceptCharset);
            if (!string.IsNullOrEmpty(this.UserAgent)) request.Headers.UserAgent.TryParseAdd(this.UserAgent);
            if (this.Version != null) request.Version = this.Version;
            if (!string.IsNullOrEmpty(this.Host)) request.Headers.Host = this.Host;
            if (!string.IsNullOrEmpty(this.From)) request.Headers.From = this.From;
            if (this.Referrer != null) request.Headers.Referrer = this.Referrer;
            foreach(KeyValuePair<string, string> kv in this.Headers)
            {
               if(!string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value))
                    request.Headers.Add(kv.Key, kv.Value);
            }
        }

        #region get
        public async Task<BytesBody> GetBytesAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = m_client.SendAsync(request);
                BytesBody result = new BytesBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public BytesBody GetBytes(string url)
        {
            var t = this.GetBytesAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> GetStreamAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = m_client.SendAsync(request);
                StreamBody result = new StreamBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StreamBody GetStream(string url)
        {
            var t = this.GetStreamAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> GetAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = m_client.SendAsync(request);
                StringBody result = new StringBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StringBody Get(string url)
        {
            var t = this.GetAsync(url);
            t.Wait();

            return t.Result;
        }
        #endregion

        #region delete
        public async Task<BytesBody> DeleteBytesAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = m_client.SendAsync(request);
                BytesBody result = new BytesBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public BytesBody DeleteBytes(string url)
        {
            var t = this.DeleteBytesAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> DeleteStreamAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = m_client.SendAsync(request);
                StreamBody result = new StreamBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StreamBody DeleteStream(string url)
        {
            var t = this.DeleteStreamAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> DeleteAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = m_client.SendAsync(request);
                StringBody result = new StringBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StringBody Delete(string url)
        {
            var t = this.DeleteAsync(url);
            t.Wait();

            return t.Result;
        }
        #endregion

        #region post
        public async Task<BytesBody> PostBytesAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = m_client.SendAsync(request);
                    
                    BytesBody result = new BytesBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public BytesBody PostBytes(string url, FormData formData)
        {
            var t = this.PostBytesAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> PostStreamAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = m_client.SendAsync(request);
                    
                    StreamBody result = new StreamBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public StreamBody PostStream(string url, FormData formData)
        {
            var t = this.PostStreamAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> PostAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = m_client.SendAsync(request);

                    StringBody result = new StringBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public StringBody Post(string url, FormData formData)
        {
            var t = this.PostAsync(url, formData);
            t.Wait();

            return t.Result;
        }
        #endregion

        #region put
        public async Task<BytesBody> PutBytesAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = m_client.SendAsync(request);
                    
                    BytesBody result = new BytesBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public BytesBody PutBytes(string url, FormData formData)
        {
            var t = this.PutBytesAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> PutStreamAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = m_client.SendAsync(request);
                    
                    StreamBody result = new StreamBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public StreamBody PutStream(string url, FormData formData)
        {
            var t = this.PutStreamAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> PutAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = m_client.SendAsync(request);
                    
                    StringBody result = new StringBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public StringBody Put(string url, FormData formData)
        {
            var t = this.PutAsync(url, formData);
            t.Wait();

            return t.Result;
        }
        #endregion

        private List<IDisposable> disposables;
        private void AddDispose(IDisposable dis)
        {
            if (dis == null) return;
            if (this.disposables == null) this.disposables = new List<IDisposable>();
            this.disposables.Add(dis);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if(this.disposables != null)
            {
                foreach (var dis in this.disposables)
                    dis.Dispose();
                this.disposables = null;
            }
            if (this.Headers != null) this.Headers.Clear();
            this.Headers = null;
            this.m_baseAddress = null;
            this.Accept = null;
            this.AcceptCharset = null;
            this.AcceptLanguage = null;
            this.From = null;
            this.Host = null;
            this.Referrer = null;
            this.UserAgent = null;
            this.Version = null;
        }
    }
}
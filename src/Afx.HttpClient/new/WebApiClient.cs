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
        /// 使用默认（系统）代理时，获取或设置要提交到默认代理服务器进行身份验证的凭据。 只有在 UseProxy 设置为 true 且 Proxy 设置为 null 时才使用默认代理。
        /// </summary>
        public ICredentials DefaultProxyCredentials
        {
            get { return this.handler.DefaultProxyCredentials; }
            set { this.handler.DefaultProxyCredentials = value; }
        }

        /// <summary>
        /// 获取与对服务器的请求相关联的安全证书集合。
        /// </summary>
        public X509CertificateCollection ClientCertificates
        {
            get { return this.handler.ClientCertificates; }
        }

        /// <summary>
        /// 获取或设置 HttpClientHandler 对象管理的 HttpClient 对象所用的 TLS/SSL 协议,仅限 .NET Framework 4.7.1：不实现此属性。
        /// </summary>
        public SslProtocols SslProtocols
        {
            get { return this.handler.SslProtocols; }
            set { this.handler.SslProtocols = value; }
        }
        /// <summary>
        /// 获取或设置响应标头的最大长度，以千字节（1024 字节）为单位。 例如，如果该值为 64，那么允许的最大响应标头长度为 65536 字节。
        /// </summary>
        public int MaxResponseHeadersLength
        {
            get { return this.handler.MaxResponseHeadersLength; }
            set { this.handler.MaxResponseHeadersLength = value; }
        }
        /// <summary>
        /// 获取或设置使用 HttpClient 对象发出请求时允许的最大并发连接数（每个服务器终结点）。 请注意，该限制针对每个服务器终结点，例如，值为 256 表示允许 256 个到 http://www.adatum.com/ 的并发连接，以及另外 256 个到 http://www.adventure-works.com/ 的并发连接。
        /// </summary>
        public int MaxConnectionsPerServer
        {
            get { return this.handler.MaxConnectionsPerServer; }
            set { this.handler.MaxConnectionsPerServer = value; }
        }
        /// <summary>
        /// 获取或设置一个值，该值指示是否根据证书颁发机构吊销列表检查证书。如果检查证书吊销列表，则为 true；否则为 false。
        /// 仅限 .NET Framework 4.7.1：不实现此属性。
        /// </summary>
        public bool CheckCertificateRevocationList
        {
            get { return this.handler.CheckCertificateRevocationList; }
            set { this.handler.CheckCertificateRevocationList = value; }
        }

        public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback
        {
            get { return this.handler.ServerCertificateCustomValidationCallback; }
            set { this.handler.ServerCertificateCustomValidationCallback = value; }
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

        private static bool ServerCertificateValidation(HttpRequestMessage httpRequestMessage, X509Certificate2 x509Certificate2, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// HttpClient
        /// </summary>
        public WebApiClient()
        {
            this.handler = new HttpClientHandler();
            this.client = new System.Net.Http.HttpClient(handler);
            this.handler.UseDefaultCredentials = true;
            this.handler.Credentials = CredentialCache.DefaultCredentials;
            this.handler.ServerCertificateCustomValidationCallback = ServerCertificateValidation;

            //text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.TryParseAdd("text/html");
            this.client.DefaultRequestHeaders.Accept.TryParseAdd("application/xhtml+xml");
            this.client.DefaultRequestHeaders.Accept.TryParseAdd("*/*");
            this.client.DefaultRequestHeaders.AcceptLanguage.Clear();
            this.client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh-CN");
            this.client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh");
            this.client.DefaultRequestHeaders.AcceptCharset.Clear();
            this.client.DefaultRequestHeaders.AcceptCharset.TryParseAdd("utf-8");
            this.client.DefaultRequestHeaders.UserAgent.Clear();
            this.client.DefaultRequestHeaders.UserAgent.TryParseAdd("Afx.HttpClient");

            this.client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };

            try { this.handler.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12; }
            catch { }
        }

        #region get
        public async Task<BytesBody> GetBytesAsync(string url)
        {
            var t = this.client.GetAsync(url);

            BytesBody result = new BytesBody(t);
            this.AddDispose(result);
            await result.Proc();

            return result;
        }

        public BytesBody GetBytes(string url)
        {
            var t = this.GetBytesAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> GetStreamAsync(string url)
        {
            var t = this.client.GetAsync(url);

            StreamBody result = new StreamBody(t);
            this.AddDispose(result);
            await result.Proc();

            return result;
        }

        public StreamBody GetStream(string url)
        {
            var t = this.GetStreamAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> GetStringAsync(string url)
        {
            var t = this.client.GetAsync(url);

            StringBody result = new StringBody(t);
            this.AddDispose(result);
            await result.Proc();

            return result;
        }

        public StringBody GetString(string url)
        {
            var t = this.GetStringAsync(url);
            t.Wait();

            return t.Result;
        }
        #endregion

        #region delete
        public async Task<BytesBody> DeleteBytesAsync(string url)
        {
            var t = this.client.DeleteAsync(url);

            BytesBody result = new BytesBody(t);
            this.AddDispose(result);
            await result.Proc();

            return result;
        }

        public BytesBody DeleteBytes(string url)
        {
            var t = this.DeleteBytesAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> DeleteStreamAsync(string url)
        {
            var t = this.client.DeleteAsync(url);

            StreamBody result = new StreamBody(t);
            this.AddDispose(result);
            await result.Proc();

            return result;
        }

        public StreamBody DeleteStream(string url)
        {
            var t = this.DeleteStreamAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> DeleteStringAsync(string url)
        {
            var t = this.client.DeleteAsync(url);

            StringBody result = new StringBody(t);
            this.AddDispose(result);
            await result.Proc();

            return result;
        }

        public StringBody DeleteString(string url)
        {
            var t = this.DeleteStringAsync(url);
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
                var t = this.client.PostAsync(url, content);

                BytesBody result = new BytesBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
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
                var t = this.client.PostAsync(url, content);

                StreamBody result = new StreamBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StreamBody PostStream(string url, FormData formData)
        {
            var t = this.PostStreamAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> PostStringAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                var t = this.client.PostAsync(url, content);

                StringBody result = new StringBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StringBody PostString(string url, FormData formData)
        {
            var t = this.PostStringAsync(url, formData);
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
                var t = this.client.PutAsync(url, content);

                BytesBody result = new BytesBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
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
                var t = this.client.PutAsync(url, content);

                StreamBody result = new StreamBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StreamBody PutStream(string url, FormData formData)
        {
            var t = this.PutStreamAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> PutStringAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                var t = this.client.PutAsync(url, content);

                StringBody result = new StringBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StringBody PutString(string url, FormData formData)
        {
            var t = this.PutStringAsync(url, formData);
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
            if (this.client != null) this.client.Dispose();
            if (this.handler != null)
            {
                try
                {
                    foreach (X509Certificate cer in this.handler.ClientCertificates)
                        cer.Dispose();
                }
                catch { }

                this.handler.Dispose();
            }
            if(this.disposables != null)
            {
                foreach (var dis in this.disposables)
                    dis.Dispose();
                this.disposables = null;
            }
            this.client = null;
            this.handler = null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Collections.ObjectModel;

using Afx.Web.Http;
using System.Threading;
using System.Net;

namespace System.Web.Http
{
    /// <summary>
    /// AfxController
    /// </summary>
    public abstract class AfxController : ApiController
    {
        private NameValueCollection m_form;
        /// <summary>
        /// Form
        /// </summary>
        internal protected virtual NameValueCollection Form
        {
            get
            {
                if (this.m_form == null)
                    this.GetFormData();

                return this.m_form;
            }
        }

        private Collection<MultipartFileData> m_files;
        /// <summary>
        /// Files
        /// </summary>
        protected Collection<MultipartFileData> Files
        {
            get
            {
                if (this.m_files == null)
                    this.GetFormData();

                return this.m_files;
            }
        }

        private bool m_isJson;
        /// <summary>
        /// IsJson
        /// </summary>
        protected bool IsJson
        {
            get
            {
                this.GetFormData();

                return this.m_isJson;
            }
        }

        private string m_requestBody;
        /// <summary>
        /// RequestBody
        /// </summary>
        protected virtual string RequestBody
        {
            get
            {
                this.GetFormData();

                return this.m_requestBody;
            }
        }

        private void GetFormData()
        {
            if (this.Request == null)
            {
                return;
            }

            if (this.m_form == null && this.Request.Method != HttpMethod.Get)
            {
                this.m_isJson = false;
                this.m_requestBody = null;
                if (this.Request.Content.IsFormData())
                {
                    var task = this.Request.Content.ReadAsFormDataAsync();
                    task.Wait();
                    this.m_form = task.Result;
                }
                else if (this.Request.Content.IsMimeMultipartContent())
                {
                    string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
                    if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                    MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(tempPath);
                    var task = this.Request.Content.ReadAsMultipartAsync(provider);
                    if(!task.Wait(5 * 1000))
                    {

                    }
                    this.m_form = provider.FormData;
                    this.m_files = provider.FileData;
                }
                else
                {
                    var contentType = this.Request.Content.Headers.ContentType;
                    if (contentType != null)
                    {
                        this.m_isJson = string.Equals(contentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(contentType.MediaType, "json", StringComparison.OrdinalIgnoreCase);
                    }

                    var buffer = this.GetRequestBytes();
                    if (buffer != null && buffer.Length > 0)
                    {
                        this.m_requestBody = Encoding.UTF8.GetString(buffer);
                    }
                }
            }

            if (this.m_form == null) this.m_form = new NameValueCollection(0);
            if (this.m_files == null) this.m_files = new Collection<MultipartFileData>();

        }

        private byte[] m_readByteArray;
        private bool m_isReadByteArray = false;
        /// <summary>
        /// GetRequestBytes
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] GetRequestBytes()
        {
            if (!this.m_isReadByteArray)
            {
                this.m_isReadByteArray = true;
                var task = this.Request.Content.ReadAsByteArrayAsync();
                task.Wait();
                this.m_readByteArray = task.Result;
            }

            byte[] result = new byte[0];
            if(this.m_readByteArray != null)
            {
                result = new byte[this.m_readByteArray.Length];
                Array.Copy(this.m_readByteArray, result, result.Length);
            }

            return this.m_readByteArray;
        }

        private NameValueCollection m_queryString;
        /// <summary>
        /// QueryString
        /// </summary>
        internal protected NameValueCollection QueryString
        {
            get
            {
                if (this.m_queryString == null)
                {
                    m_queryString = this.Request.RequestUri.ParseQueryString();
                }

                return this.m_queryString;
            }
        }

        private ModelStateBinder m_modelState;
        /// <summary>
        /// GetModel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual T GetModel<T>()
        {
            T model = default(T);
            if (this.IsJson)
            {
                if (!string.IsNullOrEmpty(this.RequestBody))
                {
                    try { model = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(this.RequestBody); }
                    catch { }
                }
            }
            else
            {
                if (this.m_modelState == null) this.m_modelState = new ModelStateBinder(this);
                model = this.m_modelState.Get<T>();
            }
            
            return model;
        }
        /// <summary>
        /// ValidModel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual bool ValidModel<T>(T model)
        {
            if (this.m_modelState == null) this.m_modelState = new ModelStateBinder(this);

            return this.m_modelState.IsValid(model);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_form = null;
            this.m_files = null;
            this.m_modelState = null;
            this.m_queryString = null;

            base.Dispose(disposing);
        }
    }
}

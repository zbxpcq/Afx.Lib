using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// 请求 FormData
    /// </summary>
    public abstract class FormData : IDisposable
    {
        /// <summary>
        /// 请求 Content Encoding
        /// </summary>
        public Encoding ContentEncoding { get; protected set; }

        /// <summary>
        /// 请求 ContentType
        /// </summary>
        public string ContentType { get; protected set; }

        /// <summary>
        /// Serialize 请求 数据
        /// </summary>
        /// <param name="stream"></param>
        public abstract HttpContent GetContent();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ContentEncoding = null;
                this.ContentType = null;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Afx.HttpClient
{
    /// <summary>
    /// ByteContent
    /// </summary>
    public sealed class BytesBody : HttpBody
    {
        /// <summary>
        /// Body
        /// </summary>
        public byte[] Body { get; private set; }

        /// <summary>
        /// ByteContent
        /// </summary>
        internal BytesBody(Task<HttpResponseMessage> task)
            : base(task)
        {
            
        }

        protected override async Task<bool> Read(HttpResponseMessage httpResponse)
        {
            this.Body = await httpResponse.Content.ReadAsByteArrayAsync();

            return true;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Body = null;
            }
            base.Dispose(disposing);
        }
    }
}

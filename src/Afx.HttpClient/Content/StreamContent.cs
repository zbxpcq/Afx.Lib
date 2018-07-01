using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Afx.HttpClient
{
    /// <summary>
    /// StreamContent
    /// </summary>
    public sealed class StreamContent : HttpContent
    {
        /// <summary>
        /// Stream
        /// </summary>
        public Stream Stream { get; set; }
        /// <summary>
        /// StreamContent
        /// </summary>
        public StreamContent()
        {
            this.Stream = null;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.Stream != null)
            {
                try { this.Stream.Dispose(); }
                catch { }
            }
            this.Stream = null;
        }
    }
}

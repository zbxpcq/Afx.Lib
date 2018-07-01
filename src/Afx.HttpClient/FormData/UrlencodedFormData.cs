using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// application/x-www-form-urlencoded 表单提交
    /// </summary>
    public class UrlencodedFormData : FormData
    {
        private Dictionary<string, string> paramDic;
        /// <summary>
        /// 
        /// </summary>
        public UrlencodedFormData()
        {
            this.ContentEncoding = Encoding.UTF8;
            this.paramDic = new Dictionary<string, string>();
            this.ContentType = "application/x-www-form-urlencoded";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddParam(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                this.paramDic[key] = value ?? "";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetParam(string key)
        {
            if (this.paramDic.ContainsKey(key))
            {
                return this.paramDic[key];
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void RemoveParam(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                this.paramDic.Remove(key);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public override void Serialize(Stream stream)
        {
            StringBuilder text = new StringBuilder();
            foreach (var kv in this.paramDic)
            {
                text.AppendFormat("&{0}={1}", kv.Key, Uri.EscapeDataString(kv.Value));
            }

            if (text.Length > 0)
            {
                text.Remove(0, 1);
                byte[] buffer = this.ContentEncoding.GetBytes(text.ToString());
                stream.Write(buffer, 0, buffer.Length);
            }

           // text.Clear();
            text = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override long GetLength()
        {
            long length = 0;
            StringBuilder text = new StringBuilder();
            foreach (var kv in this.paramDic)
            {
                text.AppendFormat("&{0}={1}", kv.Key, Uri.EscapeDataString(kv.Value));
            }

            if (text.Length > 0)
            {
                text.Remove(0, 1);
                length = this.ContentEncoding.GetByteCount(text.ToString());
            }

            //text.Clear();
            text = null;

            return length;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.paramDic != null)
                this.paramDic.Clear();
            this.paramDic = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Afx.IO
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FindStream: Stream
    {
        private Stream stream;
        private bool disposingStream = true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public FindStream(Stream stream)
            : this(stream, true)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="disposingStream"></param>
        public FindStream(Stream stream, bool disposingStream)
        {
            this.stream = stream;
            this.disposingStream = disposingStream;
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool CanRead
        {
            get { return this.stream != null ? this.stream.CanRead : false; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool CanSeek
        {
            get { return this.stream != null ? this.stream.CanSeek : false; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool CanWrite
        {
            get { return this.stream != null ? this.stream.CanWrite : false; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Flush()
        {
            if (this.stream != null)
                this.stream.Flush();
        }
        /// <summary>
        /// 
        /// </summary>
        public override long Length
        {
            get { return this.stream == null ? 0 : this.stream.Length; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get
            {
                return this.stream != null ? this.stream.Position : 0;
            }
            set
            {
                if (this.stream != null)
                {
                    if (value < 0)
                    {
                        this.stream.Position = 0;
                    }
                    else if (value > this.Length)
                    {
                        this.stream.Position = this.Length;
                    }
                    else
                    {
                        this.stream.Position = value;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int len = 0;
            if(this.stream != null)
            {
                len = this.stream.Read(buffer, offset, count);
            }

            return len;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long val = 0;
            if (this.stream != null)
            {
                val = this.stream.Seek(offset, origin);
            }

            return val;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            if (this.stream != null)
            {
                this.stream.SetLength(value);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.stream != null)
            {
                this.stream.Write(buffer, offset, count);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            int val = -1;
            if (this.stream != null)
            {
                val = this.stream.ReadByte();
            }

            return val;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Close()
        {
            this.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] ReadKeyData(byte[] key)
        {
            byte[] arr = null;
            long posistion = this.Position;
            long index = this.GetKeyIndex(key);

            if (index > posistion)
            {
                long len = index - posistion;
                arr = new byte[len];
                this.stream.Seek(posistion, SeekOrigin.Begin);
                this.stream.Read(arr, 0, arr.Length);
                this.stream.Seek(key.Length, SeekOrigin.Begin);
            }

            return arr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long GetKeyIndex(byte[] key)
        {
           return this.GetKeyIndex(key, -1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public long GetKeyIndex(byte[] key, long maxCount)
        {
            long index = -1;
            if (key != null && key.Length > 0 && this.stream != null
                && this.Length - this.Position >= key.Length)
            {
                long _position = this.Position;
                
                if (maxCount <= 0 || maxCount + this.Position > this.Length)
                    maxCount = this.stream.Length - key.Length;
                else
                    maxCount = maxCount + this.Position - key.Length;
                
                long i = this.Position;
                bool flag = false;
                while (i < maxCount && !flag)
                {
                    this.stream.Seek(i, SeekOrigin.Begin);
                    flag = true;
                    for (int j = 0; j < key.Length; j++)
                    {
                        byte val = (byte)this.stream.ReadByte();
                        if (key[j] != val)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        index = i;
                        break;
                    }
                    i++;
                }

                this.stream.Seek(_position, SeekOrigin.Begin);
            }

            return index;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ReadToEnd()
        {
            byte[] buffer = null;
            if (this.stream != null && this.Position < this.Length)
            {
                long len = this.Length - this.Position;
                buffer = new byte[len];
                this.stream.Read(buffer, 0, buffer.Length);
            }

            return buffer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.disposingStream && this.stream != null)
                {
                    try { this.stream.Dispose(); }
                    catch { }
                }

                this.stream = null;
            }
        }
    }
}

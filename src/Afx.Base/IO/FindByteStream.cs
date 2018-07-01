using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Afx.IO
{
    /// <summary>
    /// 查找byte流
    /// </summary>
    public sealed class FindByteStream : Stream
    {
        private byte[] buffer;
        private long position;

        /// <summary>
        /// 初始化流
        /// </summary>
        /// <param name="buffer"></param>
        public FindByteStream(byte[] buffer)
        {
            this.buffer = buffer;
            this.position = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool CanRead
        {
            get { return this.buffer != null && this.buffer.LongLength > this.position; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool CanSeek
        {
            get { return this.buffer != null && this.buffer.LongLength > this.position; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool CanWrite
        {
            get { return false; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Flush()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        public override long Length
        {
            get { return this.buffer == null ? 0 : this.buffer.LongLength; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get
            {
                return this.position;
            }
            set
            {
                if (value < 0)
                {
                    this.position = 0;
                }
                else if (value > this.Length)
                {
                    this.position = this.Length;
                }
                else
                {
                    this.position = value;
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
            if (count > 0 && count + offset <= buffer.Length
                && this.position < this.Length)
            {
                while (this.position < this.buffer.Length && len < count)
                {
                    buffer[len++ + offset] = this.buffer[this.position++];
                }
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
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.Position = offset;
                    break;
                case SeekOrigin.End:
                    this.Position = this.Length - offset;
                    break;
                case SeekOrigin.Current:
                    this.Position = this.Position + offset;
                    break;
            }

            return this.position;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
           throw new NotSupportedException("流不支持写入!");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("流不支持写入!");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            return this.position < this.Length ? this.buffer[this.position++] : -1;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Close()
        {
            base.Close();
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
            long index = this.GetKeyIndex(key);

            if (index > 0)
            {
                long len = index - this.position;
                arr = new byte[len];
                if (len > 0)
                {
                    for (long i = 0; i < arr.LongLength; i++)
                    {
                        arr[i] = this.buffer[this.position + i];
                    }
                }

                this.position = index + key.Length;
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
            if (key != null && key.Length > 0 && this.Length - this.position >= key.Length)
            {
                long i = this.position;
                bool flag = false;

                if (maxCount <= 0 || maxCount + this.position > this.buffer.Length)
                    maxCount = this.buffer.Length - key.Length;
                else
                    maxCount = maxCount + this.position - key.Length;

                while (i < maxCount && !flag)
                {
                    flag = true;
                    for (int j = 0; j < key.Length; j++)
                    {
                        if (key[j] != this.buffer[i + j])
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
            }

            return index;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ReadToEnd()
        {
            long len = this.Length - this.position;
            byte[] buffer = new byte[len];
            for (long i = 0; i < len; i++)
            {
                buffer[i] = this.buffer[this.position++];
            }

            return buffer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.buffer = null;
                this.position = 0;
            }
        }
    }
}

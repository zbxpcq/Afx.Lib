using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Afx.Data.Extensions
{
    /// <summary>
    /// IDataReader 读取结果转换成实体
    /// </summary>
    public abstract class ReaderToModel : IReaderToModel
    {
        /// <summary>
        /// 转换实体
        /// </summary>
        /// <param name="reader">IDataReader</param>
        /// <returns></returns>
        public abstract object To(IDataReader reader);

        Dictionary<string, int> dic = new Dictionary<string, int>();
        /// <summary>
        /// 获取列所在位置
        /// </summary>
        /// <param name="reader">IDataReader</param>
        /// <param name="name"></param>
        /// <returns>未找到返回-1</returns>
        public int GetOrdinal(IDataReader reader, string name)
        {
            int i = -1;
            if (!dic.TryGetValue(name, out i))
            {
                i = -1;
                try { i = reader.GetOrdinal(name); }
                catch { }
                dic[name] = i;
            }
            return i;
        }

        /// <summary>
        /// 读取byte[]
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public byte[] GetBytes(IDataReader reader, int ordinal)
        {
            byte[] buffer = null;
            List<byte[]> list = new List<byte[]>();
            long length = 0;
            long pos = 1;
            while(pos > 0)
            {
                byte[] buf = new byte[4 * 1024];
                pos = reader.GetBytes(ordinal, length, buf, 0, buf.Length);
                if (pos > 0)
                {
                    length += pos;
                    list.Add(buf);
                }
            }

            pos = 0;
            buffer = new byte[length];
            foreach(var buf in list)
            {
                long n = length - pos;
                if (n > buf.Length) n = buf.Length;
                Array.Copy(buf, 0, buffer, pos, n);
                pos += n;
            }
            
            return buffer;
        }
        /// <summary>
        /// 读取char[]
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public char[] GetChars(IDataReader reader, int ordinal)
        {
            char[] buffer = null;
            List<char[]> list = new List<char[]>();
            long length = 0;
            long pos = 1;
            while (pos > 0)
            {
                char[] buf = new char[4 * 1024];
                pos = reader.GetChars(ordinal, length, buf, 0, buf.Length);
                if (pos > 0)
                {
                    length += pos;
                    list.Add(buf);
                }
            }

            pos = 0;
            buffer = new char[length];
            foreach (var buf in list)
            {
                long n = length - pos;
                if (n > buf.Length) n = buf.Length;
                Array.Copy(buf, 0, buffer, pos, n);
                pos += n;
            }

            return buffer;
        }
    }
}

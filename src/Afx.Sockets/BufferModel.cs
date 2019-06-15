using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Sockets.Models
{
    /// <summary>
    /// Receive BufferModel
    /// </summary>
    public class BufferModel
    {
        /// <summary>
        /// 缓存大小
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// 成功读取位置
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// 缓存
        /// </summary>
        public byte[] Data { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="size">缓存大小</param>
        public BufferModel(int size = 8 * 1024)
        {
            if (size <= 0) throw new ArgumentException("size is error!");
            this.Position = 0;
            this.Size = size < 16 ? 16 : (size > 8 * 1024 ? 8 * 1024 : size);
            this.Data = new byte[this.Size];
        }

        /// <summary>
        /// 清除读取数据
        /// </summary>
        public void Clear()
        {
            this.Position = 0;
        }
    }
}

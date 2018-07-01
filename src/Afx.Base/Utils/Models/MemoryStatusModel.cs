using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// 内存信息
    /// </summary>
    public class MemoryStatusModel
    {
        /// <summary>
        /// 已用内存的百分比
        /// </summary>
        public uint MemoryLoad { get; set; }
        /// <summary>
        /// 物理内存总量
        /// </summary>
        public ulong TotalPhys { get; set; }
        /// <summary>
        /// 可用物理内存
        /// </summary>
        public ulong AvailPhys { get; set; }
        /// <summary>
        /// 交换文件总的大小
        /// </summary>
        public ulong TotalPageFile { get; set; }
        /// <summary>
        /// 交换文件中空闲部分大小
        /// </summary>
        public ulong AvailPageFile { get; set; }
        /// <summary>
        /// 用户可用的地址空间
        /// </summary>
        public ulong TotalVirtual { get; set; }
        /// <summary>
        /// 当前空闲的地址空间
        /// </summary>
        public ulong AvailVirtual { get; set; }
    }
}

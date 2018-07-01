using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Afx.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    struct MEMORYSTATUSEX
    {
        /// <summary>
        /// 本结构的长度
        /// </summary>
        public int dwLength;
        /// <summary>
        /// 已用内存的百分比
        /// </summary>
        public uint dwMemoryLoad;
        /// <summary>
        /// 物理内存总量
        /// </summary>
        public ulong ullTotalPhys;
        /// <summary>
        /// 可用物理内存
        /// </summary>
        public ulong ullAvailPhys;
        /// <summary>
        /// 交换文件总的大小
        /// </summary>
        public ulong ullTotalPageFile;
        /// <summary>
        /// 交换文件中空闲部分大小
        /// </summary>
        public ulong ullAvailPageFile;
        /// <summary>
        /// 用户可用的地址空间
        /// </summary>
        public ulong ullTotalVirtual;
        /// <summary>
        /// 当前空闲的地址空间
        /// </summary>
        public ulong ullAvailVirtual;
        /// <summary>
        /// 
        /// </summary>
        public ulong ullAvailExtendedVirtual;
    }
}

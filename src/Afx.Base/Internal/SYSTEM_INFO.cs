using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Afx.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    struct SYSTEM_INFO
    {
        /// <summary>
        /// 处理器架构
        /// </summary>
        public ushort wProcessorArchitecture;
        /// <summary>
        /// 
        /// </summary>
        public ushort wReserved;
        /// <summary>
        /// 页面大小
        /// </summary>
        public uint dwPageSize;
        /// <summary>
        /// 应用程序最小地址
        /// </summary>
        public IntPtr lpMinimumApplicationAddress;
        /// <summary>
        /// 应用程序最大地址
        /// </summary>
        public IntPtr lpMaximumApplicationAddress;
        /// <summary>
        /// 处理器掩码 
        /// </summary>
        public IntPtr dwActiveProcessorMask;
        /// <summary>
        /// 处理器数量
        /// </summary>
        public uint dwNumberOfProcessors;
        /// <summary>
        /// 处理器类型 
        /// </summary>
        public uint dwProcessorType;
        /// <summary>
        /// 虚拟内存分配粒度 
        /// </summary>
        public uint dwAllocationGranularity;
        /// <summary>
        /// 处理器级别 
        /// </summary>
        public ushort wProcessorLevel;
        /// <summary>
        /// 处理器版本
        /// </summary>
        public ushort wProcessorRevision;
    };
}

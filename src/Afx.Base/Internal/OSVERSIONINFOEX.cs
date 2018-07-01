using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Afx.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    struct OSVERSIONINFOEX
    {
        /// <summary>
        /// 在使用GetVersionEx之前要将此初始化为结构的大小
        /// </summary>
        public int dwOSVersionInfoSize;
        /// <summary>
        /// 系统主版本号
        /// </summary>
        public uint dwMajorVersion;
        /// <summary>
        /// 系统次版本号
        /// </summary>
        public uint dwMinorVersion;
        /// <summary>
        /// 系统构建号
        /// </summary>
        public uint dwBuildNumber;
        /// <summary>
        /// 系统支持的平台
        /// </summary>
        public uint dwPlatformId;
        /// <summary>
        /// 系统补丁包的名称
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
        /// <summary>
        /// 系统补丁包的主版本
        /// </summary>
        public ushort wServicePackMajor;
        /// <summary>
        /// 系统补丁包的次版本
        /// </summary>
        public ushort wServicePackMinor;
        /// <summary>
        /// 标识系统上的程序组
        /// </summary>
        public ushort wSuiteMask;
        /// <summary>
        /// 标识系统类型
        /// </summary>
        public byte wProductType;
        /// <summary>
        /// 保留,未使用
        /// </summary>
        public byte wReserved;
    };
}

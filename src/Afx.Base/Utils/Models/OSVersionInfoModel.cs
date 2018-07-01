using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// 系统详细信息
    /// </summary>
    public class OSVersionInfoModel
    {
        /// <summary>
        /// 系统主版本号
        /// </summary>
        public uint MajorVersion { get; set; }
        /// <summary>
        /// 系统次版本号
        /// </summary>
        public uint MinorVersion { get; set; }
        /// <summary>
        /// 系统构建号
        /// </summary>
        public uint BuildNumber { get; set; }
        /// <summary>
        /// 系统支持的平台
        /// </summary>
        public uint PlatformId { get; set; }
        /// <summary>
        /// 系统补丁包的名称
        /// </summary>
        public string CSDVersion { get; set; }
        /// <summary>
        /// 系统补丁包的主版本
        /// </summary>
        public ushort ServicePackMajor { get; set; }
        /// <summary>
        /// 系统补丁包的次版本
        /// </summary>
        public ushort ServicePackMinor { get; set; }
        /// <summary>
        /// 标识系统上的程序组
        /// </summary>
        public ushort SuiteMask { get; set; }
        /// <summary>
        /// 标识系统类型
        /// </summary>
        public byte ProductType { get; set; }
        /// <summary>
        /// 保留,未使用
        /// </summary>
        public byte Reserved { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }
}

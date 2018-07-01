using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// 磁盘序列号model
    /// </summary>
    public class VolumeInfoModel
    {
        /// <summary>
        /// 序列号
        /// </summary>
        public uint SerialNumber { get; set; }
        /// <summary>
        /// FileSystemFlags
        /// </summary>
        public uint FileSystemFlags { get; set; }
        /// <summary>
        /// FileSystemName
        /// </summary>
        public string FileSystemName { get; set; }
    }
}

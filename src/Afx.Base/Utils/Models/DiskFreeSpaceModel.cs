using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// 磁盘信息
    /// </summary>
    public class DiskFreeSpaceModel
    {
        /// <summary>
        /// 盘符
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 可用大小
        /// </summary>
        public ulong AvailableFreeSpace { get; set; }
        /// <summary>
        /// 未使用大小
        /// </summary>
        public ulong TotalFreeSpace { get; set; }
        /// <summary>
        /// 总大小
        /// </summary>
        public ulong TotalSize { get; set; }
    }
}

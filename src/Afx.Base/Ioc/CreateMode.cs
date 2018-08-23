using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Ioc
{
    /// <summary>
    /// TService 实例化方式
    /// </summary>
    public enum CreateMode
    {
        /// <summary>
        /// 默认创建
        /// </summary>
        None = 0,
        /// <summary>
        /// 注册为单列
        /// </summary>
        Instance = 1,
        /// <summary>
        /// 通过注册方法创建
        /// </summary>
        Method = 2
    }
}

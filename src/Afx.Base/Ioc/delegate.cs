using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Ioc
{
    /// <summary>
    /// 创建 TService Context
    /// </summary>
    public class OnGetContext : RegisterContext
    {
        /// <summary>
        /// 构造函数参数
        /// </summary>
        public object[] Arguments { get; set; }
        /// <summary>
        /// TService 实例
        /// </summary>
        public object Target { get; set; }
    }

    /// <summary>
    /// 创建 TService Context Callback
    /// </summary>
    /// <param name="context">OnGetContext</param>
    public delegate void OnGetCallback(OnGetContext context);
}

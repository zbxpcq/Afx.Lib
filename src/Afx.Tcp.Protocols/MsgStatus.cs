using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Afx.Tcp.Protocols
{
    /// <summary>
    /// MsgStatus
    /// </summary>
    public enum MsgStatus
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// 成功
        /// </summary>
        Succeed = 1,
        /// <summary>
        /// 失败
        /// </summary>
        Error = 2,
        /// <summary>
        /// 服务器错误
        /// </summary>
        ServerError = 3,
        /// <summary>
        /// 未登录
        /// </summary>
        NeedLogin = 4,
        /// <summary>
        /// 需要权限
        /// </summary>
        NeedAuth = 5,
        /// <summary>
        /// 未知请求
        /// </summary>
        Unknown = 6
    }
}

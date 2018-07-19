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
        Succeed = 100,
        /// <summary>
        /// 失败！
        /// </summary>
        Failure = 200,
        /// <summary>
        /// 参数错误
        /// </summary>
        Error = 201,
        /// <summary>
        /// 服务器错误
        /// </summary>
        ServerError = 203,
        /// <summary>
        /// 未登录
        /// </summary>
        NeedLogin = 300,
        /// <summary>
        /// 需要权限
        /// </summary>
        NeedAuth = 301,
        /// <summary>
        /// 未知请求
        /// </summary>
        Unknown = 10000
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using Afx.Tcp.Protocols;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// 异常上下文
    /// </summary>
    public class ExceptionContext
    {
        /// <summary>
        /// 接收到的消息
        /// </summary>
        public MsgData Msg { get; set; }

        /// <summary>
        /// IsHandle = true时返回的ActionResult
        /// </summary>
        public ActionResult Result { get; set; }
        /// <summary>
        /// 是否处理异常
        /// </summary>
        public bool IsHandle { get; set; }
        
        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; set; }
    }
}

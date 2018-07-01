using System;
using System.Collections.Generic;
using System.Text;

using Afx.Tcp.Protocols;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// 权限上下文
    /// </summary>
    public class AuthorizationContext : IDisposable
    {
        /// <summary>
        /// 是否授权
        /// </summary>
        public bool IsAuth { get; set; }

        /// <summary>
        /// 当前执行的cmd
        /// </summary>
        public int Cmd { get; set; }

        /// <summary>
        /// Session
        /// </summary>
        public Session Session { get; internal set; }

        /// <summary>
        /// IsAuth = false 时返回的 ActionResult.
        /// </summary>
        public ActionResult Result { get; set; }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.Session = null;
            this.Result = null;
        }
    }
}

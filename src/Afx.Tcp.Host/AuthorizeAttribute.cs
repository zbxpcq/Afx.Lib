using System;
using System.Collections.Generic;
using System.Text;

using Afx.Tcp.Protocols;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// 授权控制Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class AuthorizeAttribute : Attribute, IAuthorize
    {
        /// <summary>
        /// 请求权限
        /// </summary>
        /// <param name="authContext"></param>
        public abstract void OnAuthorization(AuthorizationContext authContext);
    }
}

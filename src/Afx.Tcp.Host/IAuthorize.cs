using System;
using System.Collections.Generic;
using System.Text;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// 鉴权接口
    /// </summary>
    public interface IAuthorize
    {
        /// <summary>
        /// 发生鉴权
        /// </summary>
        /// <param name="authContext"></param>
        void OnAuthorization(AuthorizationContext authContext);
    }
}

using System;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// 允许匿名访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=false)]
    public class AllowAnonymousAttribute : Attribute
    {
    }
}

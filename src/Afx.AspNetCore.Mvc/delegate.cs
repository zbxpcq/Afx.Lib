using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace Afx.AspNetCore.Mvc
{
    /// <summary>
    /// 请求回调
    /// </summary>
    /// <param name="context"></param>
    public delegate void BeginRequestCallback(HttpContext context, string sid);
    /// <summary>
    /// 请求结束回调
    /// </summary>
    /// <param name="context"></param>
    public delegate string EndRequestCallback(HttpContext context, string sid);
}

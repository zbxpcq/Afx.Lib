using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace Afx.AspNetCore.Mvc
{
    /// <summary>
    /// sid请求回调， 没有
    /// </summary>
    /// <param name="sid">请求的sid</param>
    public delegate void RequestSidCallback(string sid);
    /// <summary>
    /// sid请求结束回调
    /// </summary>
    /// <param name="sid">请求的sid</param>
    /// <returns>写入client的sid</returns>
    public delegate string ResponseSidCallback(string sid);
    /// <summary>
    /// 请求回调
    /// </summary>
    /// <param name="context"></param>
    public delegate void BeginRequestCallback(HttpContext context);
    /// <summary>
    /// 请求结束回调
    /// </summary>
    /// <param name="context"></param>
    public delegate void EndRequestCallback(HttpContext context);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Afx.Web.Http
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sid"></param>
    public delegate void RequestSidCallback(string sid);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sid"></param>
    /// <returns></returns>
    public delegate string ResponseSidCallback(string sid);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    public delegate void BeginRequestCallback(HttpRequestMessage request);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
    public delegate void EndRequestCallback(HttpRequestMessage request, HttpResponseMessage response);
}

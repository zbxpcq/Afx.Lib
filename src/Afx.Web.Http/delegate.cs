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
    /// <param name="request"></param>
    public delegate void BeginRequestCallback(HttpRequestMessage request, string sid);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
    public delegate string EndRequestCallback(HttpRequestMessage request, HttpResponseMessage response, string sid);
}

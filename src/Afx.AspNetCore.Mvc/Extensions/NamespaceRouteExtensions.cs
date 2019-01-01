using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Afx.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 添加命名空间作为路由
    /// </summary>
    public static class NamespaceRouteExtensions
    {
        /// <summary>
        /// 添加命名空间作为路由
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="namespaces">需要作为路由的命名空间</param>
        public static void UseNamespaceRoute(this MvcOptions opts, params string[] namespaces)
        {
            opts.Conventions.Insert(0, new NamespaceRouteModelConvention(namespaces));
        }
    }
        
}



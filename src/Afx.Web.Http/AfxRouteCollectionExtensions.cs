using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Afx.Web.Http;

namespace System.Web.Http
{
    /// <summary>
    /// AfxRouteCollectionExtensions
    /// </summary>
    public static class AfxRouteCollectionExtensions
    {
        private static IHttpControllerSelector controllerSelector = null;
        /// <summary>
        /// MapRoute
        /// </summary>
        /// <param name="config"></param>
        /// <param name="name"></param>
        /// <param name="routeTemplate"></param>
        /// <param name="defaults"></param>
        /// <param name="constraints"></param>
        /// <param name="handler"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public static IHttpRoute MapRoute(this HttpConfiguration config, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler, string[] namespaces)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            HttpRouteValueDictionary defaults2 = new HttpRouteValueDictionary(defaults);
            HttpRouteValueDictionary constraints2 = new HttpRouteValueDictionary(constraints);
            Dictionary<string, object> dataTokens = new Dictionary<string, object>();
            if (namespaces != null && namespaces.Length > 0) dataTokens.Add("Namespace", namespaces);
            IHttpRoute httpRoute = config.Routes.CreateRoute(routeTemplate, defaults2, constraints2, dataTokens, handler);
            config.Routes.Add(name, httpRoute);

            if(controllerSelector == null)
            {
                controllerSelector = new NamespaceControllerSelector(config);
                config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            }

            return httpRoute;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="name"></param>
        /// <param name="routeTemplate"></param>
        /// <param name="defaults"></param>
        /// <param name="constraints"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public static IHttpRoute MapRoute(this HttpConfiguration config, string name, string routeTemplate, object defaults, object constraints, string[] namespaces)
        {
            return AfxRouteCollectionExtensions.MapRoute(config, name, routeTemplate, defaults,
                constraints, null, namespaces);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="name"></param>
        /// <param name="routeTemplate"></param>
        /// <param name="defaults"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public static IHttpRoute MapRoute(this HttpConfiguration config, string name, string routeTemplate, object defaults, string[] namespaces)
        {
            return AfxRouteCollectionExtensions.MapRoute(config, name, routeTemplate, defaults,
                null, null, namespaces);
        }
    }
}

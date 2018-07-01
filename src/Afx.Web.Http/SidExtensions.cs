using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    public static class AfxRouteCollectionExtensions
    {
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler, string[] namespaces)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            HttpRouteValueDictionary defaults2 = new HttpRouteValueDictionary(defaults);
            HttpRouteValueDictionary constraints2 = new HttpRouteValueDictionary(constraints);
            Dictionary<string, object> dataTokens = new Dictionary<string, object>();
            if (namespaces != null && namespaces.Length > 0) dataTokens.Add("Namespace", namespaces);
            IHttpRoute httpRoute = routes.CreateRoute(routeTemplate, defaults2, constraints2, dataTokens, handler);
            routes.Add(name, httpRoute);
            return httpRoute;
        }

        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, object constraints, string[] namespaces)
        {
            return AfxRouteCollectionExtensions.MapHttpRoute(routes, name, routeTemplate, defaults,
                constraints, null, namespaces);
        }

        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, string[] namespaces)
        {
            return AfxRouteCollectionExtensions.MapHttpRoute(routes, name, routeTemplate, defaults,
                null, null, namespaces);
        }
    }
}

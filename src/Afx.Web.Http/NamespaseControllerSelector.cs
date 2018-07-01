using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace Afx.Web.Http
{
    /// <summary>
    /// 
    /// </summary>
    public class NamespaceControllerSelector : IHttpControllerSelector
    {
        /// <summary>
        /// 
        /// </summary>
        public const string ControllerSuffix = "Controller";

        internal const string ControllerNamespaceSuffix = "Controllers";

        private readonly HttpConfiguration configuration;

        private readonly AfxControllerTypeCache controllerTypeCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public NamespaceControllerSelector(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentException("configuration is null!", "configuration");
            }
            this.configuration = configuration;
            this.controllerTypeCache = new AfxControllerTypeCache(this.configuration);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentException("request is null!", "request");
            }
            HttpControllerDescriptor descriptor = null;
            
            IHttpRouteData routeData = request.GetRouteData();
            string controllerName = this.GetControllerName(routeData);
            string[] namespaces = this.GetNamespaces(routeData);
            if (!string.IsNullOrEmpty(controllerName))
            {
                Dictionary<string, HttpControllerDescriptor> dic = controllerTypeCache.GetControllerTypes(controllerName);
                if (dic != null)
                {
                    if (namespaces != null && namespaces.Length > 0)
                    {
                        foreach(var n in namespaces)
                        {
                            if (!string.IsNullOrEmpty(n) && dic.TryGetValue(n, out descriptor))
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        string endNamespaceSuffix = "." + ControllerNamespaceSuffix;
                        foreach (KeyValuePair<string, HttpControllerDescriptor> nk in dic)
                        {
                            if (string.Compare(nk.Key, ControllerNamespaceSuffix, true) == 0
                                || nk.Key.EndsWith(endNamespaceSuffix, StringComparison.OrdinalIgnoreCase))
                            {
                                descriptor = nk.Value;
                                break;
                            }
                        }
                    }

                    if (descriptor == null)
                    {
                        if (!dic.TryGetValue(string.Empty, out descriptor))
                        {
                            descriptor = dic.Values.First();
                        }
                    }

                    return descriptor;
                }
            }

            throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.NotFound, "not found " + request.RequestUri + "!"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            Dictionary<string, HttpControllerDescriptor> dic = new Dictionary<string, HttpControllerDescriptor>(this.controllerTypeCache.Cache.Count);

            string endNamespaceSuffix = "." + ControllerNamespaceSuffix;
            foreach (KeyValuePair<string, Dictionary<string, HttpControllerDescriptor>> kv in this.controllerTypeCache.Cache)
            {
                HttpControllerDescriptor descriptor = null;
                foreach(KeyValuePair<string, HttpControllerDescriptor> nk in kv.Value)
                {
                    if (string.Compare(nk.Key, ControllerNamespaceSuffix, true) == 0
                        || nk.Key.EndsWith(endNamespaceSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        descriptor = nk.Value;
                        break;
                    }
                }
                if (descriptor == null)
                {
                    if(!kv.Value.TryGetValue(string.Empty, out descriptor))
                    {
                        descriptor = kv.Value.Values.First();
                    }
                }

                dic.Add(kv.Key, descriptor);
            }

            return dic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public virtual string GetControllerName(IHttpRouteData routeData)
        {
            if (routeData == null)
            {
                return null;
            }
            string result = null;
            object obj = null;
            if(!routeData.Values.TryGetValue("controller", out obj))
            {
                routeData.Values.TryGetValue("Controller", out obj);
            }
            result = obj as string;
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public virtual string[] GetNamespaces(IHttpRouteData routeData)
        {
            if (routeData == null || routeData.Route.DataTokens == null)
            {
                return null;
            }
            string[] result = null;
            object obj = null;
            if(!routeData.Route.DataTokens.TryGetValue("Namespace", out obj))
            {
                routeData.Route.DataTokens.TryGetValue("namespace", out obj);
            }
            if(obj is string)
            {
                result = new string[] { obj as string };
            }
            else if (obj is string[])
            {
                result = obj as string[];
            }

            return result;
        }
        
    }

}

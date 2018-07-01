using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Afx.Web.Http
{
    internal class AfxControllerTypeCache
    {
        private readonly HttpConfiguration _configuration;

        private readonly Lazy<Dictionary<string, Dictionary<string, HttpControllerDescriptor>>> _cache;

        internal Dictionary<string, Dictionary<string, HttpControllerDescriptor>> Cache
		{
			get
			{
				return this._cache.Value;
			}
		}

        public AfxControllerTypeCache(HttpConfiguration configuration)
		{
			if (configuration == null)
			{
                throw new ArgumentException("configuration is null!", "configuration");
			}
			this._configuration = configuration;
            this._cache = new Lazy<Dictionary<string, Dictionary<string, HttpControllerDescriptor>>>(new Func<Dictionary<string, Dictionary<string, HttpControllerDescriptor>>>(this.InitializeCache));
		}

        public Dictionary<string, HttpControllerDescriptor> GetControllerTypes(string controllerName)
		{
			if (string.IsNullOrEmpty(controllerName))
			{
                throw new ArgumentException("controllerName is null!", "controllerName");
			}

            Dictionary<string, HttpControllerDescriptor> dic = null;
			this._cache.Value.TryGetValue(controllerName, out dic);

            return dic;
		}

        private Dictionary<string, Dictionary<string, HttpControllerDescriptor>> InitializeCache()
		{
			IAssembliesResolver assembliesResolver = this._configuration.Services.GetAssembliesResolver();

			IHttpControllerTypeResolver httpControllerTypeResolver = this._configuration.Services.GetHttpControllerTypeResolver();

			ICollection<Type> controllerTypes = httpControllerTypeResolver.GetControllerTypes(assembliesResolver);

			IEnumerable<IGrouping<string, Type>> source = controllerTypes.GroupBy(t =>
                t.Name.EndsWith(NamespaceControllerSelector.ControllerSuffix, StringComparison.OrdinalIgnoreCase) ?
                t.Name.Substring(0, t.Name.Length - NamespaceControllerSelector.ControllerSuffix.Length)
                : t.Name, StringComparer.OrdinalIgnoreCase);

            return source.ToDictionary(g => g.Key, g => g.ToDictionary(
                t => t.Namespace ?? string.Empty,
                t=> new HttpControllerDescriptor(this._configuration, g.Key, t),
                StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
		}
    }
}

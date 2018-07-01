using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Afx.AspNetCore.Mvc
{
    /// <summary>
    /// 命名空间IApplicationModelConvention
    /// </summary>
    public class NamespaceRouteModelConvention : IApplicationModelConvention
    {
        private readonly List<string> prefixList;
        private const string DEFAULT_PREFIX = "Controllers";
        /// <summary>
        /// NamespaceRouteModelConvention
        /// </summary>
        /// <param name="namespaces"></param>
        public NamespaceRouteModelConvention(string[] namespaces)
        {
            if (namespaces != null && namespaces.Length > 0)
            {
                this.prefixList = new List<string>(namespaces.Length);
                this.prefixList.AddRange(namespaces);
            }
            else
            {
                this.prefixList = new List<string>(0);
            }
        }
        /// <summary>
        /// Apply
        /// </summary>
        /// <param name="application"></param>
        public void Apply(ApplicationModel application)
        {
            Dictionary<string, AttributeRouteModel> dic = new Dictionary<string, AttributeRouteModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var controller in application.Controllers)
            {
                var unmatchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel == null).ToList();
                if (unmatchedSelectors.Any())
                {
                    AttributeRouteModel attributeRouteModel = this.GetAttributeRouteModel(controller.ControllerType.Namespace, ref dic);
                    if (attributeRouteModel != null)
                    {
                        foreach (var selectorModel in unmatchedSelectors)
                        {
                            selectorModel.AttributeRouteModel = attributeRouteModel;
                        }
                    }
                }
            }
        }

        private AttributeRouteModel GetAttributeRouteModel(string _namespace, ref Dictionary<string, AttributeRouteModel> dic)
        {
            AttributeRouteModel attributeRouteModel = null;
            if (!string.IsNullOrEmpty(_namespace))
            {
                if (dic.ContainsKey(_namespace))
                {
                    attributeRouteModel = dic[_namespace];
                }
                else
                {
                    foreach (var prefix in this.prefixList)
                    {
                        if (prefix.Length > 1 && prefix[0] == '*')
                        {
                            var name = prefix.TrimStart('*');
                            var islast = false;
                            if (name.EndsWith('*'))
                            {
                                name = name.TrimEnd('*');
                                islast = true;
                            }
                            if (!string.IsNullOrEmpty(name))
                            {
                                int index = islast ? _namespace.LastIndexOf(name, StringComparison.OrdinalIgnoreCase)
                                    : _namespace.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                                if (index == 0 && _namespace.Length > name.Length
                                    || index > 0 && _namespace.Length - index - name.Length > 0)
                                {
                                    var template = _namespace.Substring(index + name.Length).Trim('.').Replace('.', '/');
                                    if (!string.IsNullOrEmpty(template))
                                    {
                                        attributeRouteModel = new AttributeRouteModel(new RouteAttribute(template + "/[controller]/[action]"));
                                    }
                                }
                            }
                        }
                        else
                        {
                            var name = prefix;
                            if (_namespace.Length > name.Length && _namespace.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                            {
                                string template = _namespace.Substring(name.Length).Trim('.').Replace('.', '/');
                                attributeRouteModel = new AttributeRouteModel(new RouteAttribute(template + "/[controller]/[action]"));
                                break;
                            }
                        }
                    }

                    if (attributeRouteModel == null && (this.prefixList.Count == 0 || this.prefixList.Contains("*")))
                    {
                        attributeRouteModel = this.GetDefault(_namespace);
                    }

                    dic[_namespace] = attributeRouteModel;
                }
            }

            return attributeRouteModel;
        }

        private AttributeRouteModel GetDefault(string _namespace)
        {
            AttributeRouteModel attributeRouteModel = null;
            if (_namespace.Length > DEFAULT_PREFIX.Length)
            {
                var index = _namespace.LastIndexOf(DEFAULT_PREFIX, StringComparison.OrdinalIgnoreCase);
                if (0 == index && _namespace[DEFAULT_PREFIX.Length] == '.'
                    || 0 < index && _namespace[index - 1] == '.' && _namespace.Length - index - DEFAULT_PREFIX.Length - 1 > 0
                    && _namespace[index + DEFAULT_PREFIX.Length] == '.')
                {
                    string template = _namespace.Substring(index + DEFAULT_PREFIX.Length + 1).Replace('.', '/');
                    attributeRouteModel = new AttributeRouteModel(new RouteAttribute(template + "/[controller]/[action]"));
                }
            }

            return attributeRouteModel;
        }
    }
}

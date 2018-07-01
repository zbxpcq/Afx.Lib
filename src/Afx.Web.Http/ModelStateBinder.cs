using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

using System.Net.Http;
using System.Web.Http;
using System.Collections.Concurrent;

namespace Afx.Web.Http
{
    class ModelStateBinder
    {
        private static ConcurrentDictionary<string, ModelInfo> ModelInfoDic = new ConcurrentDictionary<string, ModelInfo>();

        protected BaseApiController m_controller;

        internal ModelStateBinder(BaseApiController controller)
        {
            this.m_controller = controller;
        }
        
        public virtual TModel Get<TModel>()
        {
            TModel model = default(TModel);
            var t = typeof(TModel);
            if(t.IsClass && t.IsPublic && !t.IsAbstract)
            {
                model = Activator.CreateInstance<TModel>();
                ModelInfo info = GetModelInfo(t);
                if (info.PropertyDic != null && info.PropertyDic.Count > 0)
                {
                    bool isUp = UpdateModel(model, info.PropertyDic, null);
                    //if (isUp)
                    //{

                    //}
                }
            }

            return model;
        }

        private bool UpdateModel(object model,
            Dictionary<string, ModelProperty> propertyDic, string prefix)
        {
            bool result = false;
            if (model != null && propertyDic != null && propertyDic.Count > 0)
            {
                if (prefix == null) prefix = "";

                foreach (var kv in propertyDic)
                {
                    var propertyType = kv.Value.PropertyInfo.PropertyType;
                    if (propertyType.IsClass && !propertyType.IsArray && propertyType != typeof(string))
                    {
                        if (propertyType.IsGenericType)
                        {
                           var ts = propertyType.GetGenericArguments();
                           if (ts != null && ts.Length == 1)
                           {
                               if (propertyType == typeof(List<>).MakeGenericType(ts))
                               {
                                   var list = Activator.CreateInstance(propertyType) as IList;
                                   var t = ts[0];
                                   if (t.IsClass && !t.IsArray && t != typeof(string))
                                   {
                                       var info = GetModelInfo(t);
                                       int i = 0;
                                       while (true)
                                       {
                                           var m = Activator.CreateInstance(t);
                                           if (UpdateModel(m, info.PropertyDic, prefix + kv.Key + "[" + i + "]"))
                                           {
                                               list.Add(m);
                                           }
                                           else
                                           {
                                               break;
                                           }
                                           i++;
                                       }
                                   }

                                   try
                                   {
                                       kv.Value.PropertyInfo.SetValue(model, list, null);
                                       result = true;
                                   }
                                   catch { }
                               }
                           }
                        }
                        else
                        {
                            ModelInfo info = GetModelInfo(propertyType);
                            if (info.PropertyDic != null && info.PropertyDic.Count > 0)
                            {
                                object o = Activator.CreateInstance(propertyType);
                                bool isUp = UpdateModel(o, info.PropertyDic, kv.Key + ".");
                                if (isUp)
                                {
                                    try 
                                    { 
                                        kv.Value.PropertyInfo.SetValue(model, o, null);
                                        result = true;
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    else if (propertyType.IsArray)
                    {
                        var ts = propertyType.GetGenericArguments();
                        if (ts != null && ts.Length == 1)
                        {
                            var t = ts[0];
                            var list_t = typeof(List<>).MakeGenericType(ts);
                            var list = Activator.CreateInstance(list_t) as IList;
                            var info = GetModelInfo(t);
                            int i = 0;
                            while (true)
                            {
                                var m = Activator.CreateInstance(t);
                                if (UpdateModel(m, info.PropertyDic, prefix + kv.Key + "[" + i + "]"))
                                {
                                    list.Add(m);
                                }
                                else
                                {
                                    break;
                                }
                                i++;
                            }

                            try
                            {
                                var val = Array.CreateInstance(t, list.Count);
                                list.CopyTo(val, 0);
                                kv.Value.PropertyInfo.SetValue(model, val, null);
                                result = true;
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        string val = this.m_controller.Form[prefix + kv.Key];
                        if (val == null) val = this.m_controller.QueryString[prefix + kv.Key];

                        if (val == null)
                        {
                            continue;
                        }
                        try
                        {
                            object o = ChangeType(val, propertyType);
                            if (o != null)
                            {
                                kv.Value.PropertyInfo.SetValue(model, o, null);
                            }
                            result = true;
                        }
                        catch { }
                    }
                }
            }

            return result;
        }

        public object ChangeType(string value, Type conversionType)
        {
            if (conversionType == typeof(string))
            {
                return value;
            }

            if(string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (conversionType.IsEnum)
            {
                return Enum.Parse(conversionType, value, true);
            }

            if (conversionType.IsGenericType)
            {
                var ts = conversionType.GetGenericArguments();
                if (ts != null && ts.Length == 1 
                    && conversionType == typeof(Nullable<>).MakeGenericType(ts))
                {
                    if(value.ToLower() == "null")
                    {
                        return null;
                    }
                    conversionType = ts[0];
                }
                else
                {
                    return null;
                }
            }

            return Convert.ChangeType(value, conversionType);
        }

        public virtual bool IsValid<TModel>(TModel model)
        {
            string errorMessage;
            return IsValid<TModel>(model, out errorMessage);
        }

        public virtual bool IsValid<TModel>(TModel model, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool result = model != null;
            if (model != null)
            {
                Type t = model.GetType();
                ModelInfo info = GetModelInfo(t);
                if (info.PropertyDic != null && info.PropertyDic.Count > 0)
                {
                    result = Valid(model, info.PropertyDic, out errorMessage);
                }
            }

            return result;
        }

        private bool Valid(object model, Dictionary<string, ModelProperty> propertyDic, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool result = model != null;
            if (model != null && propertyDic != null && propertyDic.Count > 0)
            {
                foreach (var kv in propertyDic)
                {
                    var val = kv.Value.PropertyInfo.GetValue(model, null);
                    if (kv.Value.ValidationList != null && kv.Value.ValidationList.Count > 0)
                    {
                        foreach (var v in kv.Value.ValidationList)
                        {
                            result = v.IsValid(val);
                            if (!result)
                            {
                                errorMessage = v.ErrorMessage;
                                return result;
                            }
                        }
                    }

                    var propertyType = kv.Value.PropertyInfo.PropertyType;
                    if (propertyType.IsClass && !propertyType.IsArray && propertyType != typeof(string))
                    {
                        if (propertyType.IsGenericType)
                        {

                        }
                        else
                        {
                            ModelInfo info = GetModelInfo(propertyType);
                            if (info.PropertyDic != null && info.PropertyDic.Count > 0)
                            {
                                result = Valid(val, info.PropertyDic, out errorMessage);
                            }
                        }
                    }
                    else if (propertyType.IsArray)
                    {

                    }
                }
            }

            return result;
        }

        private ModelInfo GetModelInfo(Type t)
        {
            ModelInfo info = null;
            if (t != null && t.IsClass)
            {
                string key = t.FullName;
                info = ModelInfoDic[key];
                if (info == null)
                {
                    info = CreateModelInfo(t);
                    ModelInfoDic[key]  = info;
                }
            }

            return info;
        }

        private ModelInfo CreateModelInfo(Type t)
        {
            ModelInfo modelInfo = null;
            if (t != null && t.IsClass)
            {
                Dictionary<string, ModelProperty> propertyDic = null;
                var arr = t.GetProperties(BindingFlags.SetProperty| BindingFlags.GetProperty
                    | BindingFlags.Public | BindingFlags.Instance);
                if (arr != null && arr.Length > 0)
                {
                    propertyDic = new Dictionary<string, ModelProperty>();
                    foreach (var p in arr)
                    {
                        List<ValidationAttribute> list = null;
                        var attrs = p.GetCustomAttributes(typeof(ValidationAttribute), true);
                        if (attrs != null && attrs.Length > 0)
                        {
                            list = new List<ValidationAttribute>();
                            foreach (var att in attrs)
                            {
                                var o = att as ValidationAttribute;
                                if (o != null) list.Add(o);
                            }
                        }

                        ModelProperty modelProperty = new ModelProperty();
                        modelProperty.Name = p.Name;
                        modelProperty.PropertyInfo = p;
                        modelProperty.ValidationList = list;

                        propertyDic.Add(p.Name, modelProperty);
                    }
                }

                modelInfo = new ModelInfo();
                modelInfo.Name = t.Name;
                modelInfo.PropertyDic = propertyDic;
            }


            return modelInfo;
        }

    }
}

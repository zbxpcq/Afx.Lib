﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Afx.Utils;

namespace Afx.Ioc
{
#if NET20
    public delegate T Func<T>();
#endif

    public static class IocUtils
    {
        private static IContainer _defaultContainer = new IocContainer(true);

        public static IContainer DefaultContainer => _defaultContainer;

        private static Dictionary<string, string> _iocConfigDic = new Dictionary<string, string>();

        private static Dictionary<string, string> IocConfigDic => _iocConfigDic;

        public static void LoadConfig(string configFile)
        {
            if (string.IsNullOrEmpty(configFile)) throw new ArgumentNullException("configFile");
            string filepath = PathUtils.GetFileFullPath(configFile);
            if (!File.Exists(filepath)) throw new FileNotFoundException(configFile + " not found!");

            XmlDocument doc = new XmlDocument();
            using (var fs = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                doc.Load(fs);
            }

            if (doc.DocumentElement != null)
            {
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node is XmlElement)
                    {
                        var element = node as XmlElement;
                        if (element.Name == "Interface")
                        {
                            var key = element.GetAttribute("name");
                            var value = element.GetAttribute("value");
                            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                            {
                                IocConfigDic[key] = value;
                            }
                        }
                    }
                }
            }
        }

        public static void ClearConfig()
        {
            IocConfigDic.Clear();
        }

        public static TInterface Get<TInterface>() => Get<TInterface>(null, null);

        public static TInterface Get<TInterface>(string name) => Get<TInterface>(name, null);

        public static TInterface Get<TInterface>(object[] args) => Get<TInterface>(null, args);

        public static TInterface Get<TInterface>(string name, object[] args)
        {
            TInterface result = default(TInterface);

            if(string.IsNullOrEmpty(name))
            {
                var key = typeof(TInterface).FullName;
                IocConfigDic.TryGetValue(key, out name);
            }

            result = DefaultContainer.Get<TInterface>(name, args);

            if(result == null)
            {
                throw new ArgumentException($"未找到 { typeof(TInterface).FullName } 实现类（name={name}）!", typeof(TInterface).FullName);
            }

            return result;
        }

        private static Dictionary<Type, object> _singleDic = new Dictionary<Type, object>();
        public static Dictionary<Type, object> DefaultSingleContainer => _singleDic;

        public static void RegisterSingle<TInterface>(object instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            var t = typeof(TInterface);
            var it = instance.GetType();
            if (!t.IsAssignableFrom(it)) throw new ArgumentException("instance is error!");
            _singleDic[t] = instance;
        }
        
        public static void RegisterSingle<TInterface>(Func<TInterface> fun)
        {
            if (fun == null) throw new ArgumentNullException("instance");
            object instance = fun();
            RegisterSingle<TInterface>(instance);
        }

        public static TInterface GetSingle<TInterface>()
        {
            var t = typeof(TInterface);
            TInterface result = default(TInterface);
            object obj = null;
            if (_singleDic.TryGetValue(t, out obj))
            {
                result = (TInterface)obj;
            }
            else
            {
                throw new ArgumentException($"未找到 { t.FullName } 实现类!", typeof(TInterface).FullName);
            }

            return result;
        }
    }
}

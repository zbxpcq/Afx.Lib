using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using Afx.Threading;
using Afx.Utils;

namespace Afx.Ioc
{
    public class Container : IContainer
    {
        private Dictionary<Type, ServiceContext> serviceDic;
        private ReadWriteLock rwLock;
        private Afx.DynamicProxy.ProxyGenerator proxyGenerator;

        public bool IsDisposed { get; private set; }
        public event OnGetCallback GetEvent;

        public Container()
        {
            this.serviceDic = new Dictionary<Type, ServiceContext>();
            this.rwLock = new ReadWriteLock();
            this.proxyGenerator = new DynamicProxy.ProxyGenerator();
            this.proxyGenerator.AopFunc = this.CreateAop;
            this.IsDisposed = false;
        }

        private Afx.DynamicProxy.IAop CreateAop(Type type)
        {
            Afx.DynamicProxy.IAop aop = this.Get(type) as Afx.DynamicProxy.IAop;

            return aop;
        }

        public IRegisterContext Register<TService>(TService instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            var targetType = instance.GetType();
            var serviceType = typeof(TService);
            ServiceContext serviceContext = null;
            ObjectContext objectContext = null;
            using (this.rwLock.GetWriteLock())
            {
                if (!this.serviceDic.TryGetValue(serviceType, out serviceContext))
                {
                    objectContext = serviceContext.Add(instance);
                    this.serviceDic[serviceType] = serviceContext = new ServiceContext(serviceType);
                }
            }
            objectContext = serviceContext.Add(instance);

            return new RegisterContext() { Container = this, ServiceType = serviceType, Context = objectContext };
        }

        public IRegisterContext Register<TService>(Func<IContainer, TService> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            var serviceType = typeof(TService);
            var funcContext = new FuncContext(func.Target, func.Method);
            ServiceContext serviceContext = null;
            ObjectContext objectContext = null;
            using (this.rwLock.GetWriteLock())
            {
                if (!this.serviceDic.TryGetValue(serviceType, out serviceContext))
                {
                    this.serviceDic[serviceType] = serviceContext = new ServiceContext(serviceType, funcContext);
                }
            }
            objectContext = serviceContext.Add(funcContext);

            return new RegisterContext() { Container = this, ServiceType = serviceType, Context = objectContext };
        }

        public IRegisterContext Register(Type serviceType, Type targetType)
        {
            Extensions.Check(serviceType, targetType);
            ServiceContext serviceContext = null;
            ObjectContext objectContext = null;
            using (this.rwLock.GetWriteLock())
            {
                if (!this.serviceDic.TryGetValue(serviceType, out serviceContext))
                {
                    this.serviceDic[serviceType] = serviceContext = new ServiceContext(serviceType, new TargetContext(targetType));
                }
            }
            objectContext = serviceContext.Add(new TargetContext(targetType));

            return new RegisterContext() { Container = this, ServiceType = serviceType, Context = objectContext };
        }

        public IRegisterContext Register<TService, TClass>() where TService : TClass
        {
            return this.Register(typeof(TService), typeof(TClass));
        }

        public List<IRegisterContext> Register<TBaseService>(string assemblyName)
        {
            return this.Register(typeof(TBaseService), assemblyName);
        }

        public List<IRegisterContext> Register<TBaseService>(Assembly assembly)
        {
            return this.Register(typeof(TBaseService), assembly);
        }

        private Dictionary<string, Assembly> assemblyDic = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        private Assembly GetAssembly(string name)
        {
            Assembly assembly = null;
            if (!string.IsNullOrEmpty(name))
            {
                if (assemblyDic.TryGetValue(name, out assembly))
                {
                    return assembly;
                }

                var arr = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var item in arr)
                {
                    var s = item.FullName.Split(',')[0].Trim();
                    if (string.Equals(s, name, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(item.ManifestModule.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        assembly = item;
                        assemblyDic[name] = assembly;
                        return assembly;
                    }
                }

                if (assembly == null)
                {
                    try
                    {
                        if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            assembly = Assembly.Load(name.Substring(0, name.Length - ".dll".Length));
                        }
                        else
                        {
                            assembly = Assembly.Load(name);
                        }
                        if (assembly != null)
                        {
                            assemblyDic[name] = assembly;
                            return assembly;
                        }
                    }
                    catch { }
                }

                string filename = name;
                bool isExists = File.Exists(filename);
                if (!isExists && File.Exists(filename + ".dll"))
                {
                    isExists = true;
                    filename = filename + ".dll";
                }
                else if (!isExists && File.Exists(filename + ".exe"))
                {
                    isExists = true;
                    filename = filename + ".exe";
                }

                if (!isExists)
                {
                    var s = PathUtils.GetFileFullPath(name);
                    if (File.Exists(s))
                    {
                        isExists = true;
                        filename = s;
                    }
                    if (!name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        s = PathUtils.GetFileFullPath(name + ".dll");
                        if (File.Exists(s))
                        {
                            isExists = true;
                            filename = s;
                        }
                    }
                    else if (!name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        s = PathUtils.GetFileFullPath(name + ".exe");
                        if (File.Exists(s))
                        {
                            isExists = true;
                            filename = s;
                        }
                    }
                }

                if (isExists)
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(filename);
                        assemblyDic[name] = assembly;
                    }
                    catch { }
                }
            }

            return assembly;
        }

        public List<IRegisterContext> Register(Type baseServiceType, string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName)) throw new ArgumentNullException("assemblyName");
            var assembly = this.GetAssembly(assemblyName);
            if (assembly == null) throw new FileNotFoundException(assemblyName + " not found!", assemblyName);
            return this.Register(baseServiceType, assembly);
        }

        public List<IRegisterContext> Register(Type baseServiceType, Assembly assembly)
        {
            if (baseServiceType == null) throw new ArgumentNullException("baseServiceType");
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (!baseServiceType.IsClass && !baseServiceType.IsInterface) throw new ArgumentException("baseServiceType is not interface or class!", "baseServiceType");

            var arr = assembly.GetExportedTypes();
            List<IRegisterContext> result = new List<IRegisterContext>();
            foreach (var t in arr)
            {
                if (t.IsClass && !t.IsAbstract && baseServiceType.IsAssignableFrom(t))
                {
                    var ctors = t.GetConstructors();
                    if (ctors != null && ctors.Length > 0)
                    {
                        var interfas = t.GetInterfaces();
                        foreach (var f in interfas)
                        {
                            if (baseServiceType != f && baseServiceType.IsAssignableFrom(f))
                            {
                                result.Add(this.Register(f, t));
                            }
                        }
                    }
                }
            }

            return result;
        }

        private object OnGetEvent(Type serviceType, ObjectContext objectContext, object[] args)
        {
            object result = null;
            if (this.GetEvent != null)
            {
                OnGetContext context = new OnGetContext()
                {
                    Container = this,
                    ServiceType = serviceType,
                    Context = objectContext,
                    Arguments = args
                };

                this.GetEvent(context);

                result = context.Target;
            }

            if((result == null || serviceType.IsInterface) && objectContext.EnableAop)
            {
                if(result == null && objectContext.Mode == CreateMode.None)
                {
                    result = this.proxyGenerator.CreateClassProxy(objectContext.TargetInfo.TargetType, args,
                        true, objectContext.AopTypeList?.ToArray());
                }
                else if(serviceType.IsInterface)
                {
                    object target = result;
                    if (target == null)
                    {
                        target = objectContext.Mode == CreateMode.Instance
                              ? objectContext.Instance : objectContext.Func.Invoke(this);
                    }
                    result = this.proxyGenerator.CreateInterfaceProxy(serviceType, target, true, objectContext.AopTypeList?.ToArray());
                }
            }

            return result;
        }

        private object Create(Type serviceType, object[] args, string name, object key)
        {
            object result = null;
            ServiceContext serviceContext = null;
            bool isGenericType = false;
            using (this.rwLock.GetReadLock())
            {
                if (!this.serviceDic.TryGetValue(serviceType, out serviceContext)
                    && serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition)
                {
                    this.serviceDic.TryGetValue(serviceType.GetGenericTypeDefinition(), out serviceContext);
                    isGenericType = true;
                }
            }

            if (serviceContext != null)
            {
                ObjectContext objectContext = null;
                if (!string.IsNullOrEmpty(name)) objectContext = serviceContext.GetByName(name);
                else if (key != null) objectContext = serviceContext.GetByKey(key);
                else objectContext = serviceContext.Get();

                if (objectContext != null)
                {
                    switch (objectContext.Mode)
                    {
                        case CreateMode.Instance:
                            result = this.OnGetEvent(serviceType, objectContext, args)
                                ?? objectContext.Instance;
                            break;
                        case CreateMode.Method:
                            result = this.OnGetEvent(serviceType, objectContext, args)
                                ?? objectContext.Func.Invoke(this);
                            break;
                        case CreateMode.None:
                            var targetInfo = objectContext.TargetInfo;
                            if (isGenericType)
                            {
                                targetInfo = new TargetContext(targetInfo.TargetType.MakeGenericType(serviceType.GetGenericArguments()));
                                objectContext = new ObjectContext(targetInfo)
                                {
                                    EnableAop = objectContext.EnableAop,
                                    AopTypeList = objectContext.AopTypeList,
                                    Name = objectContext.Name,
                                    Key = objectContext.Key
                                };
                            }
                            var ctors = targetInfo.GetCtors();
                            foreach (var ct in ctors)
                            {
                                if (ct.IsMatch(args))
                                {
                                    result = this.OnGetEvent(serviceType, objectContext, args)
                                        ?? Activator.CreateInstance(targetInfo.TargetType, args);
                                }
                            }

                            if (result == null && (args == null || args.Length == 0))
                            {
                                CtorContext ctor = null;
                                using (this.rwLock.GetReadLock())
                                {
                                    foreach (var ct in ctors)
                                    {
                                        ctor = ct;
                                        foreach (var t in ct.ParameterTypes)
                                        {
                                            if (!this.serviceDic.ContainsKey(t) && t.IsGenericType && !t.IsGenericTypeDefinition)
                                            {
                                                var tt = t.GetGenericTypeDefinition();
                                                if (!this.serviceDic.ContainsKey(tt))
                                                {
                                                    ctor = null;
                                                    break;
                                                }
                                            }
                                        }
                                        if (ctor != null) break;
                                    }
                                }

                                if (ctor != null)
                                {
                                    args = new object[ctor.ParameterTypes.Length];
                                    for (int i = 0; i < args.Length; i++) args[i] = this.Create(ctor.ParameterTypes[i], null, null, null);
                                    result = this.OnGetEvent(serviceType, objectContext, args)
                                        ?? Activator.CreateInstance(targetInfo.TargetType, args);
                                }
                            }
                            break;
                    }
                }
            }

            return result;
        }

        public object Get(Type serviceType)
        {
            return this.Get(serviceType, null);
        }

        public object Get(Type serviceType, object[] args)
        {
            return this.Create(serviceType, args, null, null);
        }

        public object GetByName(Type serviceType, string name)
        {
            return this.GetByName(serviceType, name, null);
        }

        public object GetByName(Type serviceType, string name, object[] args)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            return this.Create(serviceType, args, name, null);
        }

        public object GetByKey(Type serviceType, object key)
        {
            return this.GetByKey(serviceType, key);
        }

        public object GetByKey(Type serviceType, object key, object[] args)
        {
            if (key == null) throw new ArgumentNullException("key");
            return this.Create(serviceType, args, null, key);
        }

        public TService Get<TService>()
        {
            return this.Get<TService>(null);
        }

        public TService Get<TService>(object[] args)
        {
            return (TService)this.Get(typeof(TService), args);
        }

        public TService GetByName<TService>(string name)
        {
            return this.GetByName<TService>(name, null);
        }

        public TService GetByName<TService>(string name, object[] args)
        {
            return (TService)this.GetByName(typeof(TService), name, args);
        }

        public TService GetByKey<TService>(object key)
        {
            return this.GetByKey<TService>(key, null);
        }

        public TService GetByKey<TService>(object key, object[] args)
        {
            return (TService)this.GetByKey(typeof(TService), key, args);
        }

        public void Dispose()
        {
            if (this.IsDisposed) return;

            this.rwLock.Dispose();
            this.rwLock = null;
            this.serviceDic.Clear();
            this.serviceDic = null;
            this.assemblyDic.Clear();
            this.assemblyDic = null;
        }
    }
}

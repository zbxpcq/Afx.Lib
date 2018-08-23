using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Ioc
{
    public interface IContainer : IDisposable
    {
        event OnGetCallback GetEvent;

        IRegisterContext Register<TService>(TService instance);

        IRegisterContext Register<TService>(Func<IContainer, TService> func);

        IRegisterContext Register(Type serviceType, Type targetType);

        IRegisterContext Register<TService, TImplement>() where TService : TImplement;

        List<IRegisterContext> Register<TBaseService>(string assemblyName);

        List<IRegisterContext> Register<TBaseService>(Assembly assembly);

        List<IRegisterContext> Register(Type baseServiceType, string assemblyName);

        List<IRegisterContext> Register(Type baseServiceType, Assembly assembly);
        
        object Get(Type serviceType);

        object Get(Type serviceType, object[] args);

        object GetByName(Type serviceType, string name);

        object GetByName(Type serviceType, string name, object[] args);

        object GetByKey(Type serviceType, object key);

        object GetByKey(Type serviceType, object key, object[] args);

        TService Get<TService>();

        TService Get<TService>(object[] args);

        TService GetByName<TService>(string name);

        TService GetByName<TService>(string name, object[] args);

        TService GetByKey<TService>(object key);

        TService GetByKey<TService>(object key, object[] args);

        bool IsDisposed { get; }
    }
}

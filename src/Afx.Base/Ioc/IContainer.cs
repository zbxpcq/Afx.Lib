using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Ioc
{
    /// <summary>
    /// ioc Container interface
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// 是否 Dispose
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 注册创建 TService Context Callback
        /// </summary>
        /// <param name="createCallback"></param>
        void RegisterCallback(CreateCallback createCallback);

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <typeparam name="TService">TService Type</typeparam>
        /// <param name="instance">实例</param>
        /// <returns>IRegisterContext</returns>
        IRegisterContext Register<TService>(TService instance);

        /// <summary>
        /// 注册TService Create func
        /// </summary>
        /// <typeparam name="TService">TService Type</typeparam>
        /// <param name="func">func</param>
        /// <returns>IRegisterContext</returns>
        IRegisterContext Register<TService>(Func<IContainer, TService> func);

        /// <summary>
        /// 注册TService
        /// </summary>
        /// <param name="serviceType">TService Type</param>
        /// <param name="targetType">target Type</param>
        /// <returns>IRegisterContext</returns>
        IRegisterContext Register(Type serviceType, Type targetType);

        /// <summary>
        /// 注册TService
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns>IRegisterContext</returns>
        IRegisterContext Register<TService>();

        /// <summary>
        /// 注册TService
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplement"></typeparam>
        /// <returns>IRegisterContext</returns>
        IRegisterContext Register<TService, TImplement>() where TImplement : TService;

        /// <summary>
        /// 注册程序集TService
        /// </summary>
        /// <typeparam name="TBaseService">Base TService</typeparam>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns>IRegisterContext list</returns>
        List<IRegisterContext> Register<TBaseService>(string assemblyName);

        /// <summary>
        /// 注册程序集TService
        /// </summary>
        /// <typeparam name="TBaseService">Base TService</typeparam>
        /// <param name="assembly">程序集</param>
        /// <returns>IRegisterContext list</returns>
        List<IRegisterContext> Register<TBaseService>(Assembly assembly);

        /// <summary>
        /// 注册程序集TService
        /// </summary>
        /// <param name="baseServiceType">Base TService Type</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns>IRegisterContext list</returns>
        List<IRegisterContext> Register(Type baseServiceType, string assemblyName);

        /// <summary>
        /// 注册程序集TService
        /// </summary>
        /// <param name="baseServiceType">Base TService Type</param>
        /// <param name="assembly">程序集</param>
        /// <returns>IRegisterContext list</returns>
        List<IRegisterContext> Register(Type baseServiceType, Assembly assembly);

        /// <summary>
        /// 获取TService
        /// </summary>
        /// <param name="serviceType">TService Type</param>
        /// <returns>TService</returns>
        object Get(Type serviceType);

        /// <summary>
        /// 获取TService
        /// </summary>
        /// <param name="serviceType">TService Type</param>
        /// <param name="args">构造函数参数</param>
        /// <returns>TService</returns>
        object Get(Type serviceType, object[] args);

        /// <summary>
        /// 根据命名获取TService
        /// </summary>
        /// <param name="serviceType">TService Type</param>
        /// <param name="name">命名</param>
        /// <returns>TService</returns>
        object GetByName(Type serviceType, string name);

        /// <summary>
        /// 根据命名获取TService
        /// </summary>
        /// <param name="serviceType">TService Type</param>
        /// <param name="name">命名</param>
        /// <param name="args">构造函数参数</param>
        /// <returns>TService</returns>
        object GetByName(Type serviceType, string name, object[] args);

        /// <summary>
        /// 根据Key获取TService
        /// </summary>
        /// <param name="serviceType">TService Type</param>
        /// <param name="key">key</param>
        /// <returns>TService</returns>
        object GetByKey(Type serviceType, object key);

        /// <summary>
        /// 根据Key获取TService
        /// </summary>
        /// <param name="serviceType">TService Type</param>
        /// <param name="key">key</param>
        /// <param name="args">构造函数参数</param>
        /// <returns>TService</returns>
        object GetByKey(Type serviceType, object key, object[] args);

        /// <summary>
        /// 获取TService
        /// </summary>
        /// <typeparam name="TService">TService Type</typeparam>
        /// <returns>TService</returns>
        TService Get<TService>();

        /// <summary>
        /// 获取TService
        /// </summary>
        /// <typeparam name="TService">TService Type</typeparam>
        /// <param name="args">构造函数参数</param>
        /// <returns>TService</returns>
        TService Get<TService>(object[] args);

        /// <summary>
        /// 根据命名获取TService
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="name">命名</param>
        /// <returns>TService</returns>
        TService GetByName<TService>(string name);

        /// <summary>
        /// 根据命名获取TService
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="name">命名</param>
        /// <param name="args">构造函数参数</param>
        /// <returns>TService</returns>
        TService GetByName<TService>(string name, object[] args);

        /// <summary>
        /// 根据Key获取TService
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="key">key</param>
        /// <returns>TService</returns>
        TService GetByKey<TService>(object key);

        /// <summary>
        /// 根据Key获取TService
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="key">key</param>
        /// <param name="args">构造函数参数</param>
        /// <returns>TService</returns>
        TService GetByKey<TService>(object key, object[] args);

        /// <summary>
        /// 获取 TTService 最后注册信息
        /// </summary>
        /// <typeparam name="TTService"></typeparam>
        /// <returns>IRegisterContext</returns>
        IRegisterContext GetRegister<TTService>();

        /// <summary>
        /// 获取 TTService 注册信息
        /// </summary>
        /// <typeparam name="TTService"></typeparam>
        /// <returns></returns>
        List<IRegisterContext> GetRegisterList<TTService>();

        /// <summary>
        /// 获取 TTService 最后注册信息
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        IRegisterContext GetRegister(Type serviceType);

        /// <summary>
        /// 获取 TTService 注册信息
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        List<IRegisterContext> GetRegisterList(Type serviceType);
    }
}

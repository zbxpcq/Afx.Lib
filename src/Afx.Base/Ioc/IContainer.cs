using Afx.Aop;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Ioc
{
    public interface IContainer
    {
        bool IsEnabledAop { get; set; }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="configFile"></param>
        void Load(string configFile);

        /// <summary>
        /// 注册 ioc
        /// </summary>
        /// <param name="interfaceType">接口type</param>
        /// <param name="classType">实现类type</param>
        void Register(Type interfaceType, Type classType);
        /// <summary>
        /// 注册ioc
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TClass">实现类</typeparam>
        void Register<TInterface, TClass>() where TInterface: TClass;

        /// <summary>
        /// 注册程序集所有接口实现
        /// </summary>
        /// <typeparam name="TBaseInterface">接口</typeparam>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        int Register<TBaseInterface>(string assemblyName);

        /// <summary>
        /// 注册程序集所有接口实现
        /// </summary>
        /// <typeparam name="TBaseInterface">接口</typeparam>
        /// <param name="assembly">程序集</param>
        /// <returns></returns>
        int Register<TBaseInterface>(Assembly assembly);

        /// <summary>
        /// 注册程序集所有接口实现
        /// </summary>
        /// <param name="baseInterfaceType">接口 type</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        int Register(Type baseInterfaceType, string assemblyName);
        /// <summary>
        /// 注册程序集所有接口实现
        /// </summary>
        /// <param name="baseInterfaceType">接口 type</param>
        /// <param name="assembly">程序集</param>
        /// <returns></returns>
        int Register(Type baseInterfaceType, Assembly assembly);

        /// <summary>
        /// 根据指定类型获取
        /// </summary>
        /// <typeparam name="TInterface">返回接口类型</typeparam>
        /// <param name="name">类名，不传返回最后注册实现类</param>
        /// <param name="args">构造函数参数</param>
        /// <returns></returns>
        TInterface Get<TInterface>(string name, object[] args);

        /// <summary>
        /// 根据指定类型获取
        /// </summary>
        /// <typeparam name="TInterface">返回接口类型</typeparam>
        /// <param name="name">类名，不传返回最后注册实现类</param>
        /// <returns></returns>
        TInterface Get<TInterface>(string name);

        /// <summary>
        /// 根据指定类型获取
        /// </summary>
        /// <typeparam name="TInterface">返回类型</typeparam>
        /// <returns>返回最后注册实现类</returns>
        TInterface Get<TInterface>();

        /// <summary>
        /// 添加全局IAop实现类型
        /// </summary>
        /// <param name="aopTypeList">IAop实现类型 list</param>
        void AddGlobalAop(List<Type> aopTypeList);

        /// <summary>
        /// 添加全局IAop实现类型
        /// </summary>
        /// <typeparam name="TAop"></typeparam>
        void AddGlobalAop<TAop>() where TAop : class, IAop;

        /// <summary>
        /// 添加全局IAop实现类型
        /// </summary>
        /// <param name="aopType">IAop实现类型</param>
        void AddGlobalAop(Type aopType);

        /// <summary>
        /// 添加指定实现类的IAop
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="aopTypeList"></param>
        void AddAop<TInterface>(List<Type> aopTypeList);

        /// <summary>
        /// 添加指定实现类的IAop
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TAop"></typeparam>
        void AddAop<TInterface, TAop>() where TAop : class, IAop;

        /// <summary>
        /// 添加指定实现类的IAop
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="aopType"></param>
        void AddAop<TInterface>(Type aopType);
    }
}

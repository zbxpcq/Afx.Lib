using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Threading;

using Afx.Threading;

namespace Afx.DynamicProxy
{
    /// <summary>
    /// DynamicProxy Generator
    /// </summary>
    public class ProxyGenerator : IDisposable
    {
        private AssemblyDynamicBuilder assemblyDynamicBuilder;
        private ClassProxy classProxy;
        private InterfaceProxy interfaceProxy;

        /// <summary>
        /// 创建 IAop func
        /// </summary>
        private Func<Type, IAop> AopFunc;
        /// <summary>
        /// 是否 Dispose
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProxyGenerator(Func<Type, IAop> func)
        {
            this.assemblyDynamicBuilder = new AssemblyDynamicBuilder();
            this.classProxy = new ClassProxy(this.assemblyDynamicBuilder, func);
            this.interfaceProxy = new InterfaceProxy(this.assemblyDynamicBuilder, func);
            this.AopFunc = func;

            this.IsDisposed = false;
        }

#if DEBUG && NET452
        public void SaveDynamicModule()
        {
            this.assemblyDynamicBuilder.SaveDynamicModule();
        }
#endif

        /// <summary>
        /// 创建子类代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>代理实例</returns>
        public T GetClassProxy<T>(bool enableAop, Type[] aopType) where T: class
        {
            return this.classProxy.GetProxy<T>(null, enableAop, aopType);
        }

        /// <summary>
        /// 创建子类代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">构造函数参数</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>代理实例</returns>
        public T GetClassProxy<T>(object[] args, bool enableAop, Type[] aopType) where T : class
        {
            return (T)this.classProxy.GetProxy(typeof(T), null, enableAop, aopType);
        }

        /// <summary>
        /// 创建子类代理
        /// </summary>
        /// <param name="targetType">target Type</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>代理实例</returns>
        public object GetClassProxy(Type targetType, bool enableAop, Type[] aopType)
        {
            return this.classProxy.GetProxy(targetType, null, enableAop, aopType);
        }

        /// <summary>
        /// 创建子类代理
        /// </summary>
        /// <param name="targetType">target Type</param>
        /// <param name="args">构造函数参数</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>代理实例</returns>
        public object GetClassProxy(Type targetType, object[] args, bool enableAop, Type[] aopType)
        {
            return this.classProxy.GetProxy(targetType, args, enableAop, aopType); ;
        }
        
        /// <summary>
        /// 创建接口代理
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <param name="target">target</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>TInterface</returns>
        public TInterface GetInterfaceProxy<TInterface>(TInterface target, bool enableAop, Type[] aopType)
        {
            return this.interfaceProxy.GetProxy<TInterface>(target, enableAop, aopType);
        }

        /// <summary>
        /// 创建接口代理
        /// </summary>
        /// <param name="interfaceType">接口 Type</param>
        /// <param name="target">target</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns></returns>
        public object GetInterfaceProxy(Type interfaceType, object target, bool enableAop, Type[] aopType)
        {
            return this.interfaceProxy.GetProxy(interfaceType, target, enableAop, aopType);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;
            this.assemblyDynamicBuilder = null;
            this.AopFunc = null;
            this.classProxy.Dispose();
            this.classProxy = null;
            this.interfaceProxy.Dispose();
            this.interfaceProxy = null;
        }
    }
}

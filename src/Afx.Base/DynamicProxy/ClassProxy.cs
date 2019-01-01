using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Afx.Threading;

namespace Afx.DynamicProxy
{
    /// <summary>
    /// ClassProxy
    /// </summary>
    public class ClassProxy : IDisposable
    {
        private AssemblyDynamicBuilder assemblyDynamicBuilder;
#if NET20
        private Afx.Collections.SafeDictionary<Type, Type> m_classProxyDic;
#else
        private System.Collections.Concurrent.ConcurrentDictionary<Type, Type> m_classProxyDic;
#endif
        private Func<Type, IAop> AopFunc;

        /// <summary>
        /// 是否 Dispose
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// ClassProxy
        /// </summary>
        /// <param name="assemblyDynamicBuilder"></param>
        public ClassProxy(AssemblyDynamicBuilder assemblyDynamicBuilder, Func<Type, IAop> func)
        {
            if (assemblyDynamicBuilder == null) throw new ArgumentNullException("assemblyDynamicBuilder");
            this.assemblyDynamicBuilder = assemblyDynamicBuilder;
#if NET20
            this.m_classProxyDic = new Afx.Collections.SafeDictionary<Type, Type>();
#else
            this.m_classProxyDic = new System.Collections.Concurrent.ConcurrentDictionary<Type, Type>();
#endif
            this.AopFunc = func;
            this.IsDisposed = false;
        }

        private List<MethodInfo> GetAllMethod(Type t)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            List<string> methodList = new List<string>();
            Type ot = typeof(Object);
            while (t != null && t != ot)
            {
                var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var met in methods)
                {
                    if (!met.IsAbstract)
                    {
                        StringBuilder strb = new StringBuilder();
                        if (met.IsStatic) strb.Append("static ");
                        strb.Append(met.ReturnType != null ? met.ReturnType.FullName : "void");
                        strb.Append(" ");
                        strb.Append(met.Name);
                        strb.Append("(");
                        var parameterTypes = ProxyUtil.GetParameterType(met);
                        foreach (var pt in parameterTypes) strb.Append(pt.FullName + ", ");
                        if (parameterTypes.Length > 0) strb.Remove(strb.Length - 2, 2);
                        strb.Append(")");
                        string methodkey = strb.ToString();
                        if (!methodList.Contains(methodkey))
                        {
                            methodList.Add(methodkey);
                            list.Add(met);
                        }
                    }
                }

                t = t.BaseType;
            }

            return list;
        }

        /// <summary>
        /// 创建子类代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>代理实例</returns>
        public T GetProxy<T>(bool enableAop, Type[] aopType) where T : class
        {
            return this.GetProxy<T>(null, enableAop, aopType);
        }

        /// <summary>
        /// 创建子类代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">构造函数参数</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>代理实例</returns>
        public T GetProxy<T>(object[] args, bool enableAop, Type[] aopType) where T : class
        {
            return (T)this.GetProxy(typeof(T), null, enableAop, aopType);
        }

        /// <summary>
        /// 创建子类代理
        /// </summary>
        /// <param name="targetType">target Type</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>代理实例</returns>
        public object GetProxy(Type targetType, bool enableAop, Type[] aopType)
        {
            return this.GetProxy(targetType, null, enableAop, aopType);
        }

        /// <summary>
        /// 创建子类代理
        /// </summary>
        /// <param name="targetType">target Type</param>
        /// <param name="args">构造函数参数</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>代理实例</returns>
        public object GetProxy(Type targetType, object[] args, bool enableAop, Type[] aopType)
        {
            object result = null;
            if (targetType == null) throw new ArgumentNullException("targetType");
            Type proxyType = targetType;
            if (!ProxyUtil.IsProxyType(targetType))
            {
                proxyType = this.GetClassProxyType(targetType);
            }
            else
            {
                targetType = proxyType.BaseType;
            }

            if (args != null && args.Length > 0) result = Activator.CreateInstance(proxyType, args);
            else result = Activator.CreateInstance(proxyType);

            var proxy = result as IProxy;
            proxy.SetTargetType(targetType);

            List<Func<IAop>> funcList = new List<Func<IAop>>();
            if (enableAop && this.AopFunc != null)
            {
                var attrs = targetType.GetCustomAttributes(typeof(AopAttribute), false);
                foreach (var o in attrs)
                {
                    var attr = o as AopAttribute;
                    if (attr.AopType != null && typeof(IAop).IsAssignableFrom(attr.AopType))
                    {
                        funcList.Add(() => this.AopFunc(attr.AopType));
                    }
                }
                if (aopType != null)
                {
                    foreach (var t in aopType)
                    {
                        if (t != null && typeof(IAop).IsAssignableFrom(t))
                        {
                            funcList.Add(() => this.AopFunc(t));
                        }
                    }
                }
            }
            proxy.SetAopFunc(funcList.ToArray());

            return result;
        }

        public bool Register(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (!ProxyUtil.IsProxyType(targetType))
            {
                var proxyType = this.GetClassProxyType(targetType);

                return proxyType != null;
            }

            return true;
        }

        private Type GetClassProxyType(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (!targetType.IsClass) throw new ArgumentException(targetType.FullName + "不是class!", "targetType");
            if (!targetType.IsPublic) throw new ArgumentException(targetType.FullName + "不是 public!", "targetType");
            if (targetType.IsAbstract) throw new ArgumentException(targetType.FullName + "不能是abstract!", "targetType");
            if (targetType.IsGenericType && targetType.IsGenericTypeDefinition) throw new ArgumentException(targetType.FullName + "不能是GenericTypeDefinition!", "targetType");
            if (targetType.IsSealed) throw new ArgumentException(targetType.FullName + "不能是 sealed!", "targetType");
            if (targetType.IsArray) throw new ArgumentException(targetType.FullName + "不能是 array!", "targetType");

            var targetCtors = targetType.GetConstructors();
            if (targetCtors.Length == 0) throw new ArgumentException(targetType.FullName + "没有公共构造函数!", "targetType");

            Type proxyType = null;

            if (this.m_classProxyDic.TryGetValue(targetType, out proxyType)) return proxyType;

            var interfaceTypes = targetType.GetInterfaces();
            var typeDynamicBuilder = this.assemblyDynamicBuilder.DefineType(this.assemblyDynamicBuilder.GetDynamicName(targetType), targetType.Attributes, targetType, interfaceTypes);
            typeDynamicBuilder.AddInterfaceImplementation(typeof(IProxy));
            var targetTypeField = typeDynamicBuilder.DefineField("__m_targetType", typeof(Type), FieldAttributes.Private);
            var aopFuncsField = typeDynamicBuilder.DefineField("__m_aopFuncs", typeof(Func<IAop>[]), FieldAttributes.Private);

            // 构造器
            {
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
                foreach (var baseCtor in targetCtors)
                {
                    var paramets = ProxyUtil.GetParameterType(baseCtor);
                    ILGenerator il = typeDynamicBuilder.DefineConstructor(methattr, calling, paramets);
                    il.Emit(OpCodes.Ldarg_0);
                    for (int i = 0; i < paramets.Length; i++)
                        il.Emit(OpCodes.Ldarg, i + 1);
                    il.Emit(OpCodes.Call, baseCtor);
                    il.Emit(OpCodes.Ret);
                }
            }

            // IProxy
            {
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;

                // SetTargetTypeMethod
                var il = typeDynamicBuilder.DefineMethod(ProxyUtil.SetTargetTypeMethod.Name, methattr, calling, ProxyUtil.SetTargetTypeMethod.ReturnType, new Type[] { typeof(Type) });
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, targetTypeField);
                il.Emit(OpCodes.Ret);

                //GetTargetTypeMethod
                il = typeDynamicBuilder.DefineMethod(ProxyUtil.GetTargetTypeMethod.Name, methattr, calling, ProxyUtil.GetTargetTypeMethod.ReturnType, Type.EmptyTypes);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, targetTypeField);
                il.Emit(OpCodes.Ret);

                //SetAopFuncMethod
                il = typeDynamicBuilder.DefineMethod(ProxyUtil.SetAopFuncMethod.Name, methattr, calling, ProxyUtil.SetAopFuncMethod.ReturnType, new Type[] { typeof(Func<IAop>[]) });
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, aopFuncsField);
                il.Emit(OpCodes.Ret);

                //GetAopFuncMethod
                il = typeDynamicBuilder.DefineMethod(ProxyUtil.GetAopFuncMethod.Name, methattr, calling, ProxyUtil.GetAopFuncMethod.ReturnType, Type.EmptyTypes);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ret);
            }

            //方法
            var methods = this.GetAllMethod(targetType);
            //public hidebysig newslot virtual final
            MethodAttributes newMethodAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            foreach (var baseMethod in methods)
            {
                var intefacemethlist = this.GetOverrideMethods(baseMethod, interfaceTypes);
                if (!baseMethod.IsVirtual && intefacemethlist.Count == 0)
                {
                    continue;
                }

                var parameterInfos = baseMethod.GetParameters();
                var parameterTypes = ProxyUtil.GetType(parameterInfos);
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
                if (baseMethod.Attributes == newMethodAttr || baseMethod.Attributes == (MethodAttributes.Public | MethodAttributes.HideBySig))
                {
                    methattr = newMethodAttr;// MethodAttributes.Public | MethodAttributes.HideBySig;
                }

                ILGenerator il = typeDynamicBuilder.DefineMethod(baseMethod.Name, methattr, calling, baseMethod.ReturnType, parameterInfos);
                #region 定义局部变量
                var contextLocal = il.DeclareLocal(typeof(AopContext));
                var proxyMethodBaseLocal = il.DeclareLocal(typeof(MethodBase));
                var baseMethodLocal = il.DeclareLocal(typeof(MethodInfo));
                var argsLocal = il.DeclareLocal(typeof(object[]));
                var exLocal = il.DeclareLocal(typeof(Exception));
                LocalBuilder resultLocal = null;//5
                if (baseMethod.ReturnType != null && baseMethod.ReturnType != typeof(void))
                {
                    resultLocal = il.DeclareLocal(baseMethod.ReturnType);
                }

                Label execEndLabel = il.DefineLabel();
                Label resultEndLabel = il.DefineLabel();
                Label exEndLabel = il.DefineLabel();
                #endregion

                #region 初始局部变量
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, contextLocal);
                if (resultLocal != null)
                {
                    if (!baseMethod.ReturnType.IsValueType)
                    {
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Stloc, resultLocal);
                    }
                }
                #endregion

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse, execEndLabel);
                il.Emit(OpCodes.Newobj, typeof(AopContext).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc, contextLocal);
                il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Static | BindingFlags.Public));
                il.Emit(OpCodes.Stloc, proxyMethodBaseLocal);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, targetTypeField);
                //il.Emit(OpCodes.Ldloc, methodBaseLocal);
                //il.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name"));
                il.Emit(OpCodes.Ldstr, baseMethod.Name);
                il.Emit(OpCodes.Ldloc, proxyMethodBaseLocal);
                il.Emit(OpCodes.Call, ProxyUtil.GetParameterTypeMethod);
                il.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string), typeof(Type[]) }));
                il.Emit(OpCodes.Stloc, baseMethodLocal);

                il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
                il.Emit(OpCodes.Newarr, typeof(Object));
                il.Emit(OpCodes.Stloc, argsLocal);
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i + 1);
                    if (parameterTypes[i].IsValueType)
                    {
                        il.Emit(OpCodes.Box, parameterTypes[i]);
                    }
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc, baseMethodLocal);
                il.Emit(OpCodes.Ldloc, argsLocal);
                il.Emit(OpCodes.Ldloc, contextLocal);
                il.Emit(OpCodes.Call, ProxyUtil.OnExecutingMethod);
                il.MarkLabel(execEndLabel);
                il.Emit(OpCodes.Nop);
                var exBlock = il.BeginExceptionBlock();
                il.Emit(OpCodes.Ldarg_0);
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg, i + 1);
                }
                il.Emit(OpCodes.Call, baseMethod);
                if (resultLocal != null)
                {
                    il.Emit(OpCodes.Stloc, resultLocal);
                }

                il.BeginCatchBlock(typeof(Exception));
                il.Emit(OpCodes.Stloc, exLocal);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse, exEndLabel);

                il.Emit(OpCodes.Ldloc, contextLocal);
                il.Emit(OpCodes.Ldloc, exLocal);
                il.Emit(OpCodes.Call, ProxyUtil.OnExceptionMethod);

                il.Emit(OpCodes.Nop);
                il.MarkLabel(exEndLabel);
                il.Emit(OpCodes.Ldloc, exLocal);
                il.Emit(OpCodes.Throw);
                il.EndExceptionBlock();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse, resultEndLabel);
                il.Emit(OpCodes.Ldloc, contextLocal);
                if (resultLocal != null)
                {
                    il.Emit(OpCodes.Ldloc, resultLocal);
                    if (baseMethod.ReturnType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, baseMethod.ReturnType);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                }
                il.Emit(OpCodes.Call, ProxyUtil.OnResultMethod);
                il.MarkLabel(resultEndLabel);
                il.Emit(OpCodes.Nop);
                if (resultLocal != null)
                {
                    il.Emit(OpCodes.Ldloc, resultLocal);
                }
                il.Emit(OpCodes.Ret);
            }

            proxyType = typeDynamicBuilder.CreateType();
            this.m_classProxyDic.TryAdd(targetType, proxyType);

            return proxyType;
        }

        private List<MethodInfo> GetOverrideMethods(MethodInfo meth, Type[] interfaceTypes)
        {
            List<MethodInfo> list = new List<MethodInfo>(interfaceTypes != null ? interfaceTypes.Length : 0);
            if (interfaceTypes != null && interfaceTypes.Length > 0)
            {
                foreach (var t in interfaceTypes)
                {
                    var interfaceMeth = t.GetMethod(meth.Name, ProxyUtil.GetParameterType(meth));
                    if (interfaceMeth != null && interfaceMeth.ReturnType == meth.ReturnType)
                        list.Add(interfaceMeth);
                }
            }

            return list;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;
            this.assemblyDynamicBuilder = null;
            this.m_classProxyDic.Clear();
            this.m_classProxyDic = null;
        }
    }
}

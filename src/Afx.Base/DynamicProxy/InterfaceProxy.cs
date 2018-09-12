using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Afx.Threading;

namespace Afx.DynamicProxy
{
    public class InterfaceProxy : IDisposable
    {
        private AssemblyDynamicBuilder assemblyDynamicBuilder;
        private ReadWriteLock m_rwInterfaceLock;
        private Dictionary<Type, Type> m_interfaceProxyDic;
        private Func<Type, IAop> AopFunc;
        /// <summary>
        /// 是否 Dispose
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// InterfaceProxy
        /// </summary>
        /// <param name="assemblyDynamicBuilder"></param>
        public InterfaceProxy(AssemblyDynamicBuilder assemblyDynamicBuilder, Func<Type, IAop> func)
        {
            if (assemblyDynamicBuilder == null) throw new ArgumentNullException("assemblyDynamicBuilder");
            this.assemblyDynamicBuilder = assemblyDynamicBuilder;
            this.m_rwInterfaceLock = new ReadWriteLock();
            this.m_interfaceProxyDic = new Dictionary<Type, Type>();
            this.AopFunc = func;
            this.IsDisposed = false;
        }

        /// <summary>
        /// 创建接口代理
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <param name="target">target</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns>TInterface</returns>
        public TInterface GetProxy<TInterface>(TInterface target, bool enableAop, Type[] aopType)
        {
            return (TInterface)this.GetProxy(typeof(TInterface), target, enableAop, aopType);
        }

        /// <summary>
        /// 创建接口代理
        /// </summary>
        /// <param name="interfaceType">接口 Type</param>
        /// <param name="target">target</param>
        /// <param name="enableAop">enableAop</param>
        /// <param name="aopType">aopType</param>
        /// <returns></returns>
        public object GetProxy(Type interfaceType, object target, bool enableAop, Type[] aopType)
        {
            object result = target;
            if (interfaceType == null) throw new ArgumentNullException("targetType");
            if (target == null) throw new ArgumentNullException("target");
            if (!interfaceType.IsInterface) throw new ArgumentException(interfaceType.FullName + "不是interface!", "interfaceType");
            Type targetType = target.GetType();
            if (!interfaceType.IsAssignableFrom(targetType)) throw new ArgumentException("target 不是" + interfaceType.FullName + "类型", "target");

            if (!ProxyUtil.IsProxyType(targetType))
            {
                var proxyType = this.GetProxyType(interfaceType);
                result = Activator.CreateInstance(proxyType, new object[] { target });
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
            }

            return result;
        }

        private Type GetProxyType(Type interfaceType)
        {
            Type proxyType = null;
            if (interfaceType == null) throw new ArgumentNullException("targetType");
            if (!interfaceType.IsInterface) throw new ArgumentException(interfaceType.FullName + "不是interface!", "interfaceType");
            using (this.m_rwInterfaceLock.GetReadLock())
            {
                this.m_interfaceProxyDic.TryGetValue(interfaceType, out proxyType);
            }
            if (proxyType != null) return proxyType;

            var typeDynamicBuilder = this.assemblyDynamicBuilder.DefineType(this.assemblyDynamicBuilder.GetDynamicName(interfaceType), TypeAttributes.Class | TypeAttributes.Public, typeof(object), new Type[] { interfaceType });
            typeDynamicBuilder.AddInterfaceImplementation(typeof(IProxy));
            var targetTypeField = typeDynamicBuilder.DefineField("__m_targetType", typeof(Type), FieldAttributes.Private);
            var aopFuncsField = typeDynamicBuilder.DefineField("__m_aopFuncs", typeof(Func<IAop>[]), FieldAttributes.Private);
            var targetField = typeDynamicBuilder.DefineField("__m_target", interfaceType, FieldAttributes.Private);

            // 构造器
            {
                var baseCtor = typeof(object).GetConstructor(Type.EmptyTypes);
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
                ILGenerator il = typeDynamicBuilder.DefineConstructor(methattr, calling, new Type[] { interfaceType });
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, baseCtor);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, targetField);
                il.Emit(OpCodes.Ret);
            }

            // IProxy
            {
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;

                // SetTargetTypeMethod
                ILGenerator il = typeDynamicBuilder.DefineMethod(ProxyUtil.SetTargetTypeMethod.Name, methattr, calling, ProxyUtil.SetTargetTypeMethod.ReturnType, new Type[] { typeof(Type) });
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
            var methods = interfaceType.GetMethods();
            foreach (var baseMethod in methods)
            {
                var parameterInfos = baseMethod.GetParameters();
                var parameterTypes = ProxyUtil.GetType(parameterInfos);
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;

                ILGenerator il = typeDynamicBuilder.DefineMethod(baseMethod.Name, methattr, calling, baseMethod.ReturnType, parameterInfos);
                #region 定义局部变量
                var contextLocal = il.DeclareLocal(typeof(AopContext));
                var methodBaseLocal = il.DeclareLocal(typeof(MethodBase));
                var methodLocal = il.DeclareLocal(typeof(MethodInfo));
                var argsLocal = il.DeclareLocal(typeof(object[]));
                var exLocal = il.DeclareLocal(typeof(Exception));
                LocalBuilder returnLocal = null;
                if (baseMethod.ReturnType != null && baseMethod.ReturnType != typeof(void))
                {
                    returnLocal = il.DeclareLocal(baseMethod.ReturnType);
                }

                Label execEndLabel = il.DefineLabel();
                Label resultEndLabel = il.DefineLabel();
                Label exEndLabel = il.DefineLabel();
                #endregion

                #region 初始局部变量
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, contextLocal);
                if (returnLocal != null)
                {
                    if (!baseMethod.ReturnType.IsValueType)
                    {
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Stloc, returnLocal);
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
                il.Emit(OpCodes.Stloc, methodBaseLocal);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, targetTypeField);
                //il.Emit(OpCodes.Ldloc, methodBaseLocal);
                //il.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name"));
                il.Emit(OpCodes.Ldstr, baseMethod.Name);
                il.Emit(OpCodes.Ldloc, methodBaseLocal);
                il.Emit(OpCodes.Call, ProxyUtil.GetParameterTypeMethod);
                il.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string), typeof(Type[]) }));
                il.Emit(OpCodes.Stloc, methodLocal);

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
                il.Emit(OpCodes.Ldloc, methodLocal);
                il.Emit(OpCodes.Ldloc, argsLocal);
                il.Emit(OpCodes.Ldloc, contextLocal);
                il.Emit(OpCodes.Call, ProxyUtil.OnExecutingMethod);
                il.MarkLabel(execEndLabel);
                il.Emit(OpCodes.Nop);
                var exBlock = il.BeginExceptionBlock();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, targetField);
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg, i + 1);
                }
                il.Emit(OpCodes.Call, baseMethod);
                if (returnLocal != null)
                {
                    il.Emit(OpCodes.Stloc, returnLocal);
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
                if (returnLocal != null)
                {
                    il.Emit(OpCodes.Ldloc, returnLocal);
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
                if (returnLocal != null)
                {
                    il.Emit(OpCodes.Ldloc, returnLocal);
                }
                il.Emit(OpCodes.Ret);
            }

            proxyType = typeDynamicBuilder.CreateType();
            using (this.m_rwInterfaceLock.GetWriteLock())
            {
                this.m_interfaceProxyDic[interfaceType] = proxyType;
            }

            return proxyType;
        }

        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;
            this.assemblyDynamicBuilder = null;
            this.m_interfaceProxyDic.Clear();
            this.m_interfaceProxyDic = null;
            this.m_rwInterfaceLock.Dispose();
            this.m_rwInterfaceLock = null;
        }
    }
}

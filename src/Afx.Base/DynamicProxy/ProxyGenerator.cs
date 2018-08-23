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
    /// aop代理工厂
    /// </summary>
    public class ProxyGenerator : IDisposable
    {
        private static int globalIdentify = 0;
        private static int GetGlobalId()
        {
            return Interlocked.Increment(ref globalIdentify);
        }

        private string m_moduleName;
        private ModuleBuilder m_moduleBuilder;
        private ReadWriteLock m_rwLock;
        private ReadWriteLock m_rwInterfaceLock;
        private Dictionary<Type, Type> m_classProxyDic;
        private Dictionary<Type, Type> m_interfaceProxyDic;

        private AssemblyBuilder m_assemblyBuilder;
#if NETCOREAPP || NETSTANDARD
        private AssemblyBuilderAccess m_assemblyBuilderAccess = AssemblyBuilderAccess.Run;
#else
        private AssemblyBuilderAccess m_assemblyBuilderAccess = AssemblyBuilderAccess.Run;
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProxyGenerator()
        {
            this.m_moduleName = ProxyUtil.MODULE_NAME + GetGlobalId();

            var assemblyName = new AssemblyName(this.m_moduleName);
            assemblyName.Version = new Version(1, 0, 0, 0);
#if NETCOREAPP || NETSTANDARD
            this.m_assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, this.m_assemblyBuilderAccess);
            this.m_moduleBuilder = this.m_assemblyBuilder.DefineDynamicModule(this.m_moduleName);
#else
            this.m_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, this.m_assemblyBuilderAccess);
            if (this.m_assemblyBuilderAccess == AssemblyBuilderAccess.RunAndSave)
            {
                this.m_moduleBuilder = this.m_assemblyBuilder.DefineDynamicModule(this.m_moduleName, this.m_moduleName + ".dll");
            }
            else
            {
                this.m_moduleBuilder = this.m_assemblyBuilder.DefineDynamicModule(this.m_moduleName);
            }
#endif
            this.m_rwLock = new ReadWriteLock();
            this.m_classProxyDic = new Dictionary<Type, Type>();
            this.m_rwInterfaceLock = new ReadWriteLock();
            this.m_interfaceProxyDic = new Dictionary<Type, Type>();
        }
 
        private string GetDynamicName(Type targetType)
        {
            return string.Format("{0}.{1}Proxy{2}",this.m_moduleName, targetType.Name, GetGlobalId());
        }
        
        private List<MethodInfo> GetMethod(Type t)
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

        public Func<Type, IAop> AopFunc;

        public T CreateClassProxy<T>( bool enableAop, Type[] aopType) where T: class
        {
            return this.CreateClassProxy<T>(null, enableAop, aopType);
        }

        public T CreateClassProxy<T>(object[] args, bool enableAop, Type[] aopType) where T : class
        {
            return (T)this.CreateClassProxy(typeof(T), null, enableAop, aopType);
        }

        public object CreateClassProxy(Type targetType, bool enableAop, Type[] aopType)
        {
            return this.CreateClassProxy(targetType, null, enableAop, aopType);
        }

        public object CreateClassProxy(Type targetType, object[] args, bool enableAop, Type[] aopType)
        {
            object result = null;
            if (targetType == null) throw new ArgumentNullException("targetType");
            Type proxyType = targetType;
            if (!ProxyUtil.IsProxy(targetType))
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
            if(targetCtors.Length == 0) throw new ArgumentException(targetType.FullName + "没有公共构造函数!", "targetType");

            Type proxyType = null;

            using (this.m_rwLock.GetReadLock())
            {
                this.m_classProxyDic.TryGetValue(targetType, out proxyType);
            }
            if (proxyType != null) return proxyType;

            var interfaceTypes = targetType.GetInterfaces();
            var typeBuilder = this.m_moduleBuilder.DefineType(this.GetDynamicName(targetType), targetType.Attributes, targetType, interfaceTypes);
            typeBuilder.AddInterfaceImplementation(typeof(IProxy));
            var targetTypeField = typeBuilder.DefineField("__m_targetType", typeof(Type), FieldAttributes.Private);
            var aopFuncsField = typeBuilder.DefineField("__m_aopFuncs", typeof(Func<IAop>[]), FieldAttributes.Private);

            // 构造器
            {
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
                foreach (var baseCtor in targetCtors)
                {
                    var paramets = ProxyUtil.GetParameterType(baseCtor);
                    ConstructorBuilder ctor = typeBuilder.DefineConstructor(methattr, calling, paramets);
                    ILGenerator ctorIL = ctor.GetILGenerator();
                    ctorIL.Emit(OpCodes.Ldarg_0);
                    for (int i = 0; i < paramets.Length; i++)
                        ctorIL.Emit(OpCodes.Ldarg, i + 1);
                    ctorIL.Emit(OpCodes.Call, baseCtor);
                    ctorIL.Emit(OpCodes.Ret);
                }
            }

            // IProxy
            {
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;

                // SetTargetTypeMethod
                var methodBuilder = typeBuilder.DefineMethod(ProxyUtil.SetTargetTypeMethod.Name, methattr, calling, ProxyUtil.SetTargetTypeMethod.ReturnType, new Type[] { typeof(Type) });
                ILGenerator il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, targetTypeField);
                il.Emit(OpCodes.Ret);

                //GetTargetTypeMethod
                methodBuilder = typeBuilder.DefineMethod(ProxyUtil.GetTargetTypeMethod.Name, methattr, calling, ProxyUtil.GetTargetTypeMethod.ReturnType, Type.EmptyTypes);
                il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, targetTypeField);
                il.Emit(OpCodes.Ret);

                //SetAopFuncMethod
                methodBuilder = typeBuilder.DefineMethod(ProxyUtil.SetAopFuncMethod.Name, methattr, calling, ProxyUtil.SetAopFuncMethod.ReturnType, new Type[] { typeof(Func<IAop>[]) });
                il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, aopFuncsField);
                il.Emit(OpCodes.Ret);

                //GetAopFuncMethod
                methodBuilder = typeBuilder.DefineMethod(ProxyUtil.GetAopFuncMethod.Name, methattr, calling, ProxyUtil.GetAopFuncMethod.ReturnType, Type.EmptyTypes);
                il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ret);
            }

            //方法
            var methods = this.GetMethod(targetType);
            MethodAttributes newMethodAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            foreach (var baseMethod in methods)
            {
                var intefacemethlist = this.GetOverrideMethods(baseMethod, interfaceTypes);
                if (!baseMethod.IsVirtual && intefacemethlist.Count == 0)
                {
                    continue;
                }

                var parameterTypes = ProxyUtil.GetParameterType(baseMethod);
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
                if (baseMethod.Attributes == newMethodAttr || baseMethod.Attributes == (MethodAttributes.Public | MethodAttributes.HideBySig))
                {
                    methattr = MethodAttributes.Public | MethodAttributes.HideBySig;
                }
                var methodBuilder = typeBuilder.DefineMethod(baseMethod.Name, methattr, calling, baseMethod.ReturnType, parameterTypes);
                ILGenerator il = methodBuilder.GetILGenerator();
                #region 定义局部变量
                var contextLocal = il.DeclareLocal(typeof(AopContext));
                var methodBaseLocal = il.DeclareLocal(typeof(MethodBase));
                var methodLocal = il.DeclareLocal(typeof(MethodInfo));
                var argsLocal = il.DeclareLocal(typeof(object[]));
                var exLocal = il.DeclareLocal(typeof(Exception));
                LocalBuilder returnLocal = null;//5
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
                il.Emit(OpCodes.Stloc_S, contextLocal);
                if (returnLocal != null)
                {
                    if (!baseMethod.ReturnType.IsValueType)
                    {
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Stloc_S, returnLocal);
                    }
                }
                #endregion

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse_S, execEndLabel);
                il.Emit(OpCodes.Newobj, typeof(AopContext).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc_S, contextLocal);
                il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Static | BindingFlags.Public));
                il.Emit(OpCodes.Stloc_S, methodBaseLocal);
                
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, targetTypeField);
                il.Emit(OpCodes.Ldloc_S, methodBaseLocal);
                il.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name"));
                il.Emit(OpCodes.Ldloc_S, methodBaseLocal);
                il.Emit(OpCodes.Call, ProxyUtil.GetParameterTypeMethod);
                il.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string), typeof(Type[]) }));
                il.Emit(OpCodes.Stloc_S, methodLocal);

                il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
                il.Emit(OpCodes.Newarr, typeof(Object));
                il.Emit(OpCodes.Stloc_S, argsLocal);
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc_S, argsLocal);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i + 1);
                    if (parameterTypes[i].IsValueType)
                    {
                        il.Emit(OpCodes.Box, parameterTypes[i]);
                    }
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, methodLocal);
                il.Emit(OpCodes.Ldloc_S, argsLocal);
                il.Emit(OpCodes.Ldloc_S, contextLocal);
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
                if (returnLocal != null)
                {
                    il.Emit(OpCodes.Stloc_S, returnLocal);
                }
                
                il.BeginCatchBlock(typeof(Exception));
                il.Emit(OpCodes.Stloc_S, exLocal);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse_S, exEndLabel);

                il.Emit(OpCodes.Ldloc_S, contextLocal);
                il.Emit(OpCodes.Ldloc_S, exLocal);
                il.Emit(OpCodes.Call, ProxyUtil.OnExceptionMethod);

                il.Emit(OpCodes.Nop);
                il.MarkLabel(exEndLabel);
                il.Emit(OpCodes.Ldloc_S, exLocal);
                il.Emit(OpCodes.Throw);
                il.EndExceptionBlock();
   
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse_S, resultEndLabel);
                il.Emit(OpCodes.Ldloc_S, contextLocal);
                if (returnLocal != null)
                {
                    il.Emit(OpCodes.Ldloc_S, returnLocal);
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
                    il.Emit(OpCodes.Ldloc_S, returnLocal);
                }
                il.Emit(OpCodes.Ret);
            }

#if NETSTANDARD
            proxyType = typeBuilder.CreateTypeInfo();
#else
            proxyType = typeBuilder.CreateType();
#endif
            using (this.m_rwLock.GetWriteLock())
            {
                this.m_classProxyDic[targetType] = proxyType;
            }

            return proxyType;
        }

        private List<MethodInfo> GetOverrideMethods(MethodInfo meth, Type[] interfaceTypes)
        {
            List<MethodInfo> list = new List<MethodInfo>(interfaceTypes != null ? interfaceTypes.Length : 0);
            if (interfaceTypes != null && interfaceTypes.Length > 0)
            {
                foreach(var t in interfaceTypes)
                {
                    var interfaceMeth = t.GetMethod(meth.Name, ProxyUtil.GetParameterType(meth));
                    if (interfaceMeth != null && interfaceMeth.ReturnType == meth.ReturnType)
                        list.Add(interfaceMeth);
                }
            }

            return list;
        }


        public T CreateInterfaceProxy<T>(T target, bool enableAop, Type[] aopType)
        {
            return (T)this.CreateInterfaceProxy(typeof(T), target, enableAop, aopType);
        }
        public object CreateInterfaceProxy(Type interfaceType, object target, bool enableAop, Type[] aopType)
        {
            object result = target;
            if (interfaceType == null) throw new ArgumentNullException("targetType");
            if (target == null) throw new ArgumentNullException("target");
            if (!interfaceType.IsInterface) throw new ArgumentException(interfaceType.FullName + "不是interface!", "interfaceType");
            Type targetType = target.GetType();
            if (!interfaceType.IsAssignableFrom(targetType)) throw new ArgumentException("target 不是" + interfaceType.FullName + "类型", "target");

            if (!ProxyUtil.IsProxy(targetType))
            {
                var proxyType = this.GetInterfaceProxyType(interfaceType);
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

        private Type GetInterfaceProxyType(Type interfaceType)
        {
            Type proxyType = null;
            if (interfaceType == null) throw new ArgumentNullException("targetType");
            if (!interfaceType.IsInterface) throw new ArgumentException(interfaceType.FullName + "不是interface!", "interfaceType");
            using (this.m_rwInterfaceLock.GetReadLock())
            {
                this.m_interfaceProxyDic.TryGetValue(interfaceType, out proxyType);
            }
            if (proxyType != null) return proxyType;

            var typeBuilder = this.m_moduleBuilder.DefineType(this.GetDynamicName(interfaceType), TypeAttributes.Class | TypeAttributes.Public, typeof(object), new Type[] { interfaceType });
            typeBuilder.AddInterfaceImplementation(typeof(IProxy));
            var targetTypeField = typeBuilder.DefineField("__m_targetType", typeof(Type), FieldAttributes.Private);
            var aopFuncsField = typeBuilder.DefineField("__m_aopFuncs", typeof(Func<IAop>[]), FieldAttributes.Private);
            var targetField = typeBuilder.DefineField("__m_target", interfaceType, FieldAttributes.Private);

            // 构造器
            {
                var baseCtor = typeof(object).GetConstructor(Type.EmptyTypes);
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
                ConstructorBuilder ctor = typeBuilder.DefineConstructor(methattr, calling, new Type[] { interfaceType });
                ILGenerator ctorIL = ctor.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Call, baseCtor);
                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Ldarg_1);
                ctorIL.Emit(OpCodes.Stfld, targetField);
                ctorIL.Emit(OpCodes.Ret);
            }

            // IProxy
            {
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
                // SetTargetTypeMethod
                var methodBuilder = typeBuilder.DefineMethod(ProxyUtil.SetTargetTypeMethod.Name, methattr, calling, ProxyUtil.SetTargetTypeMethod.ReturnType, new Type[] { typeof(Type) });
                ILGenerator il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, targetTypeField);
                il.Emit(OpCodes.Ret);

                //GetTargetTypeMethod
                methodBuilder = typeBuilder.DefineMethod(ProxyUtil.GetTargetTypeMethod.Name, methattr, calling, ProxyUtil.GetTargetTypeMethod.ReturnType, Type.EmptyTypes);
                il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, targetTypeField);
                il.Emit(OpCodes.Ret);

                //SetAopFuncMethod
                methodBuilder = typeBuilder.DefineMethod(ProxyUtil.SetAopFuncMethod.Name, methattr, calling, ProxyUtil.SetAopFuncMethod.ReturnType, new Type[] { typeof(Func<IAop>[]) });
                il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, aopFuncsField);
                il.Emit(OpCodes.Ret);

                //GetAopFuncMethod
                methodBuilder = typeBuilder.DefineMethod(ProxyUtil.GetAopFuncMethod.Name, methattr, calling, ProxyUtil.GetAopFuncMethod.ReturnType, Type.EmptyTypes);
                il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ret);
            }

            //方法
            var methods = interfaceType.GetMethods();
            foreach (var baseMethod in methods)
            {
                var parameterTypes = ProxyUtil.GetParameterType(baseMethod);
                MethodAttributes methattr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
                var methodBuilder = typeBuilder.DefineMethod(baseMethod.Name, methattr, calling, baseMethod.ReturnType, parameterTypes);
                ILGenerator il = methodBuilder.GetILGenerator();
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
                il.Emit(OpCodes.Stloc_S, contextLocal);
                if (returnLocal != null)
                {
                    if (!baseMethod.ReturnType.IsValueType)
                    {
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Stloc_S, returnLocal);
                    }
                }
                #endregion

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse_S, execEndLabel);
                il.Emit(OpCodes.Newobj, typeof(AopContext).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc_S, contextLocal);
                il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Static | BindingFlags.Public));
                il.Emit(OpCodes.Stloc_S, methodBaseLocal);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, targetTypeField);
                il.Emit(OpCodes.Ldloc_S, methodBaseLocal);
                il.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name"));
                il.Emit(OpCodes.Ldloc_S, methodBaseLocal);
                il.Emit(OpCodes.Call, ProxyUtil.GetParameterTypeMethod);
                il.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string), typeof(Type[]) }));
                il.Emit(OpCodes.Stloc_S, methodLocal);

                il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
                il.Emit(OpCodes.Newarr, typeof(Object));
                il.Emit(OpCodes.Stloc_S, argsLocal);
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc_S, argsLocal);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i + 1);
                    if (parameterTypes[i].IsValueType)
                    {
                        il.Emit(OpCodes.Box, parameterTypes[i]);
                    }
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, methodLocal);
                il.Emit(OpCodes.Ldloc_S, argsLocal);
                il.Emit(OpCodes.Ldloc_S, contextLocal);
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
                    il.Emit(OpCodes.Stloc_S, returnLocal);
                }

                il.BeginCatchBlock(typeof(Exception));
                il.Emit(OpCodes.Stloc_S, exLocal);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse_S, exEndLabel);

                il.Emit(OpCodes.Ldloc_S, contextLocal);
                il.Emit(OpCodes.Ldloc_S, exLocal);
                il.Emit(OpCodes.Call, ProxyUtil.OnExceptionMethod);

                il.Emit(OpCodes.Nop);
                il.MarkLabel(exEndLabel);
                il.Emit(OpCodes.Ldloc_S, exLocal);
                il.Emit(OpCodes.Throw);
                il.EndExceptionBlock();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, aopFuncsField);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt_Un);
                il.Emit(OpCodes.Brfalse_S, resultEndLabel);
                il.Emit(OpCodes.Ldloc_S, contextLocal);
                if (returnLocal != null)
                {
                    il.Emit(OpCodes.Ldloc_S, returnLocal);
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
                    il.Emit(OpCodes.Ldloc_S, returnLocal);
                }
                il.Emit(OpCodes.Ret);
            }

#if NETSTANDARD
            proxyType = typeBuilder.CreateTypeInfo();
#else
            proxyType = typeBuilder.CreateType();
#endif
            using (this.m_rwInterfaceLock.GetWriteLock())
            {
                this.m_interfaceProxyDic[interfaceType] = proxyType;
            }

            return proxyType;
        }

        public void Dispose()
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Threading;

using Afx.Threading;

namespace Afx.Aop
{
    /// <summary>
    /// aop代理工厂
    /// </summary>
    public class AopProxy
    {
        private static MethodInfo m_OnExecutingMethod = typeof(AopUtils).GetMethod("OnExecuting", BindingFlags.Static | BindingFlags.Public);
        private static MethodInfo m_OnResultMethod = typeof(AopUtils).GetMethod("OnResult", BindingFlags.Static | BindingFlags.Public);
        private static MethodInfo m_OnExceptionMethod = typeof(AopUtils).GetMethod("OnException", BindingFlags.Static | BindingFlags.Public);
        private static MethodInfo m_GetParameterTypeMethod = typeof(AopUtils).GetMethod("GetParameterType", BindingFlags.Static | BindingFlags.Public);

        private static int globalIdentify = 0;
        private static int GetGlobalId()
        {
            return Interlocked.Increment(ref globalIdentify);
        }

        private int m_classId;
        private string m_moduleName;
        private ModuleBuilder m_moduleBuilder;
        private ReadWriteLock m_rwLock;
        private Dictionary<Type, Type> m_proxyTypeDic;

        private AssemblyBuilder m_assemblyBuilder;
#if NETCOREAPP || NETSTANDARD
        private AssemblyBuilderAccess m_assemblyBuilderAccess = AssemblyBuilderAccess.Run;
#else
        private AssemblyBuilderAccess m_assemblyBuilderAccess = AssemblyBuilderAccess.Run;
#endif


        /// <summary>
        /// 构造函数
        /// </summary>
        public AopProxy()
        {
            this.m_classId = GetGlobalId();
            this.m_moduleName = "Afx.Aop.Dynamic" + GetGlobalId();

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
            this.m_proxyTypeDic = new Dictionary<Type, Type>();
        }

        //public string SaveAssembly()
        //{
        //    if (this.m_assemblyBuilderAccess == AssemblyBuilderAccess.RunAndSave)
        //    {
        //        this.m_assemblyBuilder.Save(this.m_moduleName);
        //    }

        //    return this.m_moduleName;
        //}
                
        private string GetDynamicName(Type tagetType)
        {
            return string.Format("{0}.{1}{2}",this.m_moduleName, tagetType.Name, GetGlobalId());
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
                        var parameterTypes = AopUtils.GetParameterType(met);
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
        /// 获取指定类型的代理类型
        /// </summary>
        /// <param name="tagetType">指定类型</param>
        /// <returns>返回代理类型</returns>
        public Type GetProxyType(Type tagetType)
        {
            Type proxyType = null;
            if (tagetType != null && tagetType.IsClass && !tagetType.IsAbstract
                && tagetType.IsPublic && !tagetType.IsSealed
                && tagetType.IsVisible)
            {
                var tagetCtors = tagetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                if (tagetCtors == null || tagetCtors.Length == 0) return proxyType;

                using (this.m_rwLock.GetReadLock())
                {
                    this.m_proxyTypeDic.TryGetValue(tagetType, out proxyType);
                }
                if (proxyType != null) return proxyType;
                
                using (this.m_rwLock.GetWriteLock())
                {
                    this.m_proxyTypeDic.TryGetValue(tagetType, out proxyType);
                    if (proxyType != null) return proxyType;
                    var interfaceTypes = tagetType.GetInterfaces();
                    var typeBuilder = this.m_moduleBuilder.DefineType(this.GetDynamicName(tagetType), tagetType.Attributes, tagetType, interfaceTypes);

                    var tagetTypeField = typeBuilder.DefineField("m_tagetType", typeof(Type), FieldAttributes.Private);
                    // 构造器
                    foreach (var baseCtor in tagetCtors)
                    {
                        var paramets = AopUtils.GetParameterType(baseCtor);
                        ConstructorBuilder ctor = typeBuilder.DefineConstructor(baseCtor.Attributes, baseCtor.CallingConvention, paramets);
                        ILGenerator ctorIL = ctor.GetILGenerator();
                        ctorIL.Emit(OpCodes.Ldarg_0);
                        for (int i = 0; i < paramets.Length; i++)
                            ctorIL.Emit(OpCodes.Ldarg, i + 1);
                        ctorIL.Emit(OpCodes.Call, baseCtor);
                        ctorIL.Emit(OpCodes.Ldarg_0);
                        ctorIL.Emit(OpCodes.Ldarg_0);
                        ctorIL.Emit(OpCodes.Callvirt, typeof(Object).GetMethod("GetType"));
                        ctorIL.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("get_BaseType"));
                        ctorIL.Emit(OpCodes.Stfld, tagetTypeField);
                        ctorIL.Emit(OpCodes.Ret);
                    }
                    //方法
                    var methods = this.GetMethod(tagetType);
                    var aopContextType = typeof(AopContext);
                    foreach (var met in methods)
                    {
                        var intefacemethlist = this.GetOverrideMethods(met, interfaceTypes);
                        if (!met.IsVirtual && intefacemethlist.Count == 0)
                        {
                            continue;
                        }

                        var parameterTypes = AopUtils.GetParameterType(met);
                        MethodAttributes methattr = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig;
                        if ((met.Attributes & MethodAttributes.Final) == MethodAttributes.Final)
                        {
                            methattr = methattr | MethodAttributes.VtableLayoutMask;
                        }
                        var methodBuilder = typeBuilder.DefineMethod(met.Name, methattr, CallingConventions.Standard | CallingConventions.HasThis, met.ReturnType, parameterTypes);
                        ILGenerator il = methodBuilder.GetILGenerator();
                        var aAopInfoLocal = il.DeclareLocal(typeof(AopInfoModel));//0
                        var aopContextLocal = il.DeclareLocal(aopContextType);//1
                        var currentMethodLocal = il.DeclareLocal(typeof(MethodBase));//2
                        var nameLocal = il.DeclareLocal(typeof(string));//3
                        var paramsLocal = il.DeclareLocal(typeof(Type[]));//4
                        LocalBuilder returnLocal = null;//5
                        if (met.ReturnType != null && met.ReturnType != typeof(void))
                        {
                            returnLocal = il.DeclareLocal(met.ReturnType);
                        }
                        var exLocal = il.DeclareLocal(typeof(Exception));//5.6
                        Label endOfMthd = il.DefineLabel();

                        //il.Emit(OpCodes.Nop);
                        il.Emit(OpCodes.Newobj, typeof(AopInfoModel).GetConstructor(Type.EmptyTypes));
                        il.Emit(OpCodes.Stloc, aAopInfoLocal);
                        il.Emit(OpCodes.Ldloc, aAopInfoLocal);
                        il.Emit(OpCodes.Ldc_I4, this.m_classId);
                        il.Emit(OpCodes.Callvirt, typeof(AopInfoModel).GetMethod("set_ClassId"));
                        //il.Emit(OpCodes.Nop);
                        il.Emit(OpCodes.Newobj, aopContextType.GetConstructor(Type.EmptyTypes));
                        il.Emit(OpCodes.Stloc, aopContextLocal);
                        il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Static | BindingFlags.Public));
                        il.Emit(OpCodes.Stloc, currentMethodLocal);
                        il.Emit(OpCodes.Ldloc, currentMethodLocal);
                        il.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name"));
                        il.Emit(OpCodes.Stloc, nameLocal);
                        il.Emit(OpCodes.Ldloc, currentMethodLocal);
                        il.Emit(OpCodes.Call, m_GetParameterTypeMethod);
                        il.Emit(OpCodes.Stloc, paramsLocal);
                        if (returnLocal != null)
                        {
                            if (!met.ReturnType.IsValueType)
                            {
                                il.Emit(OpCodes.Ldnull);
                                il.Emit(OpCodes.Stloc, returnLocal);
                            }
                        }
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Stloc, exLocal);
                        il.Emit(OpCodes.Ldloc, aopContextLocal);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, tagetTypeField);
                        il.Emit(OpCodes.Callvirt, aopContextType.GetMethod("set_TagetType"));
                        //il.Emit(OpCodes.Nop);
                        il.Emit(OpCodes.Ldloc, aopContextLocal);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, tagetTypeField);
                        il.Emit(OpCodes.Ldloc, nameLocal);
                        il.Emit(OpCodes.Ldloc, paramsLocal);
                        il.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string), typeof(Type[]) }));
                        il.Emit(OpCodes.Callvirt, aopContextType.GetMethod("set_Method"));
                        //il.Emit(OpCodes.Nop);
                        il.Emit(OpCodes.Ldloc, aopContextLocal);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Callvirt, aopContextType.GetMethod("set_Taget"));
                        //il.Emit(OpCodes.Nop);
                        il.Emit(OpCodes.Ldloc, aopContextLocal);
                        il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
                        il.Emit(OpCodes.Newarr, typeof(Object));
                        il.Emit(OpCodes.Callvirt, aopContextType.GetMethod("set_Parameters"));
                        //il.Emit(OpCodes.Nop);
                        for (int i = 0; i < parameterTypes.Length; i++)
                        {
                            il.Emit(OpCodes.Ldloc, aopContextLocal);
                            il.Emit(OpCodes.Callvirt, aopContextType.GetMethod("get_Parameters"));
                            il.Emit(OpCodes.Ldc_I4, i);
                            il.Emit(OpCodes.Ldarg, i + 1);
                            if (parameterTypes[i].IsValueType)
                            {
                                il.Emit(OpCodes.Box, parameterTypes[i]);
                            }
                            il.Emit(OpCodes.Stelem_Ref);
                        }
                        il.Emit(OpCodes.Ldloc, aAopInfoLocal);
                        il.Emit(OpCodes.Ldloc, aopContextLocal);
                        il.Emit(OpCodes.Call, m_OnExecutingMethod);
                        //il.Emit(OpCodes.Nop);

                        var exBlock = il.BeginExceptionBlock();
                        il.Emit(OpCodes.Ldarg_0);
                        for (int i = 0; i < parameterTypes.Length; i++)
                        {
                            il.Emit(OpCodes.Ldarg, i + 1);
                        }
                        il.Emit(OpCodes.Call, met);
                        if (returnLocal != null)
                        {
                            il.Emit(OpCodes.Stloc, returnLocal);
                        }
                        il.Emit(OpCodes.Leave_S, endOfMthd);
                        il.BeginCatchBlock(typeof(Exception));
                        il.Emit(OpCodes.Stloc, exLocal);
                        il.Emit(OpCodes.Ldloc, aAopInfoLocal);
                        il.Emit(OpCodes.Ldloc, aopContextLocal);
                        il.Emit(OpCodes.Ldloc, exLocal);
                        il.Emit(OpCodes.Call, m_OnExceptionMethod);
                        il.Emit(OpCodes.Ldloc, exLocal);
                        il.Emit(OpCodes.Throw);
                        il.EndExceptionBlock();

                        il.MarkLabel(endOfMthd);
                        il.Emit(OpCodes.Ldloc, aAopInfoLocal);
                        il.Emit(OpCodes.Ldloc, aopContextLocal);
                        if (returnLocal != null)
                        {
                            il.Emit(OpCodes.Ldloc, returnLocal);
                            if (met.ReturnType.IsValueType)
                            {
                                il.Emit(OpCodes.Box, met.ReturnType);
                            }
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                        il.Emit(OpCodes.Call, m_OnResultMethod);
                        if (returnLocal != null)
                        {
                            il.Emit(OpCodes.Ldloc, returnLocal);
                        }
                        il.Emit(OpCodes.Ret);

                        foreach (var me in intefacemethlist)
                        {
                            typeBuilder.DefineMethodOverride(methodBuilder, me);
                        }
                    }
#if NETSTANDARD
                    proxyType = typeBuilder.CreateTypeInfo();
#else
                    proxyType = typeBuilder.CreateType();
#endif
                }
                this.m_proxyTypeDic[tagetType] = proxyType;
            }

            return proxyType ?? tagetType;
        }

        private List<MethodInfo> GetOverrideMethods(MethodInfo meth, Type[] interfaceTypes)
        {
            List<MethodInfo> list = new List<MethodInfo>(interfaceTypes != null ? interfaceTypes.Length : 0);
            if (interfaceTypes != null && interfaceTypes.Length > 0)
            {
                foreach(var t in interfaceTypes)
                {
                    var interfaceMeth = t.GetMethod(meth.Name, AopUtils.GetParameterType(meth));
                    if (interfaceMeth != null && interfaceMeth.ReturnType == meth.ReturnType)
                        list.Add(interfaceMeth);
                }
            }

            return list;
        }

        /// <summary>
        /// 获取代理对象
        /// </summary>
        /// <typeparam name="T">要代理的类型</typeparam>
        /// <returns>返回代理对象</returns>
        public T Get<T>()
        {
            return this.Get<T>(typeof(T), null);
        }

        /// <summary>
        /// 获取代理对象
        /// </summary>
        /// <typeparam name="T">要代理的类型</typeparam>
        /// <param name="args">要代理的类型构函数参数</param>
        /// <returns>返回代理对象</returns>
        public T Get<T>(object[] args)
        {
            return this.Get<T>(typeof(T), args);
        }

        /// <summary>
        /// 获取代理对象
        /// </summary>
        /// <typeparam name="T">代理的类型</typeparam>
        /// <param name="tagetType">要代理的类型</param>
        /// <returns>返回代理对象</returns>
        public T Get<T>(Type tagetType)
        {
            return this.Get<T>(tagetType, null);
        }

        /// <summary>
        /// 获取代理对象
        /// </summary>
        /// <typeparam name="T">代理的类型</typeparam>
        /// <param name="tagetType">要代理的类型</param>
        /// <param name="args">要代理的类型构函数参数</param>
        /// <returns>返回代理对象</returns>
        public T Get<T>(Type tagetType, object[] args)
        {
            object obj = null;
            if (tagetType != null && tagetType.IsClass && !tagetType.IsAbstract)
            {
                var proxyType = GetProxyType(tagetType);
                if (proxyType == null)
                {
                    proxyType = tagetType;
                }

                if (args != null && args.Length > 0)
                {
                   obj = Activator.CreateInstance(proxyType, args);
                }
                else
                {
                   obj = Activator.CreateInstance(proxyType);
                }
            }

            T proxy = (T)obj;

            return proxy;
        }

        /// <summary>
        /// 添加全局IAop实现类型
        /// </summary>
        /// <param name="aopTypeList">IAop实现类型 list</param>
        public void AddOfGlobal(List<Type> aopTypeList)
        {
            AopUtils.AddOfGlobal(this.m_classId, aopTypeList);
        }

        /// <summary>
        /// 添加全局IAop实现类型
        /// </summary>
        /// <param name="aopType">IAop实现类型</param>
        public void AddOfGlobal(Type aopType)
        {
            if (aopType != null)
            {
                AopUtils.AddOfGlobal(this.m_classId, new List<Type> { aopType });
            }
        }

        /// <summary>
        /// 添加指定类型的IAop实现类型
        /// </summary>
        /// <param name="tagetType">需要aop的类型</param>
        /// <param name="aopTypeList">IAop实现类型 list</param>
        public void AddOfType(Type tagetType, List<Type> aopTypeList)
        {
            AopUtils.AddOfType(this.m_classId, tagetType, aopTypeList);
        }

        /// <summary>
        /// 添加指定类型的IAop实现类型
        /// </summary>
        /// <param name="tagetType">需要aop的类型</param>
        /// <param name="aopType">IAop实现类型</param>
        public void AddOfType(Type tagetType, Type aopType)
        {
            if (aopType != null)
            {
                var list = new List<Type> { aopType } ;
                AopUtils.AddOfType(this.m_classId, tagetType, list);
            }
        }
        
    }
}

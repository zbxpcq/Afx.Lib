using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

using Afx.Threading;
using Afx.Collections;

namespace Afx.Map
{
    class MemberModel
    {
        public Type type { get; set; }
        public MemberInfo member { get; set; }
    }

    /// <summary>
    /// model map
    /// </summary>
    public class MapFactory
    {
        private static int typeId = 0;
        private static int GetTypeId()
        {
            return Interlocked.Increment(ref typeId);
        }

        private string m_moduleName;
        private ModuleBuilder m_moduleBuilder;
#if NET20
        private Afx.Collections.SafeDictionary<TypeKey, IToObject> m_toObjectDic;
#else
        private System.Collections.Concurrent.ConcurrentDictionary<TypeKey, IToObject> m_toObjectDic;
#endif

        private AssemblyBuilderAccess m_assemblyBuilderAccess = AssemblyBuilderAccess.Run;
        private AssemblyBuilder m_assemblyBuilder;

        /// <summary>
        /// ObjectFactory
        /// </summary>
        public MapFactory()
        {
            this.m_moduleName = "Afx.Map_" + Guid.NewGuid().ToString("n");
            var assemblyName = new AssemblyName(this.m_moduleName);
            assemblyName.Version = new Version(1, 0, 0, 0);
#if NETCOREAPP || NETSTANDARD
            m_assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, m_assemblyBuilderAccess);
            this.m_moduleBuilder = m_assemblyBuilder.DefineDynamicModule(this.m_moduleName);
#else
            m_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, m_assemblyBuilderAccess);
            if (this.m_assemblyBuilderAccess == AssemblyBuilderAccess.RunAndSave)
            {
                this.m_moduleBuilder = m_assemblyBuilder.DefineDynamicModule(this.m_moduleName, this.m_moduleName + ".dll");
            }
            else
            {
                this.m_moduleBuilder = m_assemblyBuilder.DefineDynamicModule(this.m_moduleName);
            }
#endif
#if NET20
            this.m_toObjectDic = new Afx.Collections.SafeDictionary<TypeKey, IToObject>();
#else
            this.m_toObjectDic = new System.Collections.Concurrent.ConcurrentDictionary<TypeKey, IToObject>();
#endif
        }

#if !NETCOREAPP && !NETSTANDARD && DEBUG
        public string SaveAssembly()
        {
            if (this.m_assemblyBuilderAccess == AssemblyBuilderAccess.RunAndSave)
            {
                this.m_assemblyBuilder.Save(this.m_moduleName);
            }

            return this.m_moduleName;
        }
#endif

        private string GetDynamicName(TypeKey key)
        {
            return string.Format("{0}_To_{1}_{2}", key.FromType.Name, key.ToType.Name, GetTypeId());
        }

        private Type CreateMapType(TypeKey key)
        {
            Type toObjectType = null;
            if (key != null && key.FromType != null && key.ToType != null
                && key.FromType.IsClass
                && key.ToType.IsClass && key.ToType.GetConstructor(Type.EmptyTypes) != null
                )
            {
                var toObjectBaseType = typeof(object);
                var typeBuilder = this.m_moduleBuilder.DefineType(this.GetDynamicName(key), toObjectBaseType.Attributes, toObjectBaseType, new Type[] { typeof(IToObject) });

#region ctor
                var baseCtor = toObjectBaseType.GetConstructor(Type.EmptyTypes);
                ConstructorBuilder ctor = typeBuilder.DefineConstructor(baseCtor.Attributes, baseCtor.CallingConvention, Type.EmptyTypes);
                ILGenerator ctorIL = ctor.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Call, baseCtor);
                ctorIL.Emit(OpCodes.Ret);
#endregion

                var fromProperties = new List<PropertyInfo>(key.FromType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));
                var toProperties = new List<PropertyInfo>(key.ToType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty));

                #region To(objec obj)
                MethodAttributes methattr = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig;
                MethodBuilder toMethodBuilder = typeBuilder.DefineMethod("To", methattr, CallingConventions.Standard, typeof(object), new Type[] { typeof(object) });

                ILGenerator il = toMethodBuilder.GetILGenerator();
                var resultLocal = il.DeclareLocal(typeof(object));
                var oLocal = il.DeclareLocal(typeof(object));
                var fromLocal = il.DeclareLocal(key.FromType);
                var toLocal = il.DeclareLocal(key.ToType);

                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, resultLocal);

                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, oLocal);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, key.FromType);
                il.Emit(OpCodes.Stloc, fromLocal);

                il.Emit(OpCodes.Newobj, key.ToType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc, toLocal);

                foreach (var fp in fromProperties)
                {
                    var tp = toProperties.Find(q => q.Name == fp.Name && q.PropertyType == fp.PropertyType);
                    if (tp != null)
                    {
                        il.Emit(OpCodes.Ldloc, toLocal);
                        il.Emit(OpCodes.Ldloc, fromLocal);

                        il.Emit(OpCodes.Callvirt, fp.GetGetMethod());
                        il.Emit(OpCodes.Callvirt, tp.GetSetMethod());
                    }
                }

                il.Emit(OpCodes.Ldloc, toLocal);
                if (key.ToType.IsValueType)
                {
                    il.Emit(OpCodes.Box, key.ToType);
                }
                il.Emit(OpCodes.Stloc, resultLocal);

                il.Emit(OpCodes.Ldloc, resultLocal);
                il.Emit(OpCodes.Ret);
                #endregion to2MethodBuilder end
#if NETSTANDARD
                toObjectType = typeBuilder.CreateTypeInfo();
#else
                toObjectType = typeBuilder.CreateType();
#endif
            }

            return toObjectType;
        }

        private IToObject GetToObject(TypeKey key)
        {
            IToObject toObject = null;
            if (key != null && key.FromType != null && key.ToType != null)
            {
                if (this.m_toObjectDic.TryGetValue(key, out toObject)) return toObject;

                try
                {
                    var type = this.CreateMapType(key);
                    if (type == null) return toObject;

                    object obj = Activator.CreateInstance(type);
                    toObject = obj as IToObject;
                    this.m_toObjectDic.TryAdd(key, toObject);
                }
                catch { }
            }

            return toObject;
        }

        private bool IsExsitType(Type[] arr, Type t)
        {
            if (arr != null && arr.Length > 0 && t != null)
            {
                foreach (var item in arr)
                {
                    if (item == t)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// map to
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="resultType"></param>
        /// <returns></returns>
        public object To(object obj, Type resultType)
        {
            if (obj != null && resultType != null)
            {
                //string
                if (resultType == typeof(string))
                {
                    object o = obj.ToString();
                    return o;
                }

                //Enum
                if (resultType.IsEnum)
                {
                    object o = null;
                    try { o = Enum.Parse(resultType, obj.ToString(), true); }
                    catch { }
                    return o;
                }

                var objType = obj.GetType();
                //ValueType
                if (resultType.IsValueType && (resultType.IsPrimitive
                    || resultType.FullName.ToLower() == "system." + resultType.Name.ToLower()
                    && resultType.Assembly.FullName.StartsWith("mscorlib,", StringComparison.OrdinalIgnoreCase)))
                {
                    if (objType == resultType)
                    {
                        return obj;
                    }
                    else
                    {
                        object o = null;
                        try { o = Convert.ChangeType(obj, resultType); }
                        catch { }
                        return o;
                    }
                }

                //Interface
                if (objType.IsClass && resultType.IsInterface)
                {
                    return this.IsExsitType(objType.GetInterfaces(), resultType) ? obj : null;
                }

                //Class
                if (objType.IsClass && !resultType.IsInterface && !resultType.IsAbstract)
                {
                    var toObject = this.GetToObject(new TypeKey() { FromType = objType, ToType = resultType });
                    if (toObject != null)
                    {
                        object o = null;
                        try { o = toObject.To(obj); }
                        catch { }

                        return o;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// map to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public T To<T>(object obj)
        {
            T result = default(T);
            if (obj != null)
            {
                object o = this.To(obj, typeof(T));
                if (o != null) result = (T)o;
            }

            return result;
        }
    }
}

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
        private ReadWriteLock m_rwLock;
        private Dictionary<TypeKey, IToObject> m_toObjectDic;

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
            this.m_rwLock = new ReadWriteLock();
            this.m_toObjectDic = new Dictionary<TypeKey, IToObject>();
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

        private List<MemberModel> GetMembers(Type t)
        {
            List<MemberModel> list = new List<MemberModel>();
            Type ot = typeof(Object);
            while (t != null && t != ot)
            {
                var ps = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.DeclaredOnly);
                foreach (var p in ps)
                {
                    if (!list.Exists(q=>q.member.Name == p.Name))
                    {
                        list.Add(new MemberModel { type = p.PropertyType, member = p });
                    }
                }

                var fs = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var f in fs)
                {
                    if (!list.Exists(q => q.member.Name == f.Name))
                    {
                        list.Add(new MemberModel { type = f.FieldType, member = f });
                    }
                }

                t = t.BaseType;
            }

            return list;
        }

        private Type CreateMapType(TypeKey key)
        {
            Type toObjectType = null;
            if (key != null && key.FromType != null && key.ToType != null
                && (key.FromType.IsClass || key.FromType.IsValueType && !key.ToType.IsPrimitive)
                && (key.ToType.IsClass && key.ToType.GetConstructor(Type.EmptyTypes) != null
                || key.ToType.IsValueType && !key.ToType.IsPrimitive))
            {
                var toObjectBaseType = typeof(ToObject);
                var typeBuilder = this.m_moduleBuilder.DefineType(this.GetDynamicName(key), toObjectBaseType.Attributes, toObjectBaseType, new Type[] { typeof(IToObject) });

                var objectFactoryFieldBuilder = typeBuilder.DefineField("m_objectFactory", typeof(MapFactory), FieldAttributes.Private);

#region ctor
                var baseCtor = toObjectBaseType.GetConstructor(new Type[] { typeof(Type) });
                ConstructorBuilder ctor = typeBuilder.DefineConstructor(baseCtor.Attributes, baseCtor.CallingConvention, new Type[] { typeof(Type), typeof(MapFactory) });
                ILGenerator ctorIL = ctor.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Ldarg_1);
                ctorIL.Emit(OpCodes.Call, baseCtor);
                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Ldarg_2);
                ctorIL.Emit(OpCodes.Stfld, objectFactoryFieldBuilder);
                ctorIL.Emit(OpCodes.Ret);
#endregion

                List<MemberModel> objMemberList = this.GetMembers(key.FromType);
                List<MemberModel> toMemberList = key.FromType != key.ToType ? this.GetMembers(key.ToType)
                    : new List<MemberModel>(objMemberList);

#region To(objec obj)
                MethodAttributes methattr = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig;
                MethodBuilder toMethodBuilder = typeBuilder.DefineMethod("To", methattr, CallingConventions.Standard, typeof(Object), new Type[] { typeof(Object) });

                ILGenerator il = toMethodBuilder.GetILGenerator();
                var resultLocal = il.DeclareLocal(typeof(Object));
                var nLocal = il.DeclareLocal(typeof(int));
                var oLocal = il.DeclareLocal(typeof(Object));
                var objLocal = il.DeclareLocal(key.FromType);
                var toLocal = il.DeclareLocal(key.ToType);

                var endLocal = il.DefineLabel();

                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, resultLocal);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, typeof(ToObject).GetMethod("IsTo", BindingFlags.Instance | BindingFlags.NonPublic));
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brtrue, endLocal);

                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, nLocal);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, oLocal);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, key.FromType);
                il.Emit(OpCodes.Stloc, objLocal);
                if (key.ToType.IsClass)
                {
                    il.Emit(OpCodes.Newobj, key.ToType.GetConstructor(Type.EmptyTypes));
                    il.Emit(OpCodes.Stloc, toLocal);
                }
                else
                {
                    il.Emit(OpCodes.Ldloca, toLocal);
                    il.Emit(OpCodes.Initobj, key.ToType);
                }

                foreach (var toMember in toMemberList)
                {
                    var objMember = objMemberList.Find(q => q.member.Name == toMember.member.Name);
                    if (objMember == null) objMember = objMemberList.Find(q => q.member.Name.ToLower() == toMember.member.Name.ToLower());
                    if (objMember != null)
                    {
                        objMemberList.Remove(objMember);
                        if (objMember.type == toMember.type && objMember.type.IsValueType)
                        {
                            il.Emit(OpCodes.Ldloc, toLocal);
                            il.Emit(OpCodes.Ldloc, objLocal);

                            if (objMember.member.MemberType == MemberTypes.Property)
                                il.Emit(OpCodes.Callvirt, (objMember.member as PropertyInfo).GetGetMethod());
                            else
                                il.Emit(OpCodes.Ldfld, (objMember.member as FieldInfo));

                            if (toMember.member.MemberType == MemberTypes.Property)
                                il.Emit(OpCodes.Callvirt, (toMember.member as PropertyInfo).GetSetMethod());
                            else
                                il.Emit(OpCodes.Stfld, (toMember.member as FieldInfo));
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Stloc, nLocal);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, objectFactoryFieldBuilder);
                            il.Emit(OpCodes.Ldloc, objLocal);
                            if (objMember.member.MemberType == MemberTypes.Property)
                                il.Emit(OpCodes.Callvirt, (objMember.member as PropertyInfo).GetGetMethod());
                            else
                                il.Emit(OpCodes.Ldfld, (objMember.member as FieldInfo));
                            if (objMember.type.IsValueType) il.Emit(OpCodes.Box, objMember.type);
                            il.Emit(OpCodes.Ldtoken, toMember.type);
                            il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                            il.Emit(OpCodes.Callvirt, typeof(MapFactory).GetMethod("To", new Type[] { typeof(Object), typeof(Type) }));
                            il.Emit(OpCodes.Stloc, oLocal);
                            il.Emit(OpCodes.Ldloc, oLocal);
                            il.Emit(OpCodes.Ldnull);
                            il.Emit(OpCodes.Ceq);
                            var isnulll = il.DefineLabel();
                            il.Emit(OpCodes.Brtrue, isnulll);
                            il.Emit(OpCodes.Ldloc, toLocal);
                            il.Emit(OpCodes.Ldloc, oLocal);
                            il.Emit(OpCodes.Castclass, toMember.type);
                            if (toMember.member.MemberType == MemberTypes.Property)
                                il.Emit(OpCodes.Callvirt, (toMember.member as PropertyInfo).GetSetMethod());
                            else
                                il.Emit(OpCodes.Stfld, (toMember.member as FieldInfo));
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Stloc, nLocal);

                            il.MarkLabel(isnulll);
                            il.Emit(OpCodes.Nop);
                        }

                    }
                }

                il.Emit(OpCodes.Ldloc, nLocal);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Cgt);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brtrue, endLocal);
                il.Emit(OpCodes.Ldloc, toLocal);
                if (key.ToType.IsValueType)
                {
                    il.Emit(OpCodes.Box, key.ToType);
                }
                il.Emit(OpCodes.Stloc, resultLocal);

                il.MarkLabel(endLocal);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(ToObject).GetMethod("Clear", BindingFlags.Instance | BindingFlags.NonPublic));
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
                using (this.m_rwLock.GetReadLock())
                {
                    this.m_toObjectDic.TryGetValue(key, out toObject);
                }
                if (toObject != null) return toObject;

                using (this.m_rwLock.GetWriteLock())
                {
                    this.m_toObjectDic.TryGetValue(key, out toObject);
                    if (toObject != null) return toObject;

                    try
                    {
                        var type = this.CreateMapType(key);
                        if (type == null) return toObject;

                        object obj = Activator.CreateInstance(type, new object[] { key.FromType, this });
                        toObject = obj as IToObject;
                        this.m_toObjectDic[key] = toObject;
                    }
                    catch { }
                }
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

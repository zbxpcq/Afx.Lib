using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Collections;
#if !NET20
using System.Collections.Concurrent;
#endif

using Afx.Data.Extensions;

namespace Afx.Data
{
    /// <summary>
    /// 数据库访问扩展方法
    /// </summary>
    public static class DatabaseExtension
    {
#if NET20
        private static Afx.Collections.SafeDictionary<Type, Type> cacheDic = new Afx.Collections.SafeDictionary<Type, Type>();
        private static Afx.Collections.SafeDictionary<Type, Type> setParamDic = new Afx.Collections.SafeDictionary<Type, Type>();
#else
        private static ConcurrentDictionary<Type, Type> cacheDic = new ConcurrentDictionary<Type, Type>();
        private static ConcurrentDictionary<Type, Type> setParamDic = new ConcurrentDictionary<Type, Type>();
#endif
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="T">返回实体class</typeparam>
        /// <param name="db">IDatabase</param>
        /// <param name="sql">sql 语句，参数用@前缀，自动识别数据库类型</param>
        /// <param name="obj">参数：class or Dictionary&lt;string, object&gt;，class属性 or key必须与参数名称一致</param>
        /// <returns>实体 list</returns>
        public static List<T> Query<T>(this IDatabase db, string sql, object obj = null)
        {
            var list = new List<T>();
            var t = typeof(T);
            if (t.IsArray || t.IsAbstract)
            {
                return list;
            }

            IReaderToModel ic = null;
            bool isvalue = false;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var gt = t.GetGenericArguments()[0];
                if (gt.IsPrimitive && gt.IsValueType) isvalue = true;
            }

            if (!isvalue && (t == typeof(string) || t.IsPrimitive && t.IsValueType))
            {
                isvalue = true;
            }
            if (!isvalue && t.IsClass)
            {
                ic = GetReaderToModel(t);
            }

            db.ClearParameters();
            AddParam(db, sql, obj);
            using (var reader = db.ExecuteReader())
            {
                while (reader.Read())
                {
                    object o = default(T);
                    if (isvalue)
                    {
                        var v = reader.GetValue(0);
                        o = ChangeType(v, t) ?? default(T);
                    }
                    else if (ic != null)
                    {
                        o = ic.To(reader);
                    }
                    list.Add((T)o);
                }
            }

            return list;
        }

        private static object ChangeType(object o, Type t)
        {
            if (o != null && o != DBNull.Value)
            {
                if (o.GetType() == t)
                {
                    return o;
                }
                else
                {
                    var gt = t;
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var _gt = t.GetGenericArguments()[0];
                        if (gt.IsPrimitive && gt.IsValueType) gt = _gt;
                    }

                    if (t == typeof(string))
                    {
                        return o.ToString();
                    }
                    else if (gt.IsPrimitive && gt.IsValueType)
                    {
                        return Convert.ChangeType(o, gt);
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// 执行并返回查询结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="db">IDatabase</param>
        /// <param name="sql">sql 语句，参数用@前缀，自动识别数据库类型</param>
        /// <param name="obj">参数：class or Dictionary&lt;string, object&gt;，class属性 or key必须与参数名称一致</param>
        /// <returns>数据</returns>
        public static T ExecuteScalar<T>(this IDatabase db, string sql, object obj = null)
        {
            T m = default(T);
            var rt = typeof(T);
            Type t = rt;
            if (obj != null)
            {
                var ot = obj.GetType();
                if (IsArray(ot))
                {
                    IEnumerable ie = obj as IEnumerable;
                    var tor = ie.GetEnumerator();
                    IList il = null;
                    if (rt.IsGenericType)
                    {
                        m = (T)Activator.CreateInstance(rt);
                        il = m as IList;
                        t = rt.GetGenericArguments()[0];
                        if (t == typeof(Nullable<>))
                        {
                            t = t.GetGenericArguments()[0];
                        }
                    }
                    while (tor.MoveNext())
                    {
                        AddParam(db, sql, tor.Current);
                        object o = db.ExecuteScalar();
                        object v = ChangeType(o, t);
                        if (il != null)
                        {
                            il.Add(v);
                        }
                        else
                        {
                            m = (T)v;
                        }
                    }
                }
                else
                {
                    AddParam(db, sql, obj);
                    object o = db.ExecuteScalar();
                    if (rt.IsGenericType && typeof(Nullable<>) == rt.GetGenericTypeDefinition())
                    {
                        t = rt.GetGenericArguments()[0];
                    }
                    object v = ChangeType(o, t);
                    if (v != null) m = (T)v;
                }
            }
            else
            {
                db.ClearParameters();
                db.CommandText = sql;
                object o = db.ExecuteScalar();
                if (rt.IsGenericType && typeof(Nullable<>) == rt.GetGenericTypeDefinition())
                {
                    t = rt.GetGenericArguments()[0];
                }
                object v = ChangeType(o, t);
                if (v != null) m = (T)v;
            }

            return m;
        }

        private static bool IsArray(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) || t.IsArray;
        }

        /// <summary>
        /// 对连接对象执行 SQL 语句。
        /// </summary>
        /// <param name="db">IDatabase</param>
        /// <param name="sql">sql 语句，参数用@前缀，自动识别数据库类型</param>
        /// <param name="obj">参数：class or Dictionary&lt;string, object&gt;，class属性 or key必须与参数名称一致</param>
        /// <returns>受影响的行数。</returns>
        public static int ExecuteNonQuery(this IDatabase db, string sql, object obj = null)
        {
            int count = 0;
            if (obj != null)
            {
                var ot = obj.GetType();
                if (IsArray(ot))
                {
                    IEnumerable ie = obj as IEnumerable;
                    var tor = ie.GetEnumerator();
                    while (tor.MoveNext())
                    {
                        count += ExecuteObj(db, sql, tor.Current);
                    }
                }
                else
                {
                    count = ExecuteObj(db, sql, obj);
                }
            }
            else
            {
                db.ClearParameters();
                db.CommandText = sql;
                count = db.ExecuteNonQuery();
            }


            return count;
        }

        private static int ExecuteObj(IDatabase db, string sql, object obj)
        {
            int count = 0;
            AddParam(db, sql, obj);
            count = db.ExecuteNonQuery();

            return count;
        }

        private static void AddParam(IDatabase db, string sql, object obj)
        {
            string _sql = sql;
            db.ClearParameters();
            if (obj != null)
            {
                var t = obj.GetType();
                if (t == typeof(Dictionary<string, object>))
                {
                    var dic = obj as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> kv in dic)
                    {
                        string s = kv.Key.TrimStart('@');
                        string paramname = "@" + s;
                        if (_sql.Contains(paramname))
                        {
                            var pname = db.EncodeParameterName(s);
                            if (paramname != pname)
                            {
                                _sql = _sql.Replace(paramname, pname);
                            }
                            var v = kv.Value;
                            db.AddParameter(pname, v);
                        }
                    }
                }
                else if (t.IsClass && !t.Name.StartsWith("<>"))
                {
                    IModelToParam toparam = GetModelToParam(t);
                    _sql = toparam.To(db, _sql, obj);
                }
                else
                {
                    IModelToParam toparam = new ModelToParam();
                    _sql = toparam.To(db, _sql, obj);
                }
            }

            db.CommandText = _sql;
        }

        private static ModuleBuilder moduleBuilder;
        static AssemblyBuilder assbuilder;
        private static Dictionary<Type, MethodInfo> methDic = new Dictionary<Type, MethodInfo>();
        static DatabaseExtension()
        {
            methDic.Add(typeof(Guid), typeof(IDataRecord).GetMethod("GetGuid", new Type[] { typeof(int) }));
            methDic.Add(typeof(Guid?), typeof(IDataRecord).GetMethod("GetGuid", new Type[] { typeof(int) }));

            methDic.Add(typeof(string), typeof(IDataRecord).GetMethod("GetString", new Type[] { typeof(int) }));

            methDic.Add(typeof(DateTime), typeof(IDataRecord).GetMethod("GetDateTime", new Type[] { typeof(int) }));
            methDic.Add(typeof(DateTime?), typeof(IDataRecord).GetMethod("GetDateTime", new Type[] { typeof(int) }));
            
            methDic.Add(typeof(bool), typeof(IDataRecord).GetMethod("GetBoolean", new Type[] { typeof(int) }));
            methDic.Add(typeof(bool?), typeof(IDataRecord).GetMethod("GetBoolean", new Type[] { typeof(int) }));

            methDic.Add(typeof(byte), typeof(IDataRecord).GetMethod("GetByte", new Type[] { typeof(int) }));
            methDic.Add(typeof(byte?), typeof(IDataRecord).GetMethod("GetByte", new Type[] { typeof(int) }));

            methDic.Add(typeof(char), typeof(IDataRecord).GetMethod("GetChar", new Type[] { typeof(int) }));
            methDic.Add(typeof(char?), typeof(IDataRecord).GetMethod("GetChar", new Type[] { typeof(int) }));

            methDic.Add(typeof(decimal), typeof(IDataRecord).GetMethod("GetDecimal", new Type[] { typeof(int) }));
            methDic.Add(typeof(decimal?), typeof(IDataRecord).GetMethod("GetDecimal", new Type[] { typeof(int) }));

            methDic.Add(typeof(double), typeof(IDataRecord).GetMethod("GetDouble", new Type[] { typeof(int) }));
            methDic.Add(typeof(double?), typeof(IDataRecord).GetMethod("GetDouble", new Type[] { typeof(int) }));
             
            methDic.Add(typeof(float), typeof(IDataRecord).GetMethod("GetFloat", new Type[] { typeof(int) }));
            methDic.Add(typeof(float?), typeof(IDataRecord).GetMethod("GetFloat", new Type[] { typeof(int) }));
            
            methDic.Add(typeof(Int16), typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) }));
            methDic.Add(typeof(Int16?), typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) }));

            methDic.Add(typeof(UInt16), typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) }));
            methDic.Add(typeof(UInt16?), typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) }));
            
            methDic.Add(typeof(Int32), typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) }));
            methDic.Add(typeof(Int32?), typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) }));

            methDic.Add(typeof(UInt32), typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) }));
            methDic.Add(typeof(UInt32?), typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) }));
            
            methDic.Add(typeof(Int64), typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) }));
            methDic.Add(typeof(Int64?), typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) }));

            methDic.Add(typeof(UInt64), typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) }));
            methDic.Add(typeof(UInt64?), typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) }));

        }

        private static ModuleBuilder GetModuleBuilder()
        {
            if (moduleBuilder == null)
            {
                AssemblyName assname = new AssemblyName("Afx.Data.Extensions.Dynamic");
                assname.Version = new Version(1, 0, 0, 0);
#if NETCOREAPP || NETSTANDARD
                assbuilder = AssemblyBuilder.DefineDynamicAssembly(assname, AssemblyBuilderAccess.Run);
#else
                assbuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assname, AssemblyBuilderAccess.Run);
#endif
                moduleBuilder = assbuilder.DefineDynamicModule("Afx.Data.Extensions.Dynamic");
            }

            return moduleBuilder;
        }

        private static IReaderToModel GetReaderToModel(Type t)
        {
            IReaderToModel ic = null;
            Type convertType = null;
            if (!cacheDic.TryGetValue(t, out convertType))
            {
                if (convertType == null)
                    {
                        convertType = CreateReaderToModelType(t);
                        if (convertType != null && !cacheDic.ContainsKey(t))
                        {
                            cacheDic[t] = convertType;
                        }
                    }
            }

            if (convertType != null)
            {
                ic = Activator.CreateInstance(convertType) as IReaderToModel;
            }

            return ic;
        }

        private static Type CreateReaderToModelType(Type t)
        {
            Type convertType = null;
            var _moduleBuilder = GetModuleBuilder();

            TypeBuilder typeBuilder = _moduleBuilder.DefineType("ReaderToModel_" + Guid.NewGuid().ToString("n"), TypeAttributes.Class | TypeAttributes.Public, typeof(ReaderToModel), new Type[] { typeof(IReaderToModel) });

            var ctor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            var toMethod = typeBuilder.DefineMethod("To", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.Standard, typeof(object), new Type[] { typeof(IDataReader) });

            var il = toMethod.GetILGenerator();
            var localModel = il.DeclareLocal(t);
            var locali = il.DeclareLocal(typeof(int));
            var localbool = il.DeclareLocal(typeof(bool));

            il.Emit(OpCodes.Newobj, t.GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc, localModel);

            var parr = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in parr)
            {
                var setmethod = p.GetSetMethod();
                if (setmethod != null)
                {
                    var p_endlb = il.DefineLabel();
                    var p_lb1 = il.DefineLabel();
                    var p_lb2 = il.DefineLabel();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, p.Name);
                    il.Emit(OpCodes.Call, typeof(ReaderToModel).GetMethod("GetOrdinal", new Type[] { typeof(IDataReader), typeof(string) }));
                    il.Emit(OpCodes.Stloc, locali);
                    il.Emit(OpCodes.Ldloc, locali);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.Emit(OpCodes.Ble, p_lb1);

                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldloc, locali);
                    il.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) }));
                    il.Emit(OpCodes.Br, p_lb2);

                    il.MarkLabel(p_lb1);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.MarkLabel(p_lb2);
                    il.Emit(OpCodes.Stloc, localbool);
                    il.Emit(OpCodes.Ldloc, localbool);
                    il.Emit(OpCodes.Brtrue, p_endlb);

                    Type gt = null;
                    if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        gt = p.PropertyType.GetGenericArguments()[0];
                    }

                    MethodInfo cal = null;
                    if (p.PropertyType == typeof(byte[]))
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, typeof(ReaderToModel).GetMethod("GetBytes", new Type[] { typeof(IDataReader), typeof(int) }));
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }
                    else if (p.PropertyType == typeof(char[]))
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, typeof(ReaderToModel).GetMethod("GetChars", new Type[] { typeof(IDataReader), typeof(int) }));
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }
                    else if (methDic.TryGetValue(p.PropertyType, out cal))
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, cal);
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }
                    else if (p.PropertyType.IsEnum || gt != null && gt.IsEnum)
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, methDic[typeof(int)]);
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }
                    else if (!p.PropertyType.IsGenericType && p.PropertyType.IsValueType
                        || gt != null && !gt.IsGenericType && gt.IsValueType)
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) }));
                        il.Emit(OpCodes.Unbox, p.PropertyType);
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }

                    il.MarkLabel(p_endlb);
                }
            }

            il.Emit(OpCodes.Ldloc, localModel);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(toMethod, typeof(IReaderToModel).GetMethod("To", new Type[] { typeof(IDataReader) }));
#if NETSTANDARD
            convertType = typeBuilder.CreateTypeInfo();
#else
            convertType = typeBuilder.CreateType();
#endif

            return convertType;
        }

        private static IModelToParam GetModelToParam(Type t)
        {
            IModelToParam setParam = null;
            Type setParamType = null;
            if (!setParamDic.TryGetValue(t, out setParamType))
            {
                    if (setParamType == null)
                    {
                        setParamType = CreateModelToParamType(t);
                        if (setParamType != null && !setParamDic.ContainsKey(t))
                        {
                            setParamDic[t] = setParamType;
                        }
                }
            }

            if (setParamType != null)
            {
                setParam = Activator.CreateInstance(setParamType) as IModelToParam;
            }

            return setParam;
        }

        private static Type CreateModelToParamType(Type t)
        {
            Type setParamType = null;
            var _moduleBuilder = GetModuleBuilder();

            TypeBuilder typeBuilder = _moduleBuilder.DefineType("ModelToParam_" + Guid.NewGuid().ToString("n"), TypeAttributes.Class | TypeAttributes.Public, typeof(ModelToParam), new Type[]{ typeof(IModelToParam) });

            var ctor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            var toMethod = typeBuilder.DefineMethod("To", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.Standard | CallingConventions.HasThis, typeof(string), new Type[] { typeof(IDatabase), typeof(string), typeof(object) });

            var il = toMethod.GetILGenerator();
            var localModel = il.DeclareLocal(t);
            var localSql = il.DeclareLocal(typeof(string));
            var localStr = il.DeclareLocal(typeof(string));
            

            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Castclass, t);
            il.Emit(OpCodes.Stloc, localModel);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stloc, localSql);

            var parr = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in parr)
            {
                var getmethod = p.GetGetMethod();
                if (getmethod != null)
                {
                    var p_lb1 = il.DefineLabel();
                    var p_lb2 = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, localSql);
                    il.Emit(OpCodes.Ldstr, "@" + p.Name);
                    il.Emit(OpCodes.Callvirt, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }));
                    il.Emit(OpCodes.Brfalse, p_lb1);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, p.Name);
                    il.Emit(OpCodes.Callvirt, typeof(IDatabase).GetMethod("EncodeParameterName", new Type[] { typeof(string) }));
                    il.Emit(OpCodes.Stloc, localStr);
                    il.Emit(OpCodes.Ldloc, localStr);
                    il.Emit(OpCodes.Ldstr, "@" + p.Name);
                    il.Emit(OpCodes.Callvirt, typeof(string).GetMethod("Equals", new Type[] { typeof(string) }));
                    il.Emit(OpCodes.Brtrue, p_lb2);
                    il.Emit(OpCodes.Ldloc, localSql);
                    il.Emit(OpCodes.Ldstr, "@" + p.Name);
                    il.Emit(OpCodes.Ldloc, localStr);
                    il.Emit(OpCodes.Callvirt, typeof(string).GetMethod("Replace", new Type[] { typeof(string), typeof(string) }));
                    il.Emit(OpCodes.Stloc, localSql);
                    il.MarkLabel(p_lb2);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldloc, localStr);
                    il.Emit(OpCodes.Ldloc, localModel);
                    il.Emit(OpCodes.Callvirt, getmethod);
                    if (p.PropertyType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, p.PropertyType);
                    }
                    il.Emit(OpCodes.Call, typeof(IDatabase).GetMethod("AddParameter", new Type[] { typeof(string), typeof(object) }));
                    il.MarkLabel(p_lb1);
                }
            }

            il.Emit(OpCodes.Ldloc, localSql);
            il.Emit(OpCodes.Ret);

            var ito = typeof(IModelToParam).GetMethod("To", new Type[] { typeof(IDatabase), typeof(string), typeof(object) });
            typeBuilder.DefineMethodOverride(toMethod, ito);

#if NETSTANDARD
            setParamType = typeBuilder.CreateTypeInfo();
#else
            setParamType = typeBuilder.CreateType();
#endif
            return setParamType;
        }
    }
}

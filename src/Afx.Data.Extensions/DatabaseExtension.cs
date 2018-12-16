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
#else
        private static ConcurrentDictionary<Type, Type> cacheDic = new ConcurrentDictionary<Type, Type>();
#endif
        static object setlockparamobj = new object();

        private static void Check(this IDatabase db, string sql)
        {
            if (db == null) throw new ArgumentNullException("db");
            if (db.IsDisposed) throw new ObjectDisposedException("db");
            if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql", "sql is null!");
        }

        private static bool CheckModel(Type t)
        {
            bool result = false;
            if (t.IsArray || t.IsAbstract || !(t.IsClass || t.IsValueType))
                throw new ArgumentException("T is error!");
            if (t.IsGenericType)
            {
                var gt = t.GetGenericTypeDefinition();
                if (gt != typeof(Nullable<>))
                {
                    throw new ArgumentException("T is error!");
                }

                var tt = t.GetGenericArguments()[0];
                if (!convertDic.ContainsKey(tt))
                {
                    throw new ArgumentException("T is error!");
                }
                result = true;
            }

            return result;
        }

        public static List<T> Query<T>(this IDatabase db, string sql, object parameters = null, CommandType? commandType = null)
        {
            db.Check(sql);
            var t = typeof(T);
            bool isvalue = CheckModel(t);
            var list = new List<T>();

            ReaderToModel ic = null;
            if (!isvalue && (t == typeof(string) || t.IsPrimitive && t.IsValueType))
            {
                isvalue = true;
            }
            if (!isvalue && t.IsClass)
            {
                ic = GetReaderToModel(t);
            }

            AddParam(db, sql, parameters, commandType);
            using (var reader = db.ExecuteReader())
            {
                while (reader.Read())
                {
                    object o = default(T);
                    if (ic != null)
                    {
                        o = ic.To(reader);
                    }
                    else
                    {
                        var v = reader.GetValue(0);
                        o = ChangeType(v, t) ?? default(T);
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
                if (o.GetType() == t || t == typeof(object))
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
                    else
                    {
                        throw new InvalidCastException("ChangeType " + o.GetType().FullName + " to " + t.FullName + " error!");
                    }
                }
            }

            return null;
        }

        public static T ExecuteScalar<T>(this IDatabase db, string sql, object parameters = null, CommandType? commandType = null)
        {
            db.Check(sql);
            T m = default(T);
            var t = typeof(T);
            if (t.IsGenericType && t == typeof(Nullable<>)) t = t.GetGenericArguments()[0];
            else throw new ArgumentException("T is error!");
            if (!convertDic.ContainsKey(t)) throw new ArgumentException("T is error!");

            AddParam(db, sql, parameters, commandType);
            object o = db.ExecuteScalar();
            object v = ChangeType(o, t);
            if (v != null) m = (T)v;

            return m;
        }

        public static int ExecuteNonQuery(this IDatabase db, string sql, object parameters = null, CommandType? commandType = null)
        {
            db.Check(sql);
            AddParam(db, sql, parameters, commandType);
            int count = db.ExecuteNonQuery();

            return count;
        }

        private static void AddParam(IDatabase db, string sql, object parameters, CommandType? commandType)
        {
            db.ClearParameters();
            db.CommandType = commandType;
            string s = sql;
            if (parameters != null)
            {
                if (parameters is IEnumerable<KeyValuePair<string, object>>)
                {
                    IModelToParam toparam = new DicToParam();
                    s = toparam.To(db, sql, parameters);
                }
                else
                {
                    IModelToParam toparam = new ModelToParam();
                    s = toparam.To(db, sql, parameters);
                }
            }
            db.CommandText = s;
        }

        private static ModuleBuilder moduleBuilder;
        static AssemblyBuilder assbuilder;
        private static Dictionary<Type, MethodInfo> convertDic = new Dictionary<Type, MethodInfo>();
        static DatabaseExtension()
        {
            var t = typeof(Convert);
            var types = new Type[] { typeof(object) };
            convertDic.Add(typeof(bool), t.GetMethod("ToBoolean", types));
            convertDic.Add(typeof(byte), t.GetMethod("ToByte", types));
            convertDic.Add(typeof(char), t.GetMethod("ToChar", types));
            convertDic.Add(typeof(DateTime), t.GetMethod("ToDateTime", types));
            convertDic.Add(typeof(decimal), t.GetMethod("ToDecimal", types));
            convertDic.Add(typeof(double), t.GetMethod("ToDouble", types));
            convertDic.Add(typeof(short), t.GetMethod("ToInt16", types));
            convertDic.Add(typeof(int), t.GetMethod("ToInt32", types));
            convertDic.Add(typeof(long), t.GetMethod("ToInt64", types));
            convertDic.Add(typeof(sbyte), t.GetMethod("ToSByte", types));
            convertDic.Add(typeof(float), t.GetMethod("ToSingle", types));
            convertDic.Add(typeof(ushort), t.GetMethod("ToUInt16", types));
            convertDic.Add(typeof(uint), t.GetMethod("ToUInt32", types));
            convertDic.Add(typeof(ulong), t.GetMethod("ToUInt64", types));

            convertDic.Add(typeof(string), t.GetMethod("ToString", types));

            t = typeof(DatabaseExtension);
            convertDic.Add(typeof(DateTimeOffset), t.GetMethod("ToDateTimeOffset", types));
            convertDic.Add(typeof(Guid), t.GetMethod("ToGuid", types));
            convertDic.Add(typeof(TimeSpan), t.GetMethod("ToTimeSpan", types));

        }

        public static DateTimeOffset ToDateTimeOffset(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (obj is DateTimeOffset) return (DateTimeOffset)obj;
            return DateTimeOffset.Parse(obj.ToString());
        }

        public static Guid ToGuid(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (obj is Guid) return (Guid)obj;
            return Guid.Parse(obj.ToString());
        }

        public static TimeSpan ToTimeSpan(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (obj is TimeSpan) return (TimeSpan)obj;
            return TimeSpan.Parse(obj.ToString());
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
                //assbuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assname, AssemblyBuilderAccess.RunAndSave);
#endif
                moduleBuilder = assbuilder.DefineDynamicModule("Afx.Data.Extensions.Dynamic");
                //moduleBuilder = assbuilder.DefineDynamicModule("Afx.Data.Extensions.Dynamic", "Afx.Data.Extensions.Dynamic.dll");
            }

            return moduleBuilder;
        }

        private static ReaderToModel GetReaderToModel(Type t)
        {
            ReaderToModel ic = null;
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
                ic = Activator.CreateInstance(convertType) as ReaderToModel;
            }

            //assbuilder.Save("Afx.Data.Extensions.Dynamic.dll");

            return ic;
        }

        private static int identity = 0;
        private static string GetClassName()
        {
            var i = System.Threading.Interlocked.Increment(ref identity);
            return string.Format("Afx.Data.Extensions.Dynamic.ReaderToModel{0}", i);
        }

        private static Type CreateReaderToModelType(Type t)
        {
            Type convertType = null;
            var _moduleBuilder = GetModuleBuilder();

            TypeBuilder typeBuilder = _moduleBuilder.DefineType(GetClassName(), TypeAttributes.Class | TypeAttributes.Public, typeof(ReaderToModel), new Type[] { typeof(IReaderToModel) });

            MethodAttributes methodAttr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
            CallingConventions calling = CallingConventions.Standard | CallingConventions.HasThis;
            var toMethod = typeBuilder.DefineMethod("To", methodAttr, calling, typeof(object), new Type[] { typeof(IDataReader) });
            var parameterInfos = typeof(ReaderToModel).GetMethod("To", new Type[] { typeof(IDataReader) }).GetParameters();
            var parameterBuilder = toMethod.DefineParameter(1, parameterInfos[0].Attributes, parameterInfos[0].Name);
            var il = toMethod.GetILGenerator();
            var localModel = il.DeclareLocal(t);
            var locali = il.DeclareLocal(typeof(int));
            var localbool = il.DeclareLocal(typeof(bool));

            il.Emit(OpCodes.Newobj, t.GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc, localModel);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(ReaderToModel).GetMethod("SetOrdinal", new Type[] { typeof(IDataReader) }));
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

                    Type pt = p.PropertyType;
                    bool isNullable = false;
                    if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        pt = p.PropertyType.GetGenericArguments()[0];
                        isNullable = true;
                    }

                    MethodInfo call = null;
                    if (pt == typeof(byte[]))
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, typeof(ReaderToModel).GetMethod("GetBytes", new Type[] { typeof(IDataReader), typeof(int) }));
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }
                    else if (pt == typeof(char[]))
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, typeof(ReaderToModel).GetMethod("GetChars", new Type[] { typeof(IDataReader), typeof(int) }));
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }
                    else if (pt.IsEnum)
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldtoken, pt);
                        il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali); 
                        il.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) }));
                        il.Emit(OpCodes.Call, typeof(Enum).GetMethod("ToObject", new Type[] { typeof(Type), typeof(object) }));
                        il.Emit(OpCodes.Unbox_Any, pt);
                        if (isNullable)
                        {
                            il.Emit(OpCodes.Newobj, typeof(Nullable<>).MakeGenericType(new Type[] { pt }).GetConstructor(new Type[] { pt }));
                        }
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }
                    else if (convertDic.TryGetValue(pt, out call))
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) }));
                        il.Emit(OpCodes.Call, call);
                        if (isNullable)
                        {
                            il.Emit(OpCodes.Newobj, typeof(Nullable<>).MakeGenericType(new Type[] { pt }).GetConstructor(new Type[] { pt })); 
                        }
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }
                    else if (pt.IsValueType)
                    {
                        il.Emit(OpCodes.Ldloc, localModel);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloc, locali);
                        il.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) }));
                        il.Emit(OpCodes.Unbox, p.PropertyType);
                        if (isNullable)
                        {
                            il.Emit(OpCodes.Newobj, typeof(Nullable<>).MakeGenericType(new Type[] { pt }).GetConstructor(new Type[] { pt }));
                        }
                        il.Emit(OpCodes.Callvirt, setmethod);
                    }

                    il.MarkLabel(p_endlb);
                }
            }

            il.Emit(OpCodes.Ldloc, localModel);
            il.Emit(OpCodes.Ret);
#if NETSTANDARD
            convertType = typeBuilder.CreateTypeInfo();
#else
            convertType = typeBuilder.CreateType();
#endif

            return convertType;
        }

    }
}

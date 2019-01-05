﻿using System;
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
                throw new ArgumentException(t.FullName + " is error!");
            if (t.IsGenericType)
            {
                var gt = t.GetGenericTypeDefinition();
                if (gt != typeof(Nullable<>))
                {
                    throw new ArgumentException(t.FullName + " is error!");
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
                var ot = o.GetType();
                if (ot == t || t == typeof(object))
                {
                    return o;
                }
                else if (t == typeof(string))
                {
                    return o.ToString();
                }
                else
                {
                    MethodInfo methodInfo;
                    if (convertDic.TryGetValue(t, out methodInfo))
                    {
                        return methodInfo.Invoke(null, new object[] { o });
                    }

                    var gt = t;
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var _gt = t.GetGenericArguments()[0];
                        if (_gt == ot) return o;
                        if (gt.IsPrimitive && gt.IsValueType) gt = _gt;
                    }

                    if (gt.IsPrimitive && gt.IsValueType)
                    {
                        return Convert.ChangeType(o, gt);
                    }
                    else
                    {
                        throw new InvalidCastException("ChangeType " + ot.FullName + " to " + t.FullName + " error!");
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
            if (t.IsGenericType)
            {
                if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    t = t.GetGenericArguments()[0];
                else throw new ArgumentException(t.FullName + " is error!");
            }

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
        internal static Dictionary<Type, DbType> dbTypeDic = new Dictionary<Type, DbType>();
        static DatabaseExtension()
        {
            var t = typeof(Convert);
            var types = new Type[] { typeof(object) };

            AddConvert(Convert.ToBoolean);
            AddConvert(Convert.ToByte);
            AddConvert(Convert.ToChar);
            AddConvert(Convert.ToDateTime);
            AddConvert(Convert.ToDecimal);
            AddConvert(Convert.ToDouble);
            AddConvert(Convert.ToInt16);
            AddConvert(Convert.ToInt32);
            AddConvert(Convert.ToInt64);
            AddConvert(Convert.ToSByte);
            AddConvert(Convert.ToSingle);
            AddConvert(Convert.ToUInt16);
            AddConvert(Convert.ToUInt32);
            AddConvert(Convert.ToUInt64);

            AddConvert(Convert.ToString);

            AddConvert(ConvertToDateTimeOffset);
            AddConvert(ConvertToGuid);
            AddConvert(ConvertToTimeSpan);

            dbTypeDic = new Dictionary<Type, DbType>
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = DbType.Time,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = DbType.DateTime,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = DbType.Time
            };
        }

        public static void AddDbType<T>(DbType dbType)
        {
            dbTypeDic[typeof(T)] = dbType;
        }

        public static void AddConvert<T>(Func<object, T> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            convertDic[typeof(T)] = func.Method;
        }

        public static DateTimeOffset ConvertToDateTimeOffset(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (obj is DateTimeOffset) return (DateTimeOffset)obj;
            return DateTimeOffset.Parse(obj.ToString());
        }

        public static Guid ConvertToGuid(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (obj is Guid) return (Guid)obj;
            return Guid.Parse(obj.ToString());
        }

        public static TimeSpan ConvertToTimeSpan(object obj)
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
            bool isThrow = false;
            PropertyInfo throwPropertyInfo = null;
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
                    else
                    {
                        isThrow = true;
                        if (throwPropertyInfo == null) throwPropertyInfo = p;
                        //il.Emit(OpCodes.Ldloc, localModel);
                        //il.Emit(OpCodes.Ldarg_1);
                        //il.Emit(OpCodes.Ldloc, locali);
                        //il.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) }));
                        //il.Emit(OpCodes.Unbox, p.PropertyType);
                        //if (isNullable)
                        //{
                        //    il.Emit(OpCodes.Newobj, typeof(Nullable<>).MakeGenericType(new Type[] { pt }).GetConstructor(new Type[] { pt }));
                        //}
                        //il.Emit(OpCodes.Callvirt, setmethod);
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

            if (isThrow) throw new InvalidCastException(t.FullName + "未找到属性" + throwPropertyInfo.Name + "的数据类型转换方法，请调用Afx.Data.DatabaseExtension.AddConvert添加数据类型转换方法。");

            return convertType;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Afx.Threading;

namespace Afx.Aop
{
    /// <summary>
    /// AopUtils
    /// </summary>
    public static class AopUtils
    {
        private static Dictionary<int, List<Type>> m_globalAopDic = new Dictionary<int, List<Type>>();

        private static Dictionary<int, Dictionary<Type, List<Type>>> m_typeAopDic = new Dictionary<int, Dictionary<Type, List<Type>>>();

        internal static List<Type> GetOfGlobal(int classId)
        {
            List<Type> list = null;
            List<Type> temp = null;
            if (m_globalAopDic.TryGetValue(classId, out temp))
            {
                list = new List<Type>(temp);
            }

            return list ?? new List<Type>(0);
        }
        
        internal static void AddOfGlobal(int classId, List<Type> aopTypeList)
        {
            if (aopTypeList != null && aopTypeList.Count > 0)
            {
                var aopType = typeof(IAop);
                List<Type> list = aopTypeList.FindAll(q => aopType.IsAssignableFrom(q) && q.IsClass && !q.IsAbstract);
                if (list.Count > 0)
                {
                        List<Type> temp = null;
                        if (m_globalAopDic.TryGetValue(classId, out temp))
                        {
                            temp.AddRange(list);
                        }
                        else
                        {
                            temp = list;
                        }
                        m_globalAopDic[classId] = temp;
                    }
            }
        }

        internal static List<Type> GetOfType(int classId, Type t)
        {
            List<Type> list = null;
            if (t != null)
            {
                Dictionary<Type, List<Type>> dic = null;
                if(m_typeAopDic.TryGetValue(classId, out dic))
                {
                    dic.TryGetValue(t, out list);
                }
            }

            return list ?? new List<Type>(0);
        }

        private static bool IsMethod(MethodInfo[] arr, string name)
        {
            foreach (var meth in arr)
            {
                if (meth.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        internal static void AddOfType(int classId, Type t, List<Type> list)
        {
            if (t != null && list != null && list.Count > 0)
            {
                List<Type> temp = null;

                Dictionary<Type, List<Type>> tdic = null;
                if (!m_typeAopDic.TryGetValue(classId, out tdic))
                {
                    tdic = new Dictionary<Type, List<Type>>();
                    m_typeAopDic[classId] = tdic;
                }

                if (!tdic.TryGetValue(t, out temp))
                {
                    temp = new List<Type>();
                    tdic[t] = temp;
                }

                var aopType = typeof(IAop);
                foreach (var at in list)
                {
                    if (aopType.IsAssignableFrom(at) && !temp.Exists(q => q == at)) temp.Add(at);
                }
            }
        }
        /// <summary>
        /// OnExecuting
        /// </summary>
        /// <param name="filterInfo"></param>
        /// <param name="context"></param>
        public static void OnExecuting(AopInfoModel filterInfo, AopContext context)
        {
            if (filterInfo.GlobalList == null)
            {
                List<Type> tlist = GetOfGlobal(filterInfo.ClassId);
                filterInfo.GlobalList = new List<IAop>(tlist.Count);
                foreach (var t in tlist) filterInfo.GlobalList.Add(Activator.CreateInstance(t) as IAop);
            }
            foreach (var f in filterInfo.GlobalList)
            {
                f.OnExecuting(context);
            }

            if (filterInfo.TypeList == null)
            {
                var talist = GetOfType(filterInfo.ClassId, context.TagetType);
                filterInfo.TypeList = new List<IAop>(talist.Count);
                foreach (var t in talist) filterInfo.TypeList.Add(Activator.CreateInstance(t) as IAop);
            }
            foreach (var f in filterInfo.TypeList)
            {
                f.OnExecuting(context);
            }

            if (filterInfo.TypeAttributes == null)
            {
                var arr = context.TagetType.GetCustomAttributes(typeof(AopAttribute), true);
                filterInfo.TypeAttributes = new List<AopAttribute>(arr.Length);
                foreach (var o in arr) filterInfo.TypeAttributes.Add(o as AopAttribute);
            }
            foreach (var f in filterInfo.TypeAttributes)
            {
                f.OnExecuting(context);
            }
            
            if (filterInfo.MethodAttributes == null)
            {
                var arr = context.Method.GetCustomAttributes(typeof(AopAttribute), true);
                filterInfo.MethodAttributes = new List<AopAttribute>(arr.Length);
                foreach (var o in arr) filterInfo.MethodAttributes.Add(o as AopAttribute);
            }
            foreach (var f in filterInfo.MethodAttributes)
            {
                f.OnExecuting(context);
            }
        }
        /// <summary>
        /// OnResult
        /// </summary>
        /// <param name="filterInfo"></param>
        /// <param name="context"></param>
        /// <param name="returnValue"></param>
        public static void OnResult(AopInfoModel filterInfo, AopContext context, object returnValue)
        {
            foreach (var f in filterInfo.GlobalList)
            {
                f.OnResult(context, returnValue);
            }

            foreach (var f in filterInfo.TypeList)
            {
                f.OnResult(context, returnValue);
            }

            foreach (var f in filterInfo.TypeAttributes)
            {
                f.OnResult(context, returnValue);
            }

            foreach (var f in filterInfo.MethodAttributes)
            {
                f.OnResult(context, returnValue);
            }
        }
        /// <summary>
        /// OnException
        /// </summary>
        /// <param name="filterInfo"></param>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        public static void OnException(AopInfoModel filterInfo, AopContext context, Exception ex)
        {
            foreach (var f in filterInfo.GlobalList)
            {
                f.OnException(context, ex);
            }

            foreach (var f in filterInfo.TypeList)
            {
                f.OnException(context, ex);
            }

            foreach (var f in filterInfo.TypeAttributes)
            {
                f.OnException(context, ex);
            }

            foreach (var f in filterInfo.MethodAttributes)
            {
                f.OnException(context, ex);
            }
        }
        /// <summary>
        /// GetParameterType
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static Type[] GetParameterType(MethodBase methodInfo)
        {
            Type[] arr = null;
            if (methodInfo != null)
            {
                var param = methodInfo.GetParameters();
                arr = new Type[param.Length];
                for (int i = 0; i < param.Length; i++)
                    arr[i] = param[i].ParameterType;
            }

            return arr;
        }
    }
}

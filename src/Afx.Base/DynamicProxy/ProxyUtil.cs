using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.DynamicProxy
{
    /// <summary>
    /// ProxyUtil
    /// </summary>
    public static class ProxyUtil
    {
        internal const string MODULE_NAME = "Afx.DynamicProxy";

        /// <summary>
        /// SetTargetTypeMethod
        /// </summary>
        public static readonly MethodInfo SetTargetTypeMethod = typeof(IProxy).GetMethod("SetTargetType");

        /// <summary>
        /// GetTargetTypeMethod
        /// </summary>
        public static readonly MethodInfo GetTargetTypeMethod = typeof(IProxy).GetMethod("GetTargetType");

        /// <summary>
        /// SetAopFuncMethod
        /// </summary>
        public static readonly MethodInfo SetAopFuncMethod = typeof(IProxy).GetMethod("SetAopFunc");

        /// <summary>
        /// GetAopFuncMethod
        /// </summary>
        public static readonly MethodInfo GetAopFuncMethod = typeof(IProxy).GetMethod("GetAopFunc");

        /// <summary>
        /// OnExecutingMethod
        /// </summary>
        public static readonly MethodInfo OnExecutingMethod = typeof(ProxyUtil).GetMethod("OnExecuting", BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// OnResultMethod
        /// </summary>
        public static readonly MethodInfo OnResultMethod = typeof(ProxyUtil).GetMethod("OnResult", BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// OnExceptionMethod
        /// </summary>
        public static readonly MethodInfo OnExceptionMethod = typeof(ProxyUtil).GetMethod("OnException", BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// GetParameterTypeMethod
        /// </summary>
        public static readonly MethodInfo GetParameterTypeMethod = typeof(ProxyUtil).GetMethod("GetParameterType", BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// OnExecuting
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="context"></param>
        public static void OnExecuting(IProxy proxy, MethodInfo method, object[] args, AopContext context)
        {
            context.Proxy = proxy;
            context.TargetType = proxy.GetTargetType();
            context.Method = method;
            context.Arguments = args;
            var aopfuncs = proxy.GetAopFunc();
            context.Aops = new List<IAop>(aopfuncs.Length);
            for (int i = 0; i < aopfuncs.Length; i++)
            {
                try
                {
                    var aop = aopfuncs[i]();
                    if (aop != null) context.Aops.Add(aop);
                } catch { }
            }
            context.Aops.TrimExcess();
            foreach (var aop in context.Aops)
            {
                if (aop != null)
                {
                    aop.OnExecuting(context);
                }
            }
        }

        /// <summary>
        /// OnResult
        /// </summary>
        /// <param name="context"></param>
        /// <param name="returnValue"></param>
        public static void OnResult(AopContext context, object returnValue)
        {
            foreach (var aop in context.Aops)
            {
                if (aop != null)
                {
                    aop.OnResult(context, returnValue);
                    if (aop is IDisposable) ((IDisposable)aop).Dispose();
                }
            }
        }

        /// <summary>
        /// OnException
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        public static void OnException(AopContext context, Exception ex)
        {
            foreach (var aop in context.Aops)
            {
                if (aop != null)
                {
                    aop.OnException(context, ex);
                    if (aop is IDisposable) ((IDisposable)aop).Dispose();
                }
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

        /// <summary>
        /// IsProxy
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsProxy(object obj)
        {
            if(obj != null)
            {
                if (obj is Type) return IsProxy(obj as Type);

                return IsProxy(obj.GetType());
            }

            return false;
        }

        /// <summary>
        /// IsProxy
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsProxy(Type t)
        {
            return typeof(IProxy).IsAssignableFrom(t) && !string.IsNullOrEmpty(t.Module.Name) && t.Module.Name.StartsWith(MODULE_NAME);
        }
    }
}

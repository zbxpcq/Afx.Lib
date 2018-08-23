using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.DynamicProxy
{
    public static class ProxyUtil
    {
        internal const string MODULE_NAME = "Afx.DynamicProxy";

        public static readonly MethodInfo SetTargetTypeMethod = typeof(IProxy).GetMethod("SetTargetType");

        public static readonly MethodInfo GetTargetTypeMethod = typeof(IProxy).GetMethod("GetTargetType");

        public static readonly MethodInfo SetAopFuncMethod = typeof(IProxy).GetMethod("SetAopFunc");

        public static readonly MethodInfo GetAopFuncMethod = typeof(IProxy).GetMethod("GetAopFunc");


        public static readonly MethodInfo OnExecutingMethod = typeof(ProxyUtil).GetMethod("OnExecuting", BindingFlags.Static | BindingFlags.Public);

        public static readonly MethodInfo OnResultMethod = typeof(ProxyUtil).GetMethod("OnResult", BindingFlags.Static | BindingFlags.Public);

        public static readonly MethodInfo OnExceptionMethod = typeof(ProxyUtil).GetMethod("OnException", BindingFlags.Static | BindingFlags.Public);

        public static readonly MethodInfo GetParameterTypeMethod = typeof(ProxyUtil).GetMethod("GetParameterType", BindingFlags.Static | BindingFlags.Public);

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

        public static bool IsProxy(object obj)
        {
            return obj != null && IsProxy(obj.GetType());
        }

        public static bool IsProxy(Type t)
        {
            return typeof(IProxy).IsAssignableFrom(t) && !string.IsNullOrEmpty(t.Module.Name) && t.Module.Name.StartsWith(MODULE_NAME);
        }
    }
}

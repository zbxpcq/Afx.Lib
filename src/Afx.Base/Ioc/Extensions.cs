using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Ioc
{
    internal static class Extensions
    {
        public static bool EqualsGeneric(Type serviceType, Type targetType)
        {
            if (serviceType.IsGenericType && serviceType.IsGenericTypeDefinition
                && targetType.IsGenericType && targetType.IsGenericTypeDefinition)
            {
                if (serviceType.IsInterface)
                {
                    var arr = targetType.GetInterfaces();
                    foreach (var t in arr)
                    {
                        if (t.IsGenericType && !t.IsGenericTypeDefinition)
                        {
                            var tt = t.GetGenericTypeDefinition();
                            if (serviceType == tt) return true;
                        }
                    }
                }
                else
                {
                    var baseType = targetType.BaseType;
                    while (baseType != typeof(object))
                    {
                        if (baseType.IsGenericType && !baseType.IsGenericTypeDefinition)
                        {
                            var tt = baseType.GetGenericTypeDefinition();
                            if (tt == serviceType) return true;
                        }
                        baseType = baseType.BaseType;
                    }
                }
            }

            return false;
        }

        public static void Check(Type serviceType, Type targetType)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (targetType.IsAbstract) throw new ArgumentException(targetType.FullName + " is abstract!", "targetType");
            if (targetType == typeof(object)) throw new ArgumentException(targetType.FullName + " is error!", "targetType");
            if (serviceType == targetType) return;
            if (serviceType.IsAssignableFrom(targetType)) return;

            if (EqualsGeneric(serviceType, targetType))
            {
                return;
            }
            else if (serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition
                && targetType.IsGenericType && !targetType.IsGenericTypeDefinition)
            {
                var sg = serviceType.GetGenericTypeDefinition();
                var tg = targetType.GetGenericTypeDefinition();
                if (EqualsGeneric(serviceType, targetType))
                {
                    var sp = serviceType.GetGenericArguments();
                    var tp = targetType.GetGenericArguments();
                    if (sp.Length == tp.Length)
                    {
                        bool ok = true;
                        for (int i = 0; i < sp.Length; i++)
                        {
                            if (sp[i] != tp[i])
                            {
                                ok = false;
                                break;
                            }
                        }
                        if (ok) return;
                    }
                }
            }


            throw new ArgumentException(targetType.FullName + " is error!", "targetType");
        }
        
    }
}

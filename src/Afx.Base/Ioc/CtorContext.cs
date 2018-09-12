using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Ioc
{
    /// <summary>
    /// 构造器信息
    /// </summary>
    public class CtorContext
    {
        /// <summary>
        /// 构造器
        /// </summary>
        public ConstructorInfo Ctor { get; private set; }

        /// <summary>
        /// 构造器参数信息
        /// </summary>
        public readonly ParameterInfo[] ParameterInfos;
        /// <summary>
        /// 构造器参数类型
        /// </summary>
        public readonly Type[] ParameterTypes;

        internal CtorContext(ConstructorInfo ctor)
        {
            if (ctor == null) throw new ArgumentNullException("ctor");
            this.Ctor = ctor;
            this.ParameterInfos = this.Ctor.GetParameters();
            this.ParameterTypes = new Type[this.ParameterInfos.Length];
            for (int i = 0; i < this.ParameterInfos.Length; i++)
                this.ParameterTypes[i] = this.ParameterInfos[i].ParameterType;
        }

        /// <summary>
        /// 参数类型是否当前匹配构造器
        /// </summary>
        /// <param name="paramterTypes">参数类型</param>
        /// <returns>bool</returns>
        public bool IsMatch(Type[] paramterTypes)
        {
            bool result = false;
            if ((paramterTypes == null || paramterTypes.Length == 0)
                && this.ParameterTypes.Length == 0)
            {
                result = true;
            }
            else if (paramterTypes != null && paramterTypes.Length == this.ParameterTypes.Length)
            {
                int count = 0;
                for (int i = 0; i < this.ParameterTypes.Length; i++)
                {
                    var ct = this.ParameterTypes[i];
                    var it = paramterTypes[i];
                    if (it == null) break;

                    if (ct == it || ct.IsAssignableFrom(it))
                    {
                        count++;
                    }
                    else if (it.IsValueType && ct.IsValueType && ct.IsGenericType
                         && ct.GetGenericTypeDefinition() == typeof(Nullable<>)
                        && ct.GetGenericArguments()[0] == it)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                result = count == this.ParameterTypes.Length;
            }

            return result;
        }

        /// <summary>
        /// 参数是否当前匹配构造器
        /// </summary>
        /// <param name="args">参数</param>
        /// <returns>bool</returns>
        public bool IsMatch(object[] args)
        {
            bool result = false;
            if ((args == null || args.Length == 0)
                && this.ParameterTypes.Length == 0)
            {
                result = true;
            }
            else if(args != null && args.Length == this.ParameterTypes.Length)
            {
                int count = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    var it = args[i] == null ? null : args[i].GetType();
                    var ct = this.ParameterTypes[i];
                    if (it == null)
                    {
                        if (ct.IsClass || ct.IsValueType && ct.IsGenericType
                            && ct.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (ct == it || ct.IsAssignableFrom(it))
                    {
                        count++;
                    }
                    else if (it.IsValueType && ct.IsValueType && ct.IsGenericType
                         && ct.GetGenericTypeDefinition() == typeof(Nullable<>)
                        && ct.GetGenericArguments()[0] == it)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                result = count == this.ParameterTypes.Length;
            }

            return result;
        }
    }
}

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
            else if (paramterTypes.Length == this.ParameterTypes.Length)
            {
                result = true;
                for (int i = 0; i < this.ParameterTypes.Length; i++)
                {
                    var ct = this.ParameterTypes[i];
                    var it = paramterTypes[i];
                    if (ct != it && !ct.IsAssignableFrom(it))
                    {
                        if (it.IsValueType && ct.IsValueType && ct.IsGenericType
                            && ct.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var gt = ct.GetGenericArguments()[0];
                            if (gt != it)
                            {
                                result = false;
                                break;
                            }
                        }
                    }
                }
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
            Type[] paramterTypes = new Type[args == null ? 0 : args.Length];
            for (int i = 0; i < paramterTypes.Length; i++)
                paramterTypes[i] = args[i].GetType();

            return this.IsMatch(paramterTypes);
        }
    }
}

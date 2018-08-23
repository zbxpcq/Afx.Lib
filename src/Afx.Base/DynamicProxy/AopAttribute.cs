using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Afx.DynamicProxy
{
    /// <summary>
    /// 公共方法aop Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AopAttribute : Attribute
    {
        /// <summary>
        /// IAop Type
        /// </summary>
        public Type AopType { get; private set; }

        /// <summary>
        /// AopAttribute
        /// </summary>
        /// <param name="type">IAop Type</param>
        public AopAttribute(Type type)
        {
            if (type != null && typeof(IAop).IsAssignableFrom(type))
            {
                this.AopType = type;
            }
        }
    }
}

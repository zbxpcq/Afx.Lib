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
        public Type AopType { get; private set; }

        public AopAttribute(Type type)
        {
            if (type != null && typeof(IAop).IsAssignableFrom(type))
            {
                this.AopType = type;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Afx.Aop
{
    /// <summary>
    /// 公共方法aop Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class AopAttribute : Attribute, IAop
    {
        /// <summary>
        /// 方法执行前
        /// </summary>
        /// <param name="context">Aop上下文</param>
        public virtual void OnExecuting(AopContext context)
        {
            
        }

        /// <summary>
        /// 方法执行后
        /// </summary>
        /// <param name="context">Aop上下文</param>
        /// <param name="returnValue">返回对象</param>
        public virtual void OnResult(AopContext context, object returnValue)
        {
            
        }

        /// <summary>
        /// 方法异常
        /// </summary>
        /// <param name="context">Aop上下文</param>
        /// <param name="ex">异常信息</param>
        public virtual void OnException(AopContext context, Exception ex)
        {
            
        }
    }
}

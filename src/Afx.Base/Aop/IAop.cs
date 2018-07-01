using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Aop
{
    /// <summary>
    /// Aop 接口
    /// </summary>
    public interface IAop
    {
        /// <summary>
        /// 方法执行前
        /// </summary>
        /// <param name="context">Aop上下文</param>
        void OnExecuting(AopContext context);

        /// <summary>
        /// 方法执行后
        /// </summary>
        /// <param name="context">Aop上下文</param>
        /// <param name="returnValue">返回对象</param>
        void OnResult(AopContext context, object returnValue);

        /// <summary>
        /// 方法异常
        /// </summary>
        /// <param name="context">Aop上下文</param>
        /// <param name="ex">异常信息</param>
        void OnException(AopContext context, Exception ex);
    }
}

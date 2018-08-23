using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Ioc
{
    /// <summary>
    /// 创建 TService Func 信息
    /// </summary>
    public class FuncContext
    {
        /// <summary>
        /// TService Func Method
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// TService Func Target
        /// </summary>
        public object Target { get; private set; }

        internal FuncContext(object target, MethodInfo method)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (method == null) throw new ArgumentNullException("method");
            this.Target = target;
            this.Method = method;
        }

        /// <summary>
        /// 创建 TService
        /// </summary>
        /// <param name="container">IContainer</param>
        /// <returns>TService</returns>
        public object Invoke(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            return this.Method.Invoke(this.Target, new object[] { container });
        }
    }
}

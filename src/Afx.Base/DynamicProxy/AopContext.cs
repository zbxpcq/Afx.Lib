using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.DynamicProxy
{
    /// <summary>
    /// Aop上下文
    /// </summary>
    public sealed class AopContext
    {
        /// <summary>
        /// TargetType
        /// </summary>
        public Type TargetType { get; internal set; }
        /// <summary>
        /// Method
        /// </summary>
        public MethodInfo Method { get; internal set; }
        /// <summary>
        /// Proxy
        /// </summary>
        public object Proxy { get; internal set; }
        /// <summary>
        /// Arguments
        /// </summary>
        public object[] Arguments { get; internal set; }
        /// <summary>
        /// UserState
        /// </summary>
        public object UserState { get; set; }

        internal List<IAop> Aops;
    }
}

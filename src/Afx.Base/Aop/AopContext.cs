using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Aop
{
    /// <summary>
    /// Aop上下文
    /// </summary>
    public sealed class AopContext
    {
        /// <summary>
        /// TargetType
        /// </summary>
        public Type TargetType { get; set; }
        /// <summary>
        /// Method
        /// </summary>
        public MethodInfo Method { get; set; }
        /// <summary>
        /// Proxy
        /// </summary>
        public object Proxy { get; set; }
        /// <summary>
        /// Arguments
        /// </summary>
        public object[] Arguments { get; set; }
        /// <summary>
        /// UserState
        /// </summary>
        public object UserState { get; set; }
    }
}

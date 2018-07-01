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
        /// TagetType
        /// </summary>
        public Type TagetType { get; set; }
        /// <summary>
        /// Method
        /// </summary>
        public MethodInfo Method { get; set; }
        /// <summary>
        /// Taget
        /// </summary>
        public object Taget { get; set; }
        /// <summary>
        /// Parameters
        /// </summary>
        public object[] Parameters { get; set; }
        /// <summary>
        /// UserState
        /// </summary>
        public object UserState { get; set; }
    }
}

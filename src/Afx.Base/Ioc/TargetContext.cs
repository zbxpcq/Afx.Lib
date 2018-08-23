using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Ioc
{
    /// <summary>
    /// TService Target Info
    /// </summary>
    public class TargetContext
    {
        /// <summary>
        /// TService Target Type
        /// </summary>
        public Type TargetType { get; private set; }

        private readonly CtorContext[] ctors;

        internal TargetContext(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            var arr = targetType.GetConstructors();
            if (arr.Length == 0) throw new ArgumentException(targetType.FullName + " no public constructors!", "targetType");
            this.TargetType = targetType;
            this.ctors = new CtorContext[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                this.ctors[i] = new CtorContext(arr[i]);
        }

        /// <summary>
        /// 获取TService Target构造器
        /// </summary>
        /// <returns>CtorContext list</returns>
        public List<CtorContext> GetCtors()
        {
            return new List<CtorContext>(this.ctors);
        }
    }
}

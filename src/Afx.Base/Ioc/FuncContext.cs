using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Ioc
{
    public class FuncContext
    {
        public MethodInfo Method { get; private set; }

        public object Target { get; private set; }

        internal FuncContext(object target, MethodInfo method)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (method == null) throw new ArgumentNullException("method");
            this.Target = target;
            this.Method = method;
        }

        public object Invoke(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            return this.Method.Invoke(this.Target, new object[] { container });
        }
    }
}

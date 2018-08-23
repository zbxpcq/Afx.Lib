using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.DynamicProxy
{
    public interface IProxy
    {
        void SetTargetType(Type type);

        Type GetTargetType();

        void SetAopFunc(Func<IAop>[] funcs);

        Func<IAop>[] GetAopFunc();
    }
}

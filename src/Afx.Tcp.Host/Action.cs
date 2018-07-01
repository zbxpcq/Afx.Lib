using System;
using System.Collections.Generic;
using System.Text;

namespace  Afx.Tcp.Host
{
#if NET20
    /// <summary>
    /// Action
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
#endif
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Tcp.Protocols
{
#if NET20
    /// <summary>
    /// Func
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns>TResult</returns>
    public delegate TResult Func<in T, out TResult>(T arg);
#endif
}

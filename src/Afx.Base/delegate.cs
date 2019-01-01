using System;
using System.Collections.Generic;
using System.Text;

namespace Afx
{
#if NET20
    public delegate TResult Func<TResult>();

    public delegate TResult Func<T, TResult>(T obj);

    public delegate TResult Func<T1, T2, TResult>(T1 t1, T2 t2);

    public delegate TResult Func<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 t3);
#endif
}

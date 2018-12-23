using System;
using System.Collections.Generic;
using System.Text;

namespace Afx
{
#if NET20
    public delegate TResult Func<TResult>();

    public delegate TResult Func<T, TResult>(T obj);

#endif
}

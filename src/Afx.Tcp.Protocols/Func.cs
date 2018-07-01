using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Tcp.Protocols
{
#if NET20
    public delegate TResult Func<in T, out TResult>(T arg);
#endif
}

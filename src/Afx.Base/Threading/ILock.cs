using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Threading
{
    public interface ILock : IDisposable
    {
        void Unlock();
    }
}

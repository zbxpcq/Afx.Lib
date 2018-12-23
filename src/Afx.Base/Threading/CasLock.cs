using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Afx.Threading
{
    /// <summary>
    /// CAS 算法锁
    /// add by jerrylai@aliyun.com
    /// </summary>
    public class CasLock
    {
        class Disposable : ILock
        {
            private CasLock cas;
            public Disposable(CasLock cas)
            {
                this.cas = cas;
            }

            public void Unlock()
            {
                if (this.cas != null)
                {
                    var old = Interlocked.Exchange(ref this.cas, null);
                    if (old != null)
                    {
                        old.Unlock();
                    }
                }
            }

            public void Dispose()
            {
                this.Unlock();
            }
        }

        private volatile int value = 0;

        public ILock Lock(int millisecondsTimeout, out bool success)
        {
            success = false;
            if (millisecondsTimeout > 0)
            {
                DateTime lastTime = DateTime.Now;
                while (!(success = Interlocked.CompareExchange(ref this.value, 1, 0) == 0))
                {
                    Thread.SpinWait(5);
                    var ts = DateTime.Now - lastTime;
                    if (ts.TotalMilliseconds > millisecondsTimeout) break;
                }
            }
            else
            {
                while (!(success = Interlocked.CompareExchange(ref this.value, 1, 0) == 0))
                {
                    Thread.SpinWait(5);
                }
            }

            return success ? new Disposable(this) : new Disposable(null);
        }

        public ILock Lock()
        {
            bool success = false;
            return this.Lock(0, out success);
        }

        private void Unlock()
        {
            Interlocked.CompareExchange(ref this.value, 0, 1);
        }
    }
}

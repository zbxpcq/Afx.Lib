using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Afx.Threading
{
    /// <summary>
    /// 写锁
    /// </summary>
    public sealed class WriteLock : IDisposable
    {
        [ThreadStatic]
        private static WriteLock current = null;
        /// <summary>
        /// Current
        /// </summary>
        public static WriteLock Current { get { return current; } }

        private ReaderWriterLock rwLock;

        internal WriteLock(ReaderWriterLock rwLock)
        {
            this.rwLock = rwLock;
            if (this.rwLock != null)
            {
                current = this;
            }
        }

        internal static void ClearCurrent()
        {
            if (current != null)
            {
                current.Clear();
            }
        }

        internal void Clear()
        {
            this.rwLock = null;
            current = null;
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        public void ReleaseLock()
        {
            current = null;
            if (this.rwLock != null && this.rwLock.IsWriterLockHeld)
            {
                this.rwLock.ReleaseWriterLock();
                this.rwLock = null;
            }
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.ReleaseLock();
        }
    }
}

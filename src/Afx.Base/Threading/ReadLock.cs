using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Afx.Threading
{
    /// <summary>
    /// 只读锁对象
    /// </summary>
    public sealed class ReadLock : IDisposable
    {
        [ThreadStatic]
        private static ReadLock current = null;
        /// <summary>
        /// Current
        /// </summary>
        public static ReadLock Current { get { return current; } }

        private ReaderWriterLock rwLock;

        internal ReadLock(ReaderWriterLock rwLock)
        {
            this.rwLock = rwLock;
            if (this.rwLock != null)
            {
                current = this;
            }
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        public void ReleaseLock()
        {
            if (this.rwLock != null && this.rwLock.IsReaderLockHeld)
            {
                current = null;
                this.rwLock.ReleaseReaderLock();
                this.rwLock = null;
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
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.ReleaseLock();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Afx.Threading
{
    /// <summary>
    /// 读写琐
    /// </summary>
    public sealed class ReadWriteLock : IDisposable
    {
        private ReaderWriterLock rwLock;
        /// <summary>
        /// ReadWriteLock
        /// </summary>
        public ReadWriteLock()
        {
            this.IsDisposed = false;
            this.rwLock = new ReaderWriterLock();
        }

        /// <summary>
        /// 获取一个值，该值指示当前线程是否持有读线程锁。如果当前线程持有读线程锁，则为 true；否则为 false。
        /// </summary>
        public bool IsReaderLockHeld { get { return this.rwLock.IsReaderLockHeld; } }

        /// <summary>
        /// 获取一个值，该值指示当前线程是否持有写线程锁。如果当前线程持有写线程锁，则为 true；否则为 false。
        /// </summary>
        public bool IsWriterLockHeld { get { return this.rwLock.IsWriterLockHeld; } }

        /// <summary>
        /// 获取当前序列号。
        /// </summary>
        public int WriterSeqNum { get { return this.rwLock.WriterSeqNum; } }
        /// <summary>
        /// IsDisposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.IsDisposed = true;
                this.rwLock = null;
                ReadLock.ClearCurrent();
                WriteLock.ClearCurrent();
            }
        }

        /// <summary>
        /// 尝试进入读取模式锁定状态
        /// </summary>
        /// <returns></returns>
        public ReadLock GetReadLock()
        {
            this.rwLock.AcquireReaderLock(int.MaxValue);
            return new ReadLock(this.rwLock);
        }

        /// <summary>
        /// 获取读锁
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间，超时异常</param>
        /// <returns></returns>
        public ReadLock GetReadLock(int millisecondsTimeout)
        {
            this.rwLock.AcquireReaderLock(millisecondsTimeout);
            return new ReadLock(this.rwLock);
        }

        /// <summary>
        /// 获取读锁
        /// </summary>
        /// <param name="timeout">超时时间，超时异常</param>
        /// <returns></returns>
        public ReadLock GetReadLock(TimeSpan timeout)
        {
            this.rwLock.AcquireReaderLock(timeout);
            return new ReadLock(this.rwLock);
        }

        /// <summary>
        /// 尝试进入写入模式锁定状态
        /// </summary>
        /// <returns></returns>
        public WriteLock GetWriteLock()
        {
            this.rwLock.AcquireWriterLock(int.MaxValue);
            return new WriteLock(this.rwLock);
        }

        /// <summary>
        /// 获取写锁
        /// </summary>
        /// <param name="timeout">超时时间，超时异常</param>
        /// <returns></returns>
        public WriteLock GetWriteLock(TimeSpan timeout)
        {
            this.rwLock.AcquireWriterLock(timeout);
            return new WriteLock(this.rwLock);
        }

        /// <summary>
        /// 获取写锁
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间，超时异常</param>
        /// <returns></returns>
        public WriteLock GetWriteLock(int millisecondsTimeout)
        {
            this.rwLock.AcquireWriterLock(millisecondsTimeout);
            return new WriteLock(this.rwLock);
        }

       /// <summary>
        /// 减少读取模式的递归计数，并在生成的计数为 0（零）时退出读取模式。
       /// </summary>
        public void ReleaseReadLock()
        {
            this.rwLock.ReleaseReaderLock();
            ReadLock.ClearCurrent();
        }


        /// <summary>
        /// 减少写入模式的递归计数，并在生成的计数为 0（零）时退出写入模式。
        /// </summary>
        public void ReleaseWriteLock()
        {
            this.rwLock.ReleaseWriterLock();
            WriteLock.ClearCurrent();
        }
       
    }
}

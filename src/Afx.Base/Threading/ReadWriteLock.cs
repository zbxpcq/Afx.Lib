using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Afx.Threading
{
    public interface ILock: IDisposable
    {
        bool IsLock { get; }

        void Release();
    }

    /// <summary>
    /// 读写琐
    /// </summary>
    public sealed class ReadWriteLock : IDisposable
    {
        class ReadDisposable : ILock
        {
            private ReadWriteLock rw;
            public bool IsLock { get { return this.rw != null; } }
            
            public ReadDisposable(ReadWriteLock rw)
            {
                this.rw = rw;
            }

            public void Release()
            {
                var old = this.rw;
                if(old != null)
                {
                    this.rw = null;
                    old.ReleaseReadLock();
                }
            }

            public void Dispose()
            {
                this.Release();
            }
        }

        class WriteDisposable : ILock
        {
            private ReadWriteLock rw;

            public bool IsLock { get { return this.rw != null; } }

            public WriteDisposable(ReadWriteLock rw)
            {
                this.rw = rw;
            }

            public void Release()
            {
                var old = this.rw;
                if (old != null)
                {
                    this.rw = null;
                    old.ReleaseWriteLock();
                }
            }

            public void Dispose()
            {
                this.Release();
            }
        }

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
        public bool IsReadLockHeld { get { return this.rwLock.IsReaderLockHeld; } }

        /// <summary>
        /// 获取一个值，该值指示当前线程是否持有写线程锁。如果当前线程持有写线程锁，则为 true；否则为 false。
        /// </summary>
        public bool IsWriteLockHeld { get { return this.rwLock.IsWriterLockHeld; } }

        /// <summary>
        /// 获取当前序列号。
        /// </summary>
        public int WriteSeqNum { get { return this.rwLock.WriterSeqNum; } }
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
            }
        }

        /// <summary>
        /// 尝试进入读取模式锁定状态
        /// </summary>
        /// <returns></returns>
        public ILock GetReadLock()
        {
            try
            {
                this.rwLock.AcquireReaderLock(int.MaxValue);
                return new ReadDisposable(this);
            }
            catch { }

            return new ReadDisposable(null);
        }

        /// <summary>
        /// 获取读锁
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间，超时异常</param>
        /// <returns></returns>
        public ILock GetReadLock(int millisecondsTimeout)
        {
            try
            {
                this.rwLock.AcquireReaderLock(millisecondsTimeout);
                return new ReadDisposable(this);
            }
            catch { }

            return new ReadDisposable(null);
        }

        /// <summary>
        /// 获取读锁
        /// </summary>
        /// <param name="timeout">超时时间，超时异常</param>
        /// <returns></returns>
        public ILock GetReadLock(TimeSpan timeout)
        {
            try
            {
                this.rwLock.AcquireReaderLock(timeout);
                return new ReadDisposable(this);
            }
            catch { }

            return new ReadDisposable(null);
        }

        /// <summary>
        /// 尝试进入写入模式锁定状态
        /// </summary>
        /// <returns></returns>
        public ILock GetWriteLock()
        {
            try
            {
                this.rwLock.AcquireWriterLock(int.MaxValue);
                return new WriteDisposable(this);
            }
            catch { }

            return new WriteDisposable(null);
        }

        /// <summary>
        /// 获取写锁
        /// </summary>
        /// <param name="timeout">超时时间，超时异常</param>
        /// <returns></returns>
        public ILock GetWriteLock(TimeSpan timeout)
        {
            try
            {
                this.rwLock.AcquireWriterLock(timeout);
                return new WriteDisposable(this);
            }
            catch { }

            return new WriteDisposable(null);
        }

        /// <summary>
        /// 获取写锁
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间，超时异常</param>
        /// <returns></returns>
        public ILock GetWriteLock(int millisecondsTimeout)
        {
            try
            {
                this.rwLock.AcquireWriterLock(millisecondsTimeout);
                return new WriteDisposable(this);
            }
            catch { }

            return new WriteDisposable(null);
        }

        private void ReleaseReadLock()
        {
            this.rwLock.ReleaseReaderLock();
        }


        private void ReleaseWriteLock()
        {
            this.rwLock.ReleaseWriterLock();
        }
       
    }
}

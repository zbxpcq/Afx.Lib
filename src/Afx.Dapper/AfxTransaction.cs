using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Dapper
{
    public class AfxTransaction : IDisposable
    {
        [ThreadStatic]
        private static AfxTransaction current;
        /// <summary>
        /// 当前事务
        /// </summary>
        public static AfxTransaction Current { get { return current; } }

        private AfxDapper db;
        internal AfxTransaction(AfxDapper db)
        {
            this.db = db;
            current = this;
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
            current = null;
            this.db = null;
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            current = null;
            if (this.db != null && !this.db.IsDisposed)
            {
                this.db.Commit();
                this.db = null;
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            current = null;
            if (this.db != null && !this.db.IsDisposed)
            {
                this.db.Rollback();
                this.db = null;
            }
        }
        /// <summary>
        /// 是否资源，回滚未提交事务
        /// </summary>
        public void Dispose()
        {
            this.Rollback();
        }
    }
}

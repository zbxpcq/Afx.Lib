using System;
using System.Collections.Generic;
using System.Data;
#if NETCOREAPP || NETSTANDARD
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace Afx.Data.Entity
{
    /// <summary>
    /// Entity 事务
    /// </summary>
    public sealed class EntityTransaction : IDisposable
    {
        [ThreadStatic]
        private static EntityTransaction current;
        /// <summary>
        /// 当前已开启事务
        /// </summary>
        public static EntityTransaction Current {
            get { return current; } 
            internal set { current = value; } 
        }

        private EntityContext dbContext;
        internal EntityTransaction(EntityContext dbContext)
        {
            this.dbContext = dbContext;
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
            this.dbContext = null;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            current = null;
            if (dbContext != null && !dbContext.IsDisposed)
            {
                dbContext.Commit();
                this.dbContext = null;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            current = null;
            if (dbContext != null && !dbContext.IsDisposed)
            {
                dbContext.Rollback();
                this.dbContext = null;
            }
        }

        /// <summary>
        /// 释放，并回滚未提交事务
        /// </summary>
        public void Dispose()
        {
            this.Rollback();
        }
    }
}

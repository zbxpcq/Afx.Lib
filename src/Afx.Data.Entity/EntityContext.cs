﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;
#if NETCOREAPP || NETSTANDARD
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#endif

namespace Afx.Data.Entity
{
    /// <summary>
    /// 实体 Context
    /// </summary>
    public class EntityContext : DbContext, IDisposable
    {
#if NETCOREAPP || NETSTANDARD
        /// <summary>
        /// Initializes a new instance of the Microsoft.EntityFrameworkCore.DbContext class
        /// using the specified options. The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public EntityContext(DbContextOptions options)
         : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Microsoft.EntityFrameworkCore.DbContext class
        /// </summary>
        protected EntityContext()
            : base()
        {
            
        }
#else
         /// <summary>
        /// Initializes a new instance of the Microsoft.EntityFramework.DbContext class
        /// </summary>
        /// <param name="nameOrConnectionString">appsettingkey or connectionString</param>
        public EntityContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.RestConfiguration();
        }

        /// <summary>
        /// Initializes a new instance of the Microsoft.EntityFramework.DbContext class
        /// </summary>
        /// <param name="dbConnection">数据库链接，Context 被Dispose， dbConnection 也会被Dispose</param>
        public EntityContext(DbConnection dbConnection)
            : base(dbConnection, true)
        {
            this.RestConfiguration();
        }

         /// <summary>
        /// 重新设置Configuration
        /// this.Configuration.AutoDetectChangesEnabled = true;
        /// this.Configuration.EnsureTransactionsForFunctionsAndCommands = false;
        /// this.Configuration.LazyLoadingEnabled = true;
        /// this.Configuration.ProxyCreationEnabled = true;
        ///  this.Configuration.UseDatabaseNullSemantics = false;
        /// this.Configuration.ValidateOnSaveEnabled = false;
        /// </summary>
        public virtual void RestConfiguration()
        {
            this.Configuration.AutoDetectChangesEnabled = true;
            this.Configuration.EnsureTransactionsForFunctionsAndCommands = false;
            this.Configuration.LazyLoadingEnabled = true;
            this.Configuration.ProxyCreationEnabled = true;
            this.Configuration.UseDatabaseNullSemantics = false;
            this.Configuration.ValidateOnSaveEnabled = false;
        }
#endif

        /// <summary>
        /// 是否开启事务
        /// </summary>
        public virtual bool IsTransaction { get { return this.Database.CurrentTransaction != null; } }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns>Entity 事务</returns>
        public virtual EntityTransaction BeginTransaction()
        {
            if (this.IsTransaction)
            {
                throw new InvalidOperationException("不能重复开启事务！");
            }

            this.saveChangeCount = 0;
            this.Database.BeginTransaction();
            return new EntityTransaction(this);
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="isolationLevel">The System.Data.IsolationLevel to use.</param>
        /// <returns>Entity 事务</returns>
        public virtual EntityTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (this.IsTransaction)
            {
                throw new InvalidOperationException("不能重复开启事务！");
            }

            this.saveChangeCount = 0;
            this.Database.BeginTransaction(isolationLevel);
            return new EntityTransaction(this);
        }

        private int saveChangeCount = 0;
        /// <summary>
        /// 提交事务
        /// </summary>
        public virtual void Commit()
        {
            EntityTransaction.ClearCurrent();
            if (this.Database.CurrentTransaction != null)
            {
                this.Database.CurrentTransaction.Commit();
            }
            this.OnCommitCallback();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public virtual void Rollback()
        {
            EntityTransaction.ClearCurrent();
            if (this.Database.CurrentTransaction != null)
            {
                this.Database.CurrentTransaction.Rollback();
            }
            this.ClearCommitCallback();
        }

        private bool isDisposed = false;
        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool IsDisposed { get { return this.isDisposed; } }

        /// <summary>
        /// 执行原生sql
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteSqlCommand(string sql)
        {
            return this.Database.ExecuteSqlCommand(sql);
        }

        /// <summary>
        /// 执行原生sql
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">sql 参数</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteSqlCommand(string sql, object[] parameters)
        {
            return this.Database.ExecuteSqlCommand(sql, parameters);
        }

        /// <summary>
        /// 执行原生sql
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">sql 参数</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteSqlCommand(string sql, IEnumerable<object> parameters)
        {
            return this.Database.ExecuteSqlCommand(sql, parameters.ToArray());
        }
#if !NET40
        /// <summary>
        /// 执行原生sql
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <returns>The number of rows affected.</returns>
        public virtual Task<int> ExecuteSqlCommandAsync(string sql)
        {
            return this.Database.ExecuteSqlCommandAsync(sql);
        }

        /// <summary>
        /// 执行原生sql
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">sql 参数</param>
        /// <returns>The number of rows affected.</returns>
        public virtual Task<int> ExecuteSqlCommandAsync(string sql, object[] parameters)
        {
            return this.Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        /// <summary>
        /// 执行原生sql
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">sql 参数</param>
        /// <returns>The number of rows affected.</returns>
        public virtual Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters)
        {
            return this.Database.ExecuteSqlCommandAsync(sql, parameters.ToArray());
        }
#endif
        private List<Action<int>> commitCallbackList = new List<Action<int>>();
        /// <summary>
        ///  commit or SaveChanges 成功之后执行action list
        /// </summary>
        public List<Action<int>> CommitCallbackList => this.commitCallbackList;
        /// <summary>
        /// CommitCallback throw Exception Action
        /// </summary>
        public Action<Exception> CommitCallbackError;
        /// <summary>
        /// 添加 commit or SaveChanges 成功之后执行action
        /// action 只执行一次
        /// </summary>
        /// <param name="action">需要执行的action</param>
        /// <returns>添加成功，返回所在的位置</returns>
        public virtual int AddCommitCallback(Action<int> action)
        {
            if (action == null) throw new ArgumentNullException("action");
            int index = this.CommitCallbackList.IndexOf(action);

            if (index < 0)
            {
                this.CommitCallbackList.Add(action);
                index = this.CommitCallbackList.Count - 1;
            }

            return index;
        }
        
        /// <summary>
        /// 移除commit or SaveChanges 成功之后执行action
        /// </summary>
        /// <param name="action">需要执行的action</param>
        /// <returns>移除成功返回true</returns>
        public virtual bool RemoveCommitCallback(Action<int> action)
        {
            bool result = false;
            if (action == null) throw new ArgumentNullException("action");
            result = this.CommitCallbackList.Remove(action);

            return result;
        }

        /// <summary>
        /// 移除所有action
        /// </summary>
        public virtual void ClearCommitCallback()
        {
            this.saveChangeCount = 0;
            this.CommitCallbackList.Clear();
        }

        private void OnCommitCallback()
        {
            foreach (var action in this.CommitCallbackList)
            {
                try { action(this.saveChangeCount); }
                catch (Exception ex)
                {
                    CommitCallbackError?.Invoke(ex);
                }
            }
            this.ClearCommitCallback();
        }

#if NETCOREAPP || NETSTANDARD
                /// <summary>
        /// aves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges  is called after the changes have been sent successfully to the database.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            int count = base.SaveChanges(acceptAllChangesOnSuccess);
            if (!this.IsTransaction)
            {
                this.saveChangeCount = count;
                this.OnCommitCallback();
            }
            else
            {
                this.saveChangeCount += count;
            }

            return count;
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges  is called after the changes have been sent successfully to the database.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            int count = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            if (!this.IsTransaction)
            {
                this.saveChangeCount = count;
                this.OnCommitCallback();
            }
            else
            {
                this.saveChangeCount += count;
            }

            return count;
        }

        /// <summary>
        /// 释放，并回滚未提交事务
        /// </summary>
        public override void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.Rollback();
                base.Dispose();
            }
        }
#else
        /// <summary>
        /// SaveChanges
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            int count = base.SaveChanges();
            if (!this.IsTransaction)
            {
                this.saveChangeCount = count;
                this.OnCommitCallback();
            }
            else
            {
                this.saveChangeCount += count;
            }

            return count;
        }
#if !NET40
        /// <summary>
        /// SaveChangesAsync
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            int count = await base.SaveChangesAsync(cancellationToken);
            if (!this.IsTransaction)
            {
                this.saveChangeCount = count;
                this.OnCommitCallback();
            }
            else
            {
                this.saveChangeCount += count;
            }

            return count;
        }
#endif
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !this.isDisposed)
            {
                this.isDisposed = true;
                this.Rollback();
                base.Dispose(disposing);
            }
        }
#endif
    }
}

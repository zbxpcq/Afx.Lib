using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Afx.Data.Entity.EntityLog
{
    /// <summary>
    /// Entity LoggerProvider
    /// </summary>
    public class EntityLoggerProvider : ILoggerProvider
    {
        private EntityContext context;
        /// <summary>
        /// Entity LoggerProvider
        /// </summary>
        /// <param name="context">EntityContext</param>
        public EntityLoggerProvider(EntityContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            this.context = context;
        }

        /// <summary>
        /// CreateLogger
        /// </summary>
        /// <param name="categoryName">categoryName</param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            if(this.context != null)
            {
                return new EntityLogger(this.context, categoryName);
            }

            return null;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.context = null;
        }
    }
}

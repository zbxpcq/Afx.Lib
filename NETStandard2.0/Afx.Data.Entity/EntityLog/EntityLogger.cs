using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Afx.Data.Entity.EntityLog
{
    /// <summary>
    /// Entity Logger
    /// </summary>
    public class EntityLogger : ILogger, IDisposable
    {
        private EntityContext context;
        private string categoryName;
        /// <summary>
        /// Entity Logger
        /// </summary>
        /// <param name="context">EntityContext</param>
        /// <param name="categoryName">categoryName</param>
        public EntityLogger(EntityContext context, string categoryName)
        {
            if (context == null) throw new ArgumentNullException("context");
            this.context = context;
            this.categoryName = categoryName;
        }

        /// <summary>
        /// return null
        /// </summary>
        /// <typeparam name="TState">TState</typeparam>
        /// <param name="state">state</param>
        /// <returns>null</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.context = null;
        }

        /// <summary>
        /// return true
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns>true</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <typeparam name="TState">TState</typeparam>
        /// <param name="logLevel">logLevel</param>
        /// <param name="eventId">eventId</param>
        /// <param name="state">state</param>
        /// <param name="exception">exception</param>
        /// <param name="formatter">formatter</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(this.context != null && this.context.Log != null)
            {
                string s = formatter(state, exception);
                this.context.Log(s);
            }
        }
    }
}

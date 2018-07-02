using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Afx.Data.Entity.EntityLog
{
    /// <summary>
    /// Entity LoggerFactory
    /// </summary>
    public class EntityLoggerFactory : ILoggerFactory
    {
        private ILoggerProvider provider;

        /// <summary>
        /// Entity LoggerFactory
        /// </summary>
        /// <param name="provider">Entity LoggerProvider</param>
        public void AddProvider(ILoggerProvider provider)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            this.provider = provider;
        }
        /// <summary>
        /// Creates a new Microsoft.Extensions.Logging.ILogger instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>Entity LoggerFactory</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return this.provider.CreateLogger(categoryName);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.provider != null)
            {
                this.provider.Dispose();
                this.provider = null;
            }
        }
    }
}

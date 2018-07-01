using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Afx.Web.Http;

namespace System.Web.Http
{
    /// <summary>
    /// SidExtensions
    /// </summary>
    public static class SidExtensions
    {
        private static SidMessageHandler messageHandler = null;
        /// <summary>
        /// UseSid
        /// </summary>
        /// <param name="config"></param>
        /// <param name="configAction"></param>
        public static void UseSid(this HttpConfiguration config, Action<SidMessageHandler> configAction)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
           if(messageHandler == null)
            {
                messageHandler = new SidMessageHandler();
                if(!config.MessageHandlers.Contains(messageHandler))
                {
                    config.MessageHandlers.Add(messageHandler);
                }
            }

            configAction?.Invoke(messageHandler);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace Afx.Web
{
    public class MyCsvSessionIDManager : ISessionIDManager
    {
        private SessionIDManager manager = null;

        static SessionStateSection _s_config = null;
        static SessionStateSection s_config
        {
            get
            {
                if (_s_config == null)
                {
                    _s_config = (SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");
                }

                return _s_config;
            }
        }

        public MyCsvSessionIDManager()
        {
            this.manager = new SessionIDManager();
        }

        public string CreateSessionID(HttpContext context)
        {
            return this.manager.CreateSessionID(context);
        }

        public string GetSessionID(HttpContext context)
        {
            string id = context.Request.Headers[s_config.CookieName];
            if (!string.IsNullOrEmpty(id))
                return id;

            id = context.Request.QueryString[s_config.CookieName];
            if (!string.IsNullOrEmpty(id))
                return id;

            id = context.Request.Form[s_config.CookieName];
            if (!string.IsNullOrEmpty(id))
                return id;

            id = this.manager.GetSessionID(context);

            return id;
        }

        public void Initialize()
        {
            this.manager.Initialize();
        }

        public bool InitializeRequest(HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue)
        {
            return this.manager.InitializeRequest(context, suppressAutoDetectRedirect, out supportSessionIDReissue);
        }

        public void RemoveSessionID(HttpContext context)
        {
            this.manager.RemoveSessionID(context);
        }

        public void SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded)
        {
            this.manager.SaveSessionID(context, id, out redirected, out cookieAdded);
        }

        public bool Validate(string id)
        {
            return this.manager.Validate(id);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Web
{
    public static class MyCsvSession
    {
        public static void SetAllowRemote(string app = null)
        {
            try
            {
                if (string.IsNullOrEmpty(app) || app.Length > 20) app = "Afx.Web";
                if (!app.StartsWith("/")) app = "/" + app;
                if (!app.EndsWith("/")) app = app + "/";
                Type stateServerSessionProvider = typeof(System.Web.SessionState.HttpSessionState)
                    .Assembly.GetType("System.Web.SessionState.OutOfProcSessionStateStore");
                if (stateServerSessionProvider != null)
                {
                    FieldInfo uriField = stateServerSessionProvider.GetField("s_uribase", BindingFlags.Static | BindingFlags.NonPublic);
                    if (uriField != null)
                        uriField.SetValue(null, app);
                }
            }
            catch { }
        }
    }
}

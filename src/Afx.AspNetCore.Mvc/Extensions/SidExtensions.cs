﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Afx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 添加sid 保存 cookie
    /// </summary>
   public static  class SidExtensions
    {
        private static SidOption option;
        private static SidMiddleware sidMiddleware;
        /// <summary>
        /// 添加 sid 保存 cookie的Middleware
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configAction"></param>
        public static void UseSid(this IApplicationBuilder app, Action<SidOption> configAction)
        {
            if (app == null) throw new ArgumentNullException("app");
            if(option == null)
            {
                option = new SidOption();
                sidMiddleware = new SidMiddleware(option);
                app.Use(sidMiddleware.Invoke);
            }

            configAction?.Invoke(option);
        }
    }
}

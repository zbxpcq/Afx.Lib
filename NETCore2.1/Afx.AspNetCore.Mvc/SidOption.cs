using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Afx.AspNetCore.Mvc
{
    /// <summary>
    /// SidOption
    /// </summary>
    public class SidOption
    {
        /// <summary>
        /// 请求回调
        /// </summary>
        public BeginRequestCallback BeginRequestCallback;

        /// <summary>
        /// 请求结束回调
        /// </summary>
        public EndRequestCallback EndRequestCallback;

        /// <summary>
        /// sid 是否存在 IsQueryString (level 1)
        /// </summary>
        public bool IsQueryString { get; set; } = false;

        /// <summary>
        /// sid 是否存在header  (level 2)
        /// </summary>
        public bool IsHeader { get; set; } = false;

        /// <summary>
        /// sid 是否存在IsCookie  (level 3)
        /// </summary>
        public bool IsCookie { get; set; } = true;

        private string name = "sid";
        /// <summary>
        /// sid name
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("Name");
                name = value;
            }
        }

        private CookieOptions cookieOptions = new CookieOptions() { HttpOnly=true, Path="/" };
        /// <summary>
        /// CookieOptions
        /// </summary>
        public CookieOptions Cookie { get { return this.cookieOptions; } }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Afx.Cache
{
    /// <summary>
    /// 缓存key配置
    /// </summary>
    public class CacheKey
    {
        private XmlDocument xmlDoc;
        private XmlElement rootElement;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="xmlFile">配置文件</param>
        public CacheKey(string xmlFile)
        {
            if (string.IsNullOrEmpty(xmlFile)) throw new ArgumentNullException(xmlFile);
            if (!System.IO.File.Exists(xmlFile)) throw new FileNotFoundException(xmlFile + " not found!", xmlFile);
            xmlDoc = new XmlDocument();
            using (var fs = System.IO.File.Open(xmlFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                xmlDoc.Load(fs);
            }
            rootElement = xmlDoc.DocumentElement;
        }

        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="node">节点</param>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public string GetKey(string node, string name)
        {
            string key = null;
            if (rootElement != null)
            {
                XmlElement nodeElement = rootElement[node];
                if (nodeElement != null)
                {
                    XmlElement keyElement = nodeElement[name];
                    if (keyElement != null)
                    {
                        key = keyElement.GetAttribute("key");
                    }
                }
            }

            return key;
        }

        /// <summary>
        /// 获取过期时间
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public TimeSpan? GetExpire(string node, string name)
        {
            TimeSpan? expire = null;
            if (rootElement != null)
            {
                XmlElement nodeElement = rootElement[node];
                if (nodeElement != null)
                {
                    XmlElement keyElement = nodeElement[name];
                    if (keyElement != null)
                    {
                        string s = keyElement.GetAttribute("expire");
                        if (string.IsNullOrEmpty(s))
                        {
                            s = nodeElement.GetAttribute("expire");
                        }

                        if (!string.IsNullOrEmpty(s))
                        {
                            var arr = s.Split(':').Reverse().ToArray();
                            int seconds = 0, minutes = 0, hours = 0, days = 0;
                            switch (arr.Length)
                            {
                                case 1:
                                    if (int.TryParse(arr[0], out seconds) && seconds > 0)
                                    {
                                        expire = new TimeSpan(0, 0, seconds);
                                    }
                                    break;
                                case 2:
                                    if (int.TryParse(arr[0], out seconds) && seconds >= 0
                                        && int.TryParse(arr[1], out minutes) && minutes >= 0)
                                    {
                                        expire = new TimeSpan(0, minutes, seconds);
                                    }
                                    break;
                                case 3:
                                    if (int.TryParse(arr[0], out seconds) && seconds >= 0
                                        && int.TryParse(arr[1], out minutes) && minutes >= 0
                                        && int.TryParse(arr[2], out hours) && hours >= 0)
                                    {
                                        expire = new TimeSpan(hours, minutes, seconds);
                                    }
                                    break;
                                case 4:
                                    if (int.TryParse(arr[0], out seconds) && seconds >= 0
                                        && int.TryParse(arr[1], out minutes) && minutes >= 0
                                        && int.TryParse(arr[2], out hours) && hours >= 0
                                        && int.TryParse(arr[3], out days) && days >= 0)
                                    {
                                        expire = new TimeSpan(days, hours, minutes, seconds);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            return expire;
        }

        /// <summary>
        /// 获取db
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<int> GetDb(string node, string name)
        {
            List<int> list = new List<int>();
            if (rootElement != null)
            {
                XmlElement nodeElement = rootElement[node];
                if (nodeElement != null)
                {
                    string s = null;
                    XmlElement keyElement = nodeElement[name];
                    if (keyElement != null)
                    {
                        s = keyElement.GetAttribute("db");
                    }

                    if (string.IsNullOrEmpty(s))
                    {
                        s = nodeElement.GetAttribute("db");
                    }

                    if (!string.IsNullOrEmpty(s))
                    {
                        string[] arr = s.Split(',');
                        foreach(var ts in arr)
                        {
                            var ss = ts.Trim();
                            if (!string.IsNullOrEmpty(ss))
                            {
                                if (ss.Contains("-"))
                                {
                                    var ssarr = ss.Split('-');
                                    if(ssarr.Length == 2)
                                    {
                                        var bs = ssarr[0].Trim();
                                        var es = ssarr[1].Trim();
                                        int bv = 0;
                                        int ev = 0;
                                        if(int.TryParse(bs, out bv) && int.TryParse(es, out ev) && bv <= ev)
                                        {
                                            while(bv < ev)
                                            {
                                                list.Add(bv++);
                                            }
                                            list.Add(ev);
                                        }
                                    }
                                }
                                else
                                {
                                    int v = 0;
                                    if (int.TryParse(ss, out v)) list.Add(v);
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }
    }
}

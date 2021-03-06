﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;

namespace Afx.Utils
{
    /// <summary>
    /// EnumUtils
    /// add by jerrylai@aliyun.com
    /// https://github.com/jerrylai/Afx.Lib
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static int GetValue(this Enum @enum)
        {
            return (int)((object)@enum);
        }

        /// <summary>
        /// GetName
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetName(this Enum @enum)
        {
            return @enum.ToString();
        }

        /// <summary>
        /// GetDescription
        /// </summary>
        /// <param name="enum"></param>
        /// <param name="langue"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum @enum, string langue = null, string directory = null)
        {
            var m = GetInfo(@enum, langue, directory);

            return m != null ? m.Description : @enum.GetName();
        }

        class EnumInfoModel
        {
            public string Name { get; private set; }

            public string Description { get; private set; }

            public EnumInfoModel(string name, string description)
            {
                this.Name = name;
                this.Description = description;
            }
        }

#if NET20
        private static Afx.Collections.SafeDictionary<string, Afx.Collections.SafeDictionary<Type, List<EnumInfoModel>>> enumDic = new Afx.Collections.SafeDictionary<string, Afx.Collections.SafeDictionary<Type, List<EnumInfoModel>>>(StringComparer.OrdinalIgnoreCase);
#else
        private static System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<Type, List<EnumInfoModel>>> enumDic = new System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<Type, List<EnumInfoModel>>>(StringComparer.OrdinalIgnoreCase);
#endif

#if NET20
        private static Afx.Collections.SafeDictionary<Type, List<EnumInfoModel>> GetLangueDic(string langue)
        {
            Afx.Collections.SafeDictionary<Type, List<EnumInfoModel>> dic = null;
#else
        private static System.Collections.Concurrent.ConcurrentDictionary<Type, List<EnumInfoModel>> GetLangueDic(string langue)
        {
            System.Collections.Concurrent.ConcurrentDictionary<Type, List<EnumInfoModel>> dic = null;
#endif
            if (!enumDic.TryGetValue(langue, out dic))
            {
                enumDic[langue] = dic = new System.Collections.Concurrent.ConcurrentDictionary<Type, List<EnumInfoModel>>();
            }

            return dic;
        }

        private static List<EnumInfoModel> GetTypeInfo(Type type, string langue, string directory)
        {
            string filename = PathUtils.GetFileFullPath(Path.Combine(directory, string.Format("Langue/Enum/{0}/{1}.xml", langue, type.Name)));
            XmlDocument doc = new XmlDocument();
            if (File.Exists(filename))
            {
                using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    doc.Load(fs);
                }
            }

            var names = type.GetEnumNames();
            List<EnumInfoModel> list = new List<EnumInfoModel>(names.Length);
            var rootElement = doc.DocumentElement;
            foreach (var s in names)
            {
                string description = null;
                if (rootElement != null)
                {
                    var node = rootElement[s];
                    if (node != null)
                    {
                        description = node.GetAttribute("description");
                    }
                }

                if (string.IsNullOrEmpty(description))
                {
                    var att = (DescriptionAttribute)Attribute.GetCustomAttribute(type.GetField(s), typeof(DescriptionAttribute), false);
                    if (att != null) description = att.Description;
                    if (string.IsNullOrEmpty(description)) description = s;
                }

                list.Add(new EnumInfoModel(s, description));
            }

            return list;
        }

        private static EnumInfoModel GetInfo(Enum @enum, string langue, string directory)
        {
            EnumInfoModel m = null;
            if (string.IsNullOrEmpty(langue)) langue = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (string.IsNullOrEmpty(directory)) directory = Directory.GetCurrentDirectory();
            var dic = GetLangueDic(langue);
            if (dic != null)
            {
                var type = @enum.GetType();
                var name = @enum.GetName();
                List<EnumInfoModel> list = null;
                if (!dic.TryGetValue(type, out list))
                {
                    dic[type] = list = GetTypeInfo(type, langue, directory);
                }

                m = list.Find(q => string.Equals(q.Name, name, StringComparison.OrdinalIgnoreCase));
            }

            return m;
        }

    }
}

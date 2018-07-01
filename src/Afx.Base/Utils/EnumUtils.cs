using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;

using Afx.Collections;

namespace Afx.Utils
{
    /// <summary>
    /// EnumUtils
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
        /// GetDisplayName
        /// </summary>
        /// <param name="enum"></param>
        /// <param name="langue"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum @enum, string langue = null, string directory = null)
        {
            var m = GetInfo(@enum, langue, directory);

            return m != null ? m.DisplayName : @enum.GetName();
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

            public string DisplayName { get; private set; }

            public string Description { get; private set; }

            public EnumInfoModel(string name, string displayName, string description)
            {
                this.Name = name;
                this.DisplayName = displayName;
                this.Description = description;
            }
        }

        private static SafeDictionary<string, SafeDictionary<Type, List<EnumInfoModel>>> enumDic = new SafeDictionary<string, SafeDictionary<Type, List<EnumInfoModel>>>();

        private static SafeDictionary<Type, List<EnumInfoModel>> GetLangueDic(string langue)
        {
            SafeDictionary<Type, List<EnumInfoModel>> dic = null;
            if (enumDic.ContainsKey(langue))
            {
                dic = enumDic[langue];
            }
            else
            {
                dic = new SafeDictionary<Type, List<EnumInfoModel>>();
                enumDic[langue] = dic;
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
                string displayName = null;
                string description = null;
                if (rootElement != null)
                {
                    var node = rootElement[s];
                    if (node != null)
                    {
                        displayName = node.GetAttribute("displayName");
                        description = node.GetAttribute("description");
                    }
                }

                if (string.IsNullOrEmpty(displayName))
                {
                    var att = (DisplayNameAttribute)Attribute.GetCustomAttribute(type.GetField(s), typeof(DisplayNameAttribute), false);
                    if (att != null) displayName = att.DisplayName;
                    if (string.IsNullOrEmpty(displayName)) displayName = s;
                }

                if (string.IsNullOrEmpty(description))
                {
                    var att = (DescriptionAttribute)Attribute.GetCustomAttribute(type.GetField(s), typeof(DescriptionAttribute), false);
                    if (att != null) description = att.Description;
                    if (string.IsNullOrEmpty(description)) description = s;
                }

                list.Add(new EnumInfoModel(s, displayName, description));
            }

            return list;
        }

        private static EnumInfoModel GetInfo(Enum @enum, string langue, string directory)
        {
            EnumInfoModel m = null;
            if (string.IsNullOrEmpty(langue)) langue = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (string.IsNullOrEmpty(directory)) directory = AppDomain.CurrentDomain.BaseDirectory;
            var dic = GetLangueDic(langue);
            if (dic != null)
            {
                var type = @enum.GetType();
                var name = @enum.GetName();
                List<EnumInfoModel> list = null;
                if (dic.ContainsKey(type))
                {
                    list = dic[type];
                }
                else
                {
                    list = GetTypeInfo(type, langue, directory);
                    dic[type] = list;
                }

                m = list.Find(q => string.Equals(q.Name, name, StringComparison.OrdinalIgnoreCase));
            }

            return m;
        }

    }
}

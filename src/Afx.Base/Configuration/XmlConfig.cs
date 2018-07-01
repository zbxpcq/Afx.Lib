using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using Afx.Threading;
using Afx.Utils;

namespace Afx.Configuration
{
    /// <summary>
    /// xml 文件被外部修改事件
    /// </summary>
    /// <param name="xmlConfig"></param>
    public delegate void ChangedXmlConfigEvent(XmlConfig xmlConfig);
    /// <summary>
    /// XmlConfig
    /// </summary>
    public class XmlConfig : IDisposable
    {
        /// <summary>
        /// FileName
        /// </summary>
        public string FileName { get; private set; }
        private ReadWriteLock m_rwLock;
        private XmlDocument m_xmlDoc;
        private XmlElement m_rootElement;
        private FileSystemWatcher m_fileWatcher;

        private bool EnabledChangedEvent { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="xmlFile">配置文件路径</param>
        public XmlConfig(string xmlFile)
            : this(xmlFile, false)
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="xmlFile">配置文件路径</param>
        /// <param name="enabledChangedEvent">是否监听外部修改</param>
        public XmlConfig(string xmlFile, bool enabledChangedEvent)
        {
            if (string.IsNullOrEmpty(xmlFile)) throw new ArgumentNullException("xmlFile");
            string filePath = PathUtils.GetFileFullPath(xmlFile);
            if (!File.Exists(filePath)) throw new FileNotFoundException(xmlFile + " not found！", xmlFile);

            this.FileName = xmlFile;
            this.m_rwLock = new ReadWriteLock();
            using (var fs = this.GetFileStream(false))
            {
                this.Refresh(fs);
            }

            this.EnabledChangedEvent = enabledChangedEvent;
            var info = new FileInfo(filePath);
            this.m_fileWatcher = new FileSystemWatcher();
            m_fileWatcher.Path = info.Directory.FullName;
            m_fileWatcher.Filter = info.Name;
            m_fileWatcher.Changed += fileWatcher_Changed;
            m_fileWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            m_fileWatcher.EnableRaisingEvents = this.EnabledChangedEvent;
        }

        /// <summary>
        /// xml 文件被外部修改事件
        /// </summary>
        public event ChangedXmlConfigEvent ChangedEvent;
        private void OnChangedEvent()
        {
            if (this.ChangedEvent != null)
            {
                try { this.ChangedEvent(this); }
                catch { }
            }
        }

        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed
                || e.ChangeType == WatcherChangeTypes.Created
                || e.ChangeType == WatcherChangeTypes.Deleted
                || e.ChangeType == WatcherChangeTypes.Renamed)
            {
                using (this.m_rwLock.GetReadLock())
                {
                    using (var fs = this.GetFileStream(false))
                    {
                        this.Refresh(fs);
                    }
                }
                this.OnChangedEvent();
            }
        }

        private FileStream GetFileStream(bool isWrite)
        {
            string filePath = PathUtils.GetFileFullPath(this.FileName);
            if (!File.Exists(filePath)) return null;
            int trycount = 0;
            if (isWrite)
            {
                while (trycount < 5000)
                {
                    try
                    {
                        return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException)
                        {
                            trycount++;
                            Thread.Sleep(10);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                while (trycount < 5000)
                {
                    try
                    {
                        return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException)
                        {
                            trycount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return null;
        }

        private void Refresh(FileStream fs)
        {
            if(this.m_xmlDoc == null)  this.m_xmlDoc = new XmlDocument();
            if(this.m_xmlDoc.DocumentElement != null) this.m_xmlDoc.DocumentElement.RemoveAll();
            if (fs != null && fs.Length > 0)
            {
                fs.Seek(0, SeekOrigin.Begin);
                try { this.m_xmlDoc.Load(fs); }
                catch { }
            }
            if (this.m_xmlDoc.ChildNodes.Count == 0)
            {
                XmlDeclaration xmlDeclaration = this.m_xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                this.m_xmlDoc.AppendChild(xmlDeclaration);
            }
            this.m_rootElement = this.m_xmlDoc.DocumentElement;
            if (this.m_rootElement == null)
            {
                this.m_rootElement = this.m_xmlDoc.CreateElement("Config");
                this.m_xmlDoc.AppendChild(this.m_rootElement);
            }
        }

        private void Flush(FileStream fs)
        {
            if (fs != null)
            {
                this.m_fileWatcher.EnableRaisingEvents = false;
                try
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    this.m_xmlDoc.Save(fs);
                    fs.SetLength(fs.Position);
                    fs.Flush();
                }
                catch { }
                this.m_fileWatcher.EnableRaisingEvents = this.EnabledChangedEvent;
            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="item">item</param>
        /// <param name="defaultValue">未找到返回的默认值</param>
        /// <returns>返回</returns>
        public string Get(string node, string item, string defaultValue = null)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(item))
            {
                item = item.Trim();
            }

            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(item))
            {
                using (this.m_rwLock.GetReadLock())
                {
                    XmlElement nodeElement = this.m_rootElement[node];
                    if (nodeElement != null)
                    {
                        XmlElement itemElement = nodeElement[item];
                        if (itemElement != null)
                        {
                            defaultValue = itemElement.InnerText;
                        }
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="item">item</param>
        /// <param name="value">值</param>
        public void Set(string node, string item, string value)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(item))
            {
                item = item.Trim();
            }

            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(item))
            {
                try
                {
                    using (this.m_rwLock.GetWriteLock())
                    {
                        using (var fs = this.GetFileStream(true))
                        {
                            this.Refresh(fs);
                            XmlElement nodeElement = this.m_rootElement[node];
                            if (nodeElement == null)
                            {
                                nodeElement = this.m_xmlDoc.CreateElement(node);
                                this.m_rootElement.AppendChild(nodeElement);
                            }
                            XmlElement itemElement = nodeElement[item];
                            if (itemElement == null)
                            {
                                itemElement = this.m_xmlDoc.CreateElement(item);
                                nodeElement.AppendChild(itemElement);
                            }
                            itemElement.InnerText = value ?? "";
                            this.Flush(fs);
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 获取指定节点 attribute
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="attribute">attribute名称</param>
        /// <param name="defaultValue">未找到返回的默认值</param>
        /// <returns></returns>
        public string GetNodeAttribute(string node, string attribute, string defaultValue = null)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(attribute))
            {
                attribute = attribute.Trim();
            }

            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(attribute))
            {
                using (this.m_rwLock.GetReadLock())
                {
                    XmlElement nodeElement = this.m_rootElement[node];
                    if (nodeElement != null)
                    {
                        defaultValue = nodeElement.GetAttribute(attribute);
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 设置指定节点 attribute
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="attribute">attribute名称</param>
        /// <param name="value"></param>
        public void SetNodeAttribute(string node, string attribute, string value)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(attribute))
            {
                attribute = attribute.Trim();
            }

            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(attribute))
            {
                try
                {
                    using (this.m_rwLock.GetWriteLock())
                    {
                        using (var fs = this.GetFileStream(true))
                        {
                            this.Refresh(fs);
                            XmlElement nodeElement = this.m_rootElement[node];
                            if (nodeElement == null)
                            {
                                nodeElement = this.m_xmlDoc.CreateElement(node);
                                this.m_rootElement.AppendChild(nodeElement);
                            }
                            nodeElement.SetAttribute(attribute, value ?? "");
                            this.Flush(fs);
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 获取指定item attribute
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="item">item name</param>
        /// <param name="attribute">attribute名称</param>
        /// <param name="defaultValue">未找到返回的默认值</param>
        /// <returns></returns>
        public string GetItemAttribute(string node, string item, string attribute, string defaultValue = null)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(item))
            {
                item = item.Trim();
            }

            if (!string.IsNullOrEmpty(attribute))
            {
                attribute = attribute.Trim();
            }

            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(attribute))
            {
                using (this.m_rwLock.GetReadLock())
                {
                    XmlElement nodeElement = this.m_rootElement[node];
                    if (nodeElement != null)
                    {
                        XmlElement itemElement = nodeElement[item];
                        if (itemElement != null)
                        {
                            defaultValue = itemElement.GetAttribute(attribute);
                        }
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 设置指定item attribute
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="item">item name</param>
        /// <param name="attribute">attribute名称</param>
        /// <param name="value"></param>
        public void SetItemAttribute(string node, string item, string attribute, string value)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(item))
            {
                item = item.Trim();
            }

            if (!string.IsNullOrEmpty(attribute))
            {
                attribute = attribute.Trim();
            }

            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(attribute))
            {
                try
                {
                    using (this.m_rwLock.GetWriteLock())
                    {
                        using (var fs = this.GetFileStream(true))
                        {
                            this.Refresh(fs);
                            XmlElement nodeElement = this.m_rootElement[node];
                            if (nodeElement == null)
                            {
                                nodeElement = this.m_xmlDoc.CreateElement(node);
                                this.m_rootElement.AppendChild(nodeElement);
                            }
                            XmlElement itemElement = nodeElement[item];
                            if (itemElement == null)
                            {
                                itemElement = this.m_xmlDoc.CreateElement(item);
                                nodeElement.AppendChild(itemElement);
                            }
                            itemElement.SetAttribute(attribute, value ?? "");
                            this.Flush(fs);
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 移除节点
        /// </summary>
        /// <param name="node">节点名称</param>
        public void RemoveNode(string node)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(node))
            {
                try
                {
                    using (this.m_rwLock.GetWriteLock())
                    {
                        using (var fs = this.GetFileStream(true))
                        {
                            this.Refresh(fs);
                            XmlElement nodeElement = this.m_rootElement[node];
                            if (nodeElement != null)
                            {
                                this.m_rootElement.RemoveChild(nodeElement);
                                this.Flush(fs);
                            }
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 移除指定节点key
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="name">name</param>
        public void RemoveKey(string node, string name)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }
            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
            }


            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(name))
            {
                try
                {
                    using (this.m_rwLock.GetWriteLock())
                    {
                        using (var fs = this.GetFileStream(true))
                        {
                            this.Refresh(fs);
                            XmlElement nodeElement = this.m_rootElement[node];
                            if (nodeElement != null)
                            {
                                XmlElement keyElement = nodeElement[name];
                                if (keyElement != null)
                                {
                                    nodeElement.RemoveChild(keyElement);
                                    this.Flush(fs);
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 释放配置文件对象
        /// </summary>
        public void Dispose()
        {
            if (this.m_fileWatcher != null)
            {
                this.m_fileWatcher.EnableRaisingEvents = false;
                this.m_fileWatcher.Dispose();
            }
            this.m_fileWatcher = null;
            if (this.m_rwLock != null)
            {
                this.m_rwLock.Dispose();
                this.m_rwLock = null;
            }
            this.FileName = null;
            this.m_xmlDoc = null;
            this.m_rootElement = null;
        }
    }
}

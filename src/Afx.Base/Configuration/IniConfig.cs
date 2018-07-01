using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using Afx.Threading;
using Afx.Utils;

namespace Afx.Configuration
{
    /// <summary>
    /// ini 文件被外部修改事件
    /// </summary>
    /// <param name="iniConfig"></param>
    public delegate void IniConfigChangedEvent(IniConfig iniConfig);

    /// <summary>
    /// ini 配置文件
    /// </summary>
    public sealed class IniConfig : IDisposable
    {
        /// <summary>
        /// FileName
        /// </summary>
        public string FileName { get; private set; }
        private List<NodeModel> nodeList;
        private ReadWriteLock rwLock;
        private FileSystemWatcher m_fileWatcher;
        private bool EnabledChangedEvent { get; set; }
        
        /// <summary>
        /// ini 文件被外部修改事件
        /// </summary>
        public event IniConfigChangedEvent ChangedEvent;
        private void OnChangedEvent()
        {
            if (this.ChangedEvent != null)
            {
                try { this.ChangedEvent(this); }
                catch { }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iniFile">配置文件路径</param>
        public IniConfig(string iniFile)
            : this(iniFile, false)
        {
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iniFile">配置文件路径</param>
        /// <param name="enabledChangedEvent">是否监听外部修改</param>
        public IniConfig(string iniFile, bool enabledChangedEvent)
        {
            if (string.IsNullOrEmpty(iniFile)) throw new ArgumentNullException("iniFile");
            string filePath = PathUtils.GetFileFullPath(iniFile);
            if (!File.Exists(filePath)) throw new FileNotFoundException(iniFile + " not found！", iniFile);
            
            this.FileName = iniFile;
            this.nodeList = new List<NodeModel>();
            this.rwLock = new ReadWriteLock();
            using (var fs = this.GetFileStream(false))
            {
                this.Refresh(fs);
            }

            var info = new FileInfo(filePath);
            this.EnabledChangedEvent = enabledChangedEvent;
            this.m_fileWatcher = new FileSystemWatcher();
            m_fileWatcher.Path = info.Directory.FullName;
            m_fileWatcher.Filter = info.Name;
            m_fileWatcher.Changed += fileWatcher_Changed;
            m_fileWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            m_fileWatcher.EnableRaisingEvents = this.EnabledChangedEvent;
        }
        
        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed
                || e.ChangeType == WatcherChangeTypes.Created
                || e.ChangeType == WatcherChangeTypes.Deleted
                || e.ChangeType == WatcherChangeTypes.Renamed)
            {
                using (this.rwLock.GetWriteLock())
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
            try
            {
                this.nodeList.Clear();
                if (fs != null)
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    var sr = new StreamReader(fs);
                    NodeModel node = null;
                    string line = null;
                    StringBuilder comments = new StringBuilder();
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (comments == null) comments = new StringBuilder();
                        if (string.IsNullOrEmpty(line) || line[0] == '#')
                        {
                            comments.Append(line);
                            comments.Append("\r\n");
                            continue;
                        }

                        string nodeName = this.GetNodeName(line);
                        if (nodeName != null)
                        {
                            node = this.nodeList.Find(q => q.Name == nodeName);
                            if (node == null)
                            {
                                node = new NodeModel() { Name = nodeName };
                                this.nodeList.Add(node);
                            }

                            if (node.Comments == null)
                            {
                                node.Comments = comments;
                            }
                            else
                            {
                                node.Comments.Append(comments);
                            }
                        }
                        else
                        {
                            this.GetItem(line, node, comments);
                        }
                        comments = null;
                    }
                }
            }
            catch { }
        }

        private string GetNodeName(string line)
        {
            string result = null;
            if (line.Length > 2 && line[0] == '[' && line[line.Length - 1] == ']')
            {
                string node = line.Substring(1, line.Length - 2).Trim();
                if (!string.IsNullOrEmpty(node))
                {
                    result = node;
                }
            }

            return result;
        }

        private void GetItem(string line, NodeModel node, StringBuilder comments)
        {
            if (node != null)
            {
                int index = line.IndexOf('=');
                if (index > 0)
                {
                    string key = line.Substring(0, index).Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        string value = index < line.Length - 1
                            ? line.Substring(index + 1, line.Length - index - 1) : null;
                        var m = node.Items.Find(q => q.Name == key);
                        if (m == null)
                        {
                            m = new ItemModel() { Name = key };
                            node.Items.Add(m);
                        }
                        m.Value = value;
                        if (m.Comments == null)
                        {
                            m.Comments = comments;
                        }
                        else
                        {
                            m.Comments.Append(comments);
                        }
                    }
                }
            }
        }

        private void Flush(FileStream fs)
        {
            this.m_fileWatcher.EnableRaisingEvents = false;
            try
            {
                if (fs != null)
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    foreach (var node in this.nodeList)
                    {
                        string s = null;
                        byte[] buffer = null;
                        if (node.Comments != null && node.Comments.Length > 0)
                        {
                            s = node.Comments.ToString();
                            buffer = Encoding.UTF8.GetBytes(s);
                            fs.Write(buffer, 0, buffer.Length);
                        }
                        s = string.Format("[{0}]\r\n", node.Name);
                        buffer = Encoding.UTF8.GetBytes(s);
                        fs.Write(buffer, 0, buffer.Length);
                        foreach (var m in node.Items)
                        {
                            if (m.Comments != null && m.Comments.Length > 0)
                            {
                                s = m.Comments.ToString();
                                buffer = Encoding.UTF8.GetBytes(s);
                                fs.Write(buffer, 0, buffer.Length);
                            }
                            s = string.Format("{0}={1}\r\n", m.Name, m.Value ?? "");
                            buffer = Encoding.UTF8.GetBytes(s);
                            fs.Write(buffer, 0, buffer.Length);
                        }
                    }
                    fs.SetLength(fs.Position);
                    fs.Flush();
                }
            }
            catch { }
            this.m_fileWatcher.EnableRaisingEvents = this.EnabledChangedEvent;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        public void Set(string node, string key, string value)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(key))
            {
                key = key.Trim();
            }

            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(key))
            {
                try
                {
                    using (this.rwLock.GetWriteLock())
                    {
                        using (var fs = this.GetFileStream(true))
                        {
                            this.Refresh(fs);
                            var iniNode = this.nodeList.Find(q => q.Name == node);
                            if (iniNode == null)
                            {
                                iniNode = new NodeModel() { Name = node };
                                this.nodeList.Add(iniNode);
                            }
                            var m = iniNode.Items.Find(q => q.Name == key);
                            if (m == null)
                            {
                                m = new ItemModel() { Name = key };
                                iniNode.Items.Add(m);
                            }
                            m.Value = value;
                            this.Flush(fs);
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="node">节点名称</param>
        /// <param name="key">key</param>
        /// <param name="defaultValue">未找到返回的默认值</param>
        /// <returns>返回</returns>
        public string Get(string node, string key, string defaultValue = null)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }

            if (!string.IsNullOrEmpty(key))
            {
                key = key.Trim();
            }

            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(key))
            {
                try
                {
                    using (this.rwLock.GetReadLock())
                    {
                        var iniNode = this.nodeList.Find(q => q.Name == node);
                        if (iniNode != null)
                        {
                            var m = iniNode.Items.Find(q => q.Name == key);
                            if (m != null && m.Value != null)
                            {
                                defaultValue = m.Value;
                            }
                        }
                    }
                }
                catch { }
            }

            return defaultValue;
        }

        /// <summary>
        /// 移除节点
        /// </summary>
        /// <param name="node"></param>
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
                    using (this.rwLock.GetWriteLock())
                    {
                        using (var fs = this.GetFileStream(true))
                        {
                            this.Refresh(fs);
                            this.nodeList.RemoveAll(q => q.Name == node);
                            this.Flush(fs);
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 移除指定节点key
        /// </summary>
        /// <param name="node"></param>
        /// <param name="key"></param>
        public void RemoveKey(string node, string key)
        {
            if (!string.IsNullOrEmpty(node))
            {
                node = node.Trim();
            }
            if (!string.IsNullOrEmpty(key))
            {
                key = key.Trim();
            }


            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(key))
            {
                try
                {
                    using (this.rwLock.GetWriteLock())
                    {
                        var iniNode = this.nodeList.Find(q => q.Name == node);
                        if (iniNode != null)
                        {
                            using (var fs = this.GetFileStream(true))
                            {
                                this.Refresh(fs);
                                var m = iniNode.Items.Find(q => q.Name == key);
                                if (m != null)
                                {
                                    iniNode.Items.Remove(m);
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
            if (this.rwLock != null)
            {
                this.rwLock.Dispose();
                this.rwLock = null;
            }
            if (this.nodeList != null)
            {
                this.nodeList.Clear();
                this.nodeList = null;
            }
            this.FileName = null;
        }
    }
}

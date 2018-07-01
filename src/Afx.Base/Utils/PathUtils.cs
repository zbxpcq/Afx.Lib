using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// PathUtils
    /// </summary>
    public static class PathUtils
    {
        const string PREV_PATH = "..\\";
        const string ROOT_PATH = "~\\";
        /// <summary>
        /// 获取文件全路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileFullPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            string fullpath = fileName.Replace('/', '\\');
            bool isProc = false;
            if (fullpath.StartsWith(PREV_PATH))
            {
                string s = fullpath;
                DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
                while (s.StartsWith(PREV_PATH))
                {
                    s = s.Substring(PREV_PATH.Length) ?? "";
                    if (dir.Parent != null) dir = dir.Parent;
                }
                s = s.TrimStart('\\');
                fullpath = Path.Combine(dir.FullName, s);
                isProc = true;
            }
            else if (fullpath.StartsWith(ROOT_PATH))
            {
                string s = fullpath.Substring(ROOT_PATH.Length);
                fullpath = Path.Combine(Directory.GetCurrentDirectory(), s);
                isProc = true;
            }

            switch (Environment.OSVersion.Platform)
            {
#if !NETCOREAPP
                case PlatformID.MacOSX:
#endif
                case PlatformID.Unix:
                    fullpath = fullpath.Replace('\\', '/');
                    if (!isProc && !File.Exists(fullpath) && !fullpath.StartsWith("/"))
                    {
                        string s = Path.Combine(Directory.GetCurrentDirectory(), fullpath);
                        if (File.Exists(s))
                        {
                            fullpath = s;
                            isProc = true;
                        }
                    }
                    break;
#if !NETCOREAPP
                case PlatformID.WinCE:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Xbox:
#endif
                case PlatformID.Win32NT:
                    if (!isProc && !File.Exists(fullpath) && (fullpath.StartsWith("\\") || fullpath.IndexOf(':') < 0))
                    {
                        string s = Path.Combine(Directory.GetCurrentDirectory(), fullpath.TrimStart('\\'));
                        if (File.Exists(s))
                        {
                            fullpath = s;
                            isProc = true;
                        }
                    }
                    break;
            }

            return fullpath;
        }

        /// <summary>
        /// 获取目录全路径
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string GetDirectoryFullPath(string directory)
        {
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException("directory");
            string fullpath = directory.Replace('/', '\\');
            bool isProc = false;
            if (fullpath.StartsWith(PREV_PATH))
            {
                string s = fullpath;
                DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
                while (s.StartsWith(PREV_PATH))
                {
                    s = s.Substring(PREV_PATH.Length) ?? "";
                    if (dir.Parent != null) dir = dir.Parent;
                }
                s = s.TrimStart('\\');
                fullpath = Path.Combine(dir.FullName, s);
                isProc = true;
            }
            else if (fullpath.StartsWith(ROOT_PATH))
            {
                string s = fullpath.Substring(ROOT_PATH.Length);
                if (string.IsNullOrEmpty(s)) fullpath = Directory.GetCurrentDirectory();
                else fullpath = Path.Combine(Directory.GetCurrentDirectory(), s);
                isProc = true;
            }

            switch (Environment.OSVersion.Platform)
            {
#if !NETCOREAPP
                case PlatformID.MacOSX:
#endif
                case PlatformID.Unix:
                    fullpath = fullpath.Replace('\\', '/');
                    if (!isProc && !Directory.Exists(fullpath) && !fullpath.StartsWith("/"))
                    {
                        string s = Path.Combine(Directory.GetCurrentDirectory(), fullpath);
                        if (Directory.Exists(s))
                        {
                            fullpath = s;
                            isProc = true;
                        }
                    }
                    break;
#if !NETCOREAPP
                case PlatformID.WinCE:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Xbox:
#endif
                case PlatformID.Win32NT:
                    if (!isProc && !Directory.Exists(fullpath) && (fullpath.StartsWith("\\") || fullpath.IndexOf(':') < 0))
                    {
                        string s = Path.Combine(Directory.GetCurrentDirectory(), fullpath.TrimStart('\\'));
                        if (Directory.Exists(s))
                        {
                            fullpath = s;
                            isProc = true;
                        }
                    }
                    break;
            }

            return fullpath;
        }

        /// <summary>
        /// GetPath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPath(string path)
        {
            string s = path ?? "";
            switch (Environment.OSVersion.Platform)
            {
#if !NETCOREAPP
                case PlatformID.MacOSX:
#endif
                case PlatformID.Unix:
                    s = s.Replace('\\', '/');
                    break;
#if !NETCOREAPP
                case PlatformID.WinCE:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Xbox:
#endif
                case PlatformID.Win32NT:
                    s = s.Replace('/', '\\');
                    break;
            }

            return s;
        }
    }
}

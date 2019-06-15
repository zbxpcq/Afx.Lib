using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Afx.Sockets;
using Afx.Tcp.Protocols;
using System.IO;
using System.Xml;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// tcp host
    /// </summary>
    public class TcpHost : IDisposable
    {
        private TcpServer server;

        /// <summary>
        /// 初始化
        /// </summary>
        public TcpHost()
        {
            this.server = new TcpServer();
            this.server.AcceptEvent += OnAccept;
            this.server.ServerErrorEvent += OnServerError;
            this.server.ClientErrorEvent += OnClientClosed;
            this.server.ClientReceiveEvent += OnClientReceive;
            this.server.ClientReadingEvent += OnClientReadingEvent;

            this.CurrentDirectory = Directory.GetCurrentDirectory();
        }
        /// <summary>
        /// 加密回调
        /// </summary>
        public Func<byte[], byte[]> Encrypt;
        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected virtual byte[] OnEncrypt(byte[] buffer)
        {
            if (this.Encrypt != null)
            {
                buffer = this.Encrypt(buffer);
            }

            return buffer;
        }

        /// <summary>
        /// 解密回调
        /// </summary>
        public Func<byte[], byte[]> Decrypt;
        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected virtual byte[] OnDecrypt(byte[] buffer)
        {
            if (this.Decrypt != null)
            {
                buffer = this.Decrypt(buffer);
            }

            return buffer;
        }

        private void OnClientReadingEvent(ITcpClientAsync client, int length)
        {
            Session session = client.UserState as Session;
            if (session != null)
            {
                session.LastReceiveTime = DateTime.Now;
            }
        }

        bool isStart = false;
        /// <summary>
        /// 启动host 监听客户端连接
        /// </summary>
        /// <param name="port"></param>
        /// <param name="isBackground">是否后台监听连接</param>
        public virtual void Start(int port, bool isBackground = true)
        {
            if (isStart) return;
            isStart = true;
            this.server.Start(port, isBackground);
        }
        /// <summary>
        /// 停止监听客户端连接
        /// </summary>
        public virtual void Stop()
        {
            isStart = false;
            this.server.Close();
        }

        /// <summary>
        /// 客户端连接成功回调
        /// </summary>
        public event ClientConnected ClientConnectedEvent;
        /// <summary>
        /// 客户端连接成功
        /// </summary>
        /// <param name="session"></param>
        protected virtual void OnClientConnected(Session session)
        {
            if (this.ClientConnectedEvent != null)
            {
                try { this.ClientConnectedEvent(this, session); }
                catch { }
            }
        }

        /// <summary>
        /// 监听到客户端连接
        /// </summary>
        /// <param name="client"></param>
        protected virtual void OnAccept(ITcpClientAsync client)
        {
            Session session = new Session(client);
            session.SendCall = this.SendMsg;
            session.CloseCall = this.CloseClient;
            client.UserState = session;
            this.OnClientConnected(session);
        }

        /// <summary>
        /// host 发生错误回调
        /// </summary>
        public event MvcHostServerError MvcHostServerErrorEvent;
        /// <summary>
        /// host 发生错误回调
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnMvcHostServerErrorEvent(Exception ex)
        {
            if (this.MvcHostServerErrorEvent != null)
            {
                try { this.MvcHostServerErrorEvent(this, ex); }
                catch { }
            }
        }

        private void OnServerError(ITcpServer server, Exception ex)
        {
            this.OnMvcHostServerErrorEvent(ex);
        }
        /// <summary>
        /// client 关闭 回调
        /// </summary>
        public event ClientClosed ClientClosedEvent;
        /// <summary>
        /// client 关闭事件
        /// </summary>
        /// <param name="session"></param>
        protected virtual void OnClientClosedEvent(Session session)
        {
            if (this.ClientClosedEvent != null)
            {
                try { this.ClientClosedEvent(this, session); }
                catch { }
            }
        }

        private void OnClientClosed(ITcpClientAsync client, Exception ex)
        {
            Session session = client.UserState as Session;
            this.OnClientClosedEvent(session);
            session.SendCall = null;
            session.CloseCall = null;
            client.Dispose();
            session.Dispose();
        }

        private void CloseClient(Session session)
        {
            session.Client.Close();
            this.OnClientClosed(session.Client, null);
        }
        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="session"></param>
        protected virtual void SendMsg(MsgData msg, Session session)
        {
            if (session.Client != null && session.Client.IsConnected)
            {
                byte[] buffer = OnEncrypt(msg.Serialize());
                session.Client.Send(buffer);
            }
        }

        private void OnClientReceive(ITcpClientAsync client, List<byte[]> data)
        {
            if (data == null || data.Count == 0) return;
            foreach (var arr in data)
            {
                byte[] buffer = this.OnDecrypt(arr);
                MsgData msg = MsgData.Deserialize(buffer);
                if (msg != null)
                {
                    try
                    {
                        if (cmdCallbackDic.ContainsKey(msg.Cmd))
                        {
                            var callModel = cmdCallbackDic[msg.Cmd];
                            this.ExecCall(callModel, client, msg);
                            return;
                        }
                        else
                        {
                            msg.Rest();
                            msg.Status = (int)MsgStatus.Unknown;
                            this.SendMsg(msg, client.UserState as Session);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.OnMvcHostServerErrorEvent(ex);
                    }
                }
                else
                {
                    this.OnMvcHostServerErrorEvent(new Exception(string.Format("client({0}): msg is null!", client.RemoteEndPoint.ToString())));
                }
            }
        }
        /// <summary>
        /// 执行cmd前回调
        /// </summary>
        public event CmdExecuting CmdExecutingEvent;
        /// <summary>
        /// 执行cmd前回调
        /// </summary>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        protected virtual void OnCmdExecutingEvent(Session session, MsgData msg)
        {
            if (this.CmdExecutingEvent != null)
            {
                try { this.CmdExecutingEvent(this, session, msg); }
                catch { }
            }
        }

        /// <summary>
        /// 执行cmd后回调
        /// </summary>
        public event CmdExecuted CmdExecutedEvent;
        /// <summary>
        /// 执行cmd后回调
        /// </summary>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        /// <param name="result"></param>
        protected virtual void OnCmdExecutedEvent(Session session, MsgData msg, MsgData result)
        {
            if (this.CmdExecutedEvent != null)
            {
                try { this.CmdExecutedEvent(this, session, msg, result); }
                catch { }
            }
        }

        private void ExecCall(CmdMethodInfo cmdMethodInfo, ITcpClientAsync client, MsgData msg)
        {
            var session = client.UserState as Session;
            this.OnCmdExecutingEvent(session, msg);
            if (cmdMethodInfo != null && client != null)
            {
                ActionResult result = null;

                using (var controller = Activator.CreateInstance(cmdMethodInfo.Type) as Controller)
                {
                    try
                    {
                        controller.Init(session, msg);
                        if (this.OnAuth(cmdMethodInfo, session, msg, out result))
                        {
                            controller.OnExecuting();
                            object[] parameters = null;
                            if(cmdMethodInfo.ParameterType != null)
                            {
                                var o = msg.GetData(cmdMethodInfo.ParameterType);
                                parameters = new object[] { o };
                            }
                            var obj = cmdMethodInfo.Method.Invoke(controller, parameters);
                            if(obj != null && obj is ActionResult)
                            {
                                result = obj as ActionResult;
                            }
                            else if(obj != null && obj is MsgData)
                            {
                                result = new ActionResult(obj as MsgData);
                            }
                            else
                            {
                                result = new ActionResult();
                                result.SetMsg(MsgStatus.Succeed, null);
                                if (obj != null)
                                {
#if NET20
                                    result.Result.SetData(obj);
#else
                                    var t = cmdMethodInfo.Method.ReturnType;
                                    if (t == typeof(System.Threading.Tasks.Task)
                                        || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>)))
                                    {
                                        var task = obj as System.Threading.Tasks.Task;
                                        if (task.Status == System.Threading.Tasks.TaskStatus.Created) task.Start();
                                        if (!task.IsCompleted || !task.IsCanceled) task.Wait();
                                        if (task.IsFaulted && task.Exception != null) throw task.Exception;
                                        if(t.IsGenericType)
                                        {
                                            var o = t.GetProperty("Result").GetValue(obj, null);
                                            result.Result.SetData(o);
                                        }
                                    }
                                    else
                                    {
                                        result.Result.SetData(obj);
                                    }
#endif
                                }
                            }
                            result.Result.Cmd = msg.Cmd;
                            result.Result.Id = msg.Id;
                            controller.OnResult(result);
                            this.OnCmdExecutedEvent(session, msg, result.Result);
                        }
                    }
                    catch (Exception ex)
                    {
                        bool ishand = false;
                        try
                        {
                            ExceptionContext excontext = new ExceptionContext() { Msg = msg, IsHandle = false, Exception = ex };
                            controller.OnException(excontext);
                            ishand = excontext.IsHandle;
                            result = excontext.Result;
                        }
                        catch (Exception _ex)
                        {
                            this.OnMvcHostServerErrorEvent(_ex);
                        }
                        if (!ishand)
                        {
                            this.OnMvcHostServerErrorEvent(ex);
                        }
                        if (result == null)
                        {
                            result = new ActionResult();
                            result.SetMsg(MsgStatus.ServerError, "服务器发生错误！");
                        }
                    }
                }


                if (result != null && client.IsConnected)
                {
                    result.Result.Cmd = msg.Cmd;
                    result.Result.Id = msg.Id;
                    this.SendMsg(result.Result, session);
                }
            }
        }

        private bool OnAuth(CmdMethodInfo cmdMethodInfo, Session session, MsgData msg, out ActionResult result)
        {
            result = null;
            if (cmdMethodInfo.NoAuth) return true;
            using (AuthorizationContext authContext = new AuthorizationContext()
            {
                Session = session,
                Cmd = msg.Cmd,
                IsAuth = false,
                Result = null
            })
            {
                foreach (var t in this.GlobalAuthTypeList)
                {
                    IAuthorize auth = (IAuthorize)Activator.CreateInstance(t);
                    auth.OnAuthorization(authContext);
                    if (!authContext.IsAuth)
                    {
                        result = authContext.Result;
                        if (result == null)
                        {
                            result = new ActionResult();
                            result.SetMsg(MsgStatus.NeedAuth, "无权限请求！");
                        }

                        return false;
                    }
                }

                var tarr = cmdMethodInfo.Type.GetCustomAttributes(typeof(AuthorizeAttribute), true);
                foreach (var attr in tarr)
                {
                    var auth = attr as AuthorizeAttribute;
                    auth.OnAuthorization(authContext);
                    if (!authContext.IsAuth)
                    {
                        result = authContext.Result;
                        if (result == null)
                        {
                            result = new ActionResult();
                            result.SetMsg(MsgStatus.NeedAuth, "无权限请求！");
                        }

                        return false;
                    }
                }


                if (cmdMethodInfo.AuthTypeList != null && cmdMethodInfo.AuthTypeList.Count > 0)
                {
                    foreach (var t in cmdMethodInfo.AuthTypeList)
                    {
                        IAuthorize auth = (IAuthorize)Activator.CreateInstance(t);
                        auth.OnAuthorization(authContext);
                        if (!authContext.IsAuth)
                        {
                            result = authContext.Result;
                            if (result == null)
                            {
                                result = new ActionResult();
                                result.SetMsg(MsgStatus.NeedAuth, "无权限请求！");
                            }

                            return false;
                        }
                    }
                }

                tarr = cmdMethodInfo.Method.GetCustomAttributes(typeof(AuthorizeAttribute), true);
                foreach (var attr in tarr)
                {
                    var auth = attr as AuthorizeAttribute;
                    auth.OnAuthorization(authContext);
                    if (!authContext.IsAuth)
                    {
                        result = authContext.Result;
                        if (result == null)
                        {
                            result = new ActionResult();
                            result.SetMsg(MsgStatus.NeedAuth, "无权限请求！");
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            this.Stop();
            this.cmdCallbackDic.Clear();
        }

        private readonly Dictionary<int, CmdMethodInfo> cmdCallbackDic = new Dictionary<int, CmdMethodInfo>();
        private readonly List<Type> GlobalAuthTypeList = new List<Type>();

        const string PREV_PATH = "..\\";
        const string ROOT_PATH = "~\\";
        private string GetFileFullPath(string fileName)
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

        private Dictionary<string, Assembly> assemblyDic = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        private Assembly GetAssembly(string name)
        {
            Assembly assembly = null;
            if (!string.IsNullOrEmpty(name))
            {
                if (assemblyDic.TryGetValue(name, out assembly))
                {
                    return assembly;
                }

                var arr = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var item in arr)
                {
                    var s = item.FullName.Split(',')[0].Trim();
                    if (string.Equals(s, name, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(item.ManifestModule.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        assembly = item;
                        assemblyDic[name] = assembly;
                        return assembly;
                    }
                }

                if (assembly == null)
                {
                    try
                    {
                        if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            assembly = Assembly.Load(name.Substring(0, name.Length - ".dll".Length));
                        }
                        else
                        {
                            assembly = Assembly.Load(name);
                        }
                        if (assembly != null)
                        {
                            assemblyDic[name] = assembly;
                            return assembly;
                        }
                    }
                    catch { }
                }

                string filename = name;
                bool isExists = File.Exists(filename);
                if (!isExists && File.Exists(filename + ".dll"))
                {
                    isExists = true;
                    filename = filename + ".dll";
                }
                else if (!isExists && File.Exists(filename + ".exe"))
                {
                    isExists = true;
                    filename = filename + ".exe";
                }

                if (!isExists)
                {
                    var s = this.GetFileFullPath(name);
                    if (File.Exists(s))
                    {
                        isExists = true;
                        filename = s;
                    }
                    if (!name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        s = this.GetFileFullPath(name + ".dll");
                        if (File.Exists(s))
                        {
                            isExists = true;
                            filename = s;
                        }
                    }
                    else if (!name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        s = this.GetFileFullPath(name + ".exe");
                        if (File.Exists(s))
                        {
                            isExists = true;
                            filename = s;
                        }
                    }
                }

                if (isExists)
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(filename);
                        assemblyDic[name] = assembly;
                    }
                    catch { }
                }
            }

            return assembly;
        }

        /// <summary>
        /// 加载cmd 对于contoller
        /// </summary>
        /// <param name="configFile"></param>
        public virtual void LoadCmdMethod(string configFile)
        {
            if (string.IsNullOrEmpty(configFile)) throw new ArgumentNullException("configFile");
            string fullpath = this.GetFileFullPath(configFile);

            if (!File.Exists(fullpath)) throw new FileNotFoundException(configFile + " not found!", configFile);

            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings xmlStting = new XmlReaderSettings();
            xmlStting.IgnoreComments = true;
            xmlStting.CloseInput = true;
            using (var xmlRead = XmlReader.Create(fullpath, xmlStting))
            {
                xmlDoc.Load(xmlRead);
                xmlRead.Close();
            }
            if (xmlDoc.ChildNodes.Count > 0)
            {
                var rootElement = xmlDoc.DocumentElement;
                if (rootElement != null)
                {
                    XmlElement nodeElement = rootElement["Global"];
                    if (nodeElement != null)
                    {
                        this.LoadGlobal(nodeElement);
                    }
                    nodeElement = rootElement["Controller"];
                    if (nodeElement != null)
                    {
                        this.LoadController(nodeElement);
                    }
                }
            }
        }

        private readonly string CurrentDirectory;
        private Assembly m_defaultAssembly;

        private Type GetAuth(XmlElement nodeElement)
        {
            Type type = null;
            var s = nodeElement.GetAttribute("type");
            if (string.IsNullOrEmpty(s)) return type;
            var arr = s.Split(',');
            var t = arr[0].Trim();
            Assembly assembly = null;
            if (arr.Length > 1 && !string.IsNullOrEmpty(arr[1].Trim()))
            {
                assembly = this.GetAssembly(arr[1].Trim());
            }
            else
            {
                assembly = this.m_defaultAssembly;
            }

            if (assembly != null)
            {
                var _type = assembly.GetType(t, false);
                var iarr = _type.GetInterfaces();
                var aut = typeof(IAuthorize);
                foreach (var it in iarr)
                {
                    if (it == aut)
                    {
                        type = _type;
                        break;
                    }
                }
            }

            return type;
        }

        private void LoadGlobal(XmlElement nodeElement)
        {
           var s = nodeElement.GetAttribute("defaultAssembly");
            if (!string.IsNullOrEmpty(s))
            {
                this.m_defaultAssembly = this.GetAssembly(s);
            }

            foreach (XmlElement aopElement in nodeElement)
            {
                if (aopElement.Name == "Authorize")
                {
                    var type = this.GetAuth(aopElement);
                    if (type != null && type.IsClass && !type.IsAbstract)
                    {
                        this.GlobalAuthTypeList.Add(type);
                    }
                }
            }
        }

        private void LoadController(XmlElement nodeElement)
        {
            Type baseControllerType = typeof(Controller);
            foreach (XmlElement classElement in nodeElement)
            {
                if (classElement.Name == "Action")
                {
                    var s = classElement.GetAttribute("cmd");
                    if (string.IsNullOrEmpty(s)) continue;
                    int cmd = 0;
                    if (!int.TryParse(s, out cmd)) continue;
                    var method = classElement.GetAttribute("method");
                    if (string.IsNullOrEmpty(method)) continue;
                    s = classElement.GetAttribute("type");
                    if (string.IsNullOrEmpty(s)) continue;
                    var arr = s.Split(',');
                    var t = arr[0].Trim();
                    Assembly assembly = null;
                    if (arr.Length > 1 && !string.IsNullOrEmpty(arr[1].Trim()))
                    {
                        assembly = this.GetAssembly(arr[1].Trim());
                    }
                    else
                    {
                        assembly = this.m_defaultAssembly;
                    }

                    if (assembly == null) continue;

                    var type = assembly.GetType(t, false);
                    if (type == null) throw new TypeLoadException(t + " not found!");
                    if (!type.IsClass) throw new TypeLoadException(t + " is not class!");
                    if(!type.IsSubclassOf(baseControllerType)) throw new TypeLoadException(t + " is not Controller!");
                    if (type.IsAbstract) throw new TypeLoadException(t + " is abstract!");
                    var methodInfo = type.GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
                    if (methodInfo == null) throw new MethodAccessException(t + ", Method: " + method + " not found!");
                    var parameterinfos = methodInfo.GetParameters();
                    if (!(parameterinfos.Length == 0 || (parameterinfos.Length == 1 && !parameterinfos[0].IsOut)))
                    {
                        throw new MethodAccessException(t + ", Method: " + method + " parameter is error!");
                    }
                    CmdMethodInfo m = new CmdMethodInfo()
                    {
                        Type = type,
                        Method = methodInfo,
                        ParameterType = parameterinfos.Length > 0 ? parameterinfos[0].ParameterType : null,
                        Cmd = cmd,
                        NoAuth = false
                    };
                    var attrs = type.GetCustomAttributes(typeof(AllowAnonymousAttribute), false);
                    if (attrs == null || attrs.Length == 0)
                    {
                        attrs = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), false);
                    }
                    m.NoAuth = attrs != null && attrs.Length > 0;
                    this.cmdCallbackDic[cmd] = m;

                    if (m.NoAuth)
                    {
                        m.AuthTypeList = new List<Type>(0);
                    }
                    else
                    {
                        var tlist = new List<Type>();
                        foreach (XmlElement authElement in classElement)
                        {
                            if (authElement.Name == "Authorize")
                            {
                                var authtype = GetAuth(authElement);
                                if (authtype != null && authtype.IsClass && !authtype.IsAbstract)
                                    tlist.Add(authtype);
                            }
                        }
                        m.AuthTypeList = new List<Type>(tlist.Count);
                        m.AuthTypeList.AddRange(tlist);
                    }
                }
            }
        }

    }
}
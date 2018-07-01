using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

using Afx.Internal;

namespace Afx.Utils
{
    /// <summary>
    /// 读取硬件序列号
    /// </summary>
    public static class SystemUtils
    {
        const uint PROCESSOR_ARCHITECTURE_AMD64 = 9;
        [DllImport("kernel32.dll")]
        private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        /// <summary>
        /// 获取当前机器内存状态
        /// </summary>
        /// <returns></returns>
        public static MemoryStatusModel GetMemoryStatus()
        {
            MemoryStatusModel m = null;
            MEMORYSTATUSEX vm = new MEMORYSTATUSEX();
            vm.dwLength = Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            if(GlobalMemoryStatusEx(ref vm))
            {
                m = new MemoryStatusModel()
                {
                    MemoryLoad = vm.dwMemoryLoad,
                    AvailPageFile = vm.ullAvailPageFile,
                    AvailPhys = vm.ullAvailPhys,
                    AvailVirtual = vm.ullAvailVirtual,
                    TotalPageFile = vm.ullTotalPageFile,
                    TotalPhys = vm.ullTotalPhys,
                    TotalVirtual = vm.ullTotalVirtual
                };
            }

            return m;
        }
        
        [DllImport("kernel32.dll")]
        private static extern bool GetVolumeInformation(
            string rootPathName,
            StringBuilder volumeNameBuffer,
            int volumeNameSize,
            ref uint volumeSerialNumber,
            ref uint maximumComponentLength,
            ref uint fileSystemFlags,
            StringBuilder fileSystemNameBuffer,
            int fileSystemNameSize
            );

        /// <summary>
        /// 获取指定盘符序列号
        /// </summary>
        /// <param name="rootPathName">盘符,如：C:\</param>
        /// <returns></returns>
        public static VolumeInfoModel GetVolumeInformation(string rootPathName)
        {
            VolumeInfoModel m = null;
            const int MAX_FILENAME_LEN = 256;
            uint volumeSerialNumber = 0;
            uint maximumComponentLength = 0;
            uint fileSystemFlags = 0;
            StringBuilder volumeNameBuffer = new StringBuilder(MAX_FILENAME_LEN);
            StringBuilder fileSystemNameBuffer = new StringBuilder(MAX_FILENAME_LEN);

            if (GetVolumeInformation(rootPathName, volumeNameBuffer, MAX_FILENAME_LEN,
                ref volumeSerialNumber, ref maximumComponentLength, ref fileSystemFlags,
                fileSystemNameBuffer, MAX_FILENAME_LEN))
            {
                m = new VolumeInfoModel()
                {
                    SerialNumber = volumeSerialNumber,
                    FileSystemFlags = fileSystemFlags,
                    FileSystemName = fileSystemNameBuffer.ToString()
                };
            }

            return m;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetDiskFreeSpaceEx(
            string rootPathName,
            ref ulong freeBytesAvailableToCaller,
            ref ulong totalNumberOfBytes,
            ref ulong totalNumberOfFreeBytes
            );

        /// <summary>
        /// 获取指定盘符存储信息
        /// </summary>
        /// <param name="rootPathName">盘符,如：C:\</param>
        /// <returns></returns>
        public static DiskFreeSpaceModel GetDiskFreeSpace(string rootPathName)
        {
            DiskFreeSpaceModel m = null;
            ulong freeBytesAvailableToCaller = 0;
            ulong totalNumberOfBytes = 0;
            ulong totalNumberOfFreeBytes = 0;
            if (GetDiskFreeSpaceEx(rootPathName, ref freeBytesAvailableToCaller, ref totalNumberOfBytes, ref totalNumberOfFreeBytes))
            {
                m = new DiskFreeSpaceModel()
                {
                    Name = rootPathName,
                    AvailableFreeSpace = freeBytesAvailableToCaller,
                    TotalSize = totalNumberOfBytes,
                    TotalFreeSpace = totalNumberOfFreeBytes
                };
            }

            return m;
        }
        
        [DllImport("kernel32.dll")]
        private static extern bool GetVersionEx(ref OSVERSIONINFOEX lpVersionInfo);

        const int SM_SERVERR2 = 89;
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        const uint VER_NT_SERVER = 0x0000003;
        const uint VER_NT_DOMAIN_CONTROLLER = 0x0000002;
        const uint VER_NT_WORKSTATION = 0x0000001;
        const uint VER_SUITE_WH_SERVER = 0x00008000;
        private static string GetOSName(ref OSVERSIONINFOEX vm)
        {
            string name = null;
            switch (vm.dwMajorVersion)
            {
                case 10:
                    if (vm.wProductType == VER_NT_WORKSTATION)
                    {
                        name = "Windows 10";
                    }
                    else
                    {
                        name = "Windows Server 2016 Technical Preview";
                    }
                    break;
                case 6:
                    switch (vm.dwMinorVersion)
                    {
                        case 3:
                            if (vm.wProductType == VER_NT_WORKSTATION)
                            {
                                name = "Windows 8.1";
                            }
                            else
                            {
                                name = "Windows Server 2012 R2";
                            }
                            break;
                        case 2:
                            if (vm.wProductType == VER_NT_WORKSTATION)
                            {
                                name = "Windows 8";
                            }
                            else
                            {
                                name = "Windows Server 2012";
                            }
                            break;
                        case 1:
                            if (vm.wProductType == VER_NT_WORKSTATION)
                            {
                                name = "Windows 7";
                            }
                            else
                            {
                                name = "Windows Server 2008 R2";
                            }
                            break;
                        case 0:
                            if (vm.wProductType == VER_NT_WORKSTATION)
                            {
                                name = "Windows Vista";
                            }
                            else
                            {
                                name = "Windows Server 2008";
                            }
                            break;
                    }
                    break;
                case 5:
                    switch (vm.dwMinorVersion)
                    {
                        case 2:
                            if (GetSystemMetrics(SM_SERVERR2) != 0)
                            {
                                name = "Windows Server 2003 R2";
                            }
                            else if ((vm.wSuiteMask & VER_SUITE_WH_SERVER) != 0)
                            {
                                name = "Windows Home Server";
                            }
                            else if (GetSystemMetrics(SM_SERVERR2) == 0)
                            {
                                name = "Windows Server 2003";
                            }
                            else if (vm.wProductType == VER_NT_WORKSTATION)
                            {
                                SYSTEM_INFO info = new SYSTEM_INFO();
                                GetNativeSystemInfo(ref info);
                                if (info.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
                                {
                                    name = "Windows XP Professional x64 Edition";
                                }
                            }
                            break;
                        case 1:
                            name = "Windows XP";
                            break;
                        case 0:
                            name = "Windows 2000";
                            break;
                    }
                    break;
            }

            if (name == null) name = string.Format("{0}.{1}.{2}", vm.dwMajorVersion, vm.dwMinorVersion, vm.dwBuildNumber);

            return name;
        }

        /// <summary>
        /// 获取当前操作系统信息
        /// </summary>
        /// <returns></returns>
        public static OSVersionInfoModel GetOSVersion()
        {
            OSVersionInfoModel m = null;
            OSVERSIONINFOEX vm = new OSVERSIONINFOEX();
            vm.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
            if (GetVersionEx(ref vm))
            {
                m = new OSVersionInfoModel()
                {
                    BuildNumber = vm.dwBuildNumber,
                    MajorVersion = vm.dwMajorVersion,
                    MinorVersion = vm.dwMinorVersion,
                    PlatformId = vm.dwPlatformId,
                    CSDVersion = vm.szCSDVersion,
                    ProductType = vm.wProductType,
                    Reserved = vm.wReserved,
                    ServicePackMajor = vm.wServicePackMajor,
                    ServicePackMinor = vm.wServicePackMinor,
                    SuiteMask = vm.wSuiteMask,
                    Name = GetOSName(ref vm)
                };
            }

            return m;
        }
        
        /// <summary>
        /// 获取本机所有ip
        /// </summary>
        /// <returns></returns>
        public static List<string> GetIPAddress()
        {
            List<string> list = new List<string>();
            try
            {
                NetworkInterface[] arr = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var net in arr)
                {
                    if (net.OperationalStatus == OperationalStatus.Up
                        && net.Speed > 0
                        && net.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var ipProperties = net.GetIPProperties();
                        foreach (UnicastIPAddressInformation unicast in ipProperties.UnicastAddresses)
                        {
                            var ip = unicast.Address;
                            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                            {
                                string s = ip.ToString();
                                if (!list.Contains(s))
                                {
                                    list.Add(s);
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return list;
        }

        /// <summary>
        /// 获取本机所有Mac
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMac()
        {
            List<string> list = new List<string>();
            try
            {
                NetworkInterface[] arr = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var net in arr)
                {
                    if (net.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var phy = net.GetPhysicalAddress();
                        string mac = phy.ToString();
                        if (!string.IsNullOrEmpty(mac) && mac.Length == 12 && !list.Contains(mac))
                        {
                            list.Add(mac);
                        }
                    }
                }
            }
            catch { }

            return list;
        }

        /// <summary>
        /// 设置本地asp_status服务允许外部访问
        /// </summary>
        /// <returns></returns>
        public static bool SetAllowRemoteSession()
        {
            bool result = false;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\aspnet_state\Parameters", true))
                {
                    object obj = key.GetValue("AllowRemoteConnection");
                    int temp = 0;
                    if (obj == null || !int.TryParse(obj.ToString(), out temp) || temp == 0)
                    {
                        key.SetValue("AllowRemoteConnection", 1, RegistryValueKind.DWord);
                        key.Flush();
                        key.Close();
                    }
                    result = true;
                }
            }
            catch {  }

            return result;
        }

        /// <summary>
        /// 设置本机tcp port可以使用到最大
        /// </summary>
        /// <returns></returns>
        public static bool SetTcpMaxUserPort()
        {
            bool result = false;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\Tcpip\\Parameters", true))
                {
                    object obj = key.GetValue("MaxUserPort");
                    int maxUserPort = 65534;
                    int temp = 0;
                    if (obj == null || !int.TryParse(obj.ToString(), out temp) || temp < maxUserPort)
                    {
                        key.SetValue("MaxUserPort", maxUserPort, RegistryValueKind.DWord);
                        key.SetValue("TcpTimedWaitDelay", 30, RegistryValueKind.DWord);
                        key.Flush();
                        key.Close();
                    }
                    result = true;
                }
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// ping 指定 host 或 ip
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool Ping(string ipOrHost, int timeout = 500)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(ipOrHost))
            {
                try
                {
                    using (var ping = new System.Net.NetworkInformation.Ping())
                    {
                        var dip = ping.Send(ipOrHost, timeout);
                        if (dip != null && dip.Status == System.Net.NetworkInformation.IPStatus.Success)
                        {
                            result = true;
                        }
                    }
                }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// ping ip[]
        /// </summary>
        /// <param name="ips"></param>
        /// <param name="timeout"></param>
        /// <returns>可以ping的ip</returns>
        public static string GetValidIpAddress(string[] ips, int timeout = 500)
        {
            string ip = null;
            if (ips != null && ips.Length > 1)
            {
                foreach (var s in ips)
                {
                    if (Ping(s, timeout))
                    {
                        ip = s;
                    }
                }
            }

            return ip;
        }

        /// <summary>
        /// 根据指定远程服务，获取本机使用的ip
        /// </summary>
        /// <param name="remoteIP"></param>
        /// <param name="remotePort"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string GetLocalUsedIpAddress(string remoteIP, int remotePort, int timeout = 3000)
        {
            string localIP = null;
            try
            {
                using (var maual = new System.Threading.ManualResetEvent(false))
                {
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        var asy = socket.BeginConnect(remoteIP, remotePort, (o) =>
                        {
                            try
                            {
                                if(socket.Connected)
                                {
                                    string[] arr = socket.LocalEndPoint.ToString().Split(':');
                                    if (arr.Length == 2) localIP = arr[0];
                                }
                            }
                            catch { }
                            try { maual.Set(); }
                            catch { }
                        }, maual);
                        
                        maual.WaitOne(timeout);
                        socket.Close();
                    }
                }
            }
            catch { }

            return localIP;
        }

        /// <summary>
        /// 获取机器序列号
        /// </summary>
        /// <param name="appId">序列号标识</param>
        /// <param name="logcall">日志回调</param>
        /// <returns>序列号</returns>
        public static string GetSerialNumber(string appId = null, Action<string> logcall = null)
        {
            if (string.IsNullOrEmpty(appId) || appId.ToLower() == HD_INFO_KEY.ToLower()) appId = DEFAULT_ID_KEY;
            string hdInfo = GetHDInfo(logcall);
            string id = GetMachineID(appId, hdInfo);
            if(string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("n").ToUpper();
                SetMachineID(appId, id, hdInfo);
            }

            OnLogcall( "appId: " + id, logcall);

            return id;
        }

        private static void OnLogcall(string s, Action<string> logcall)
        {
            if (logcall != null)
            {
                try { logcall(s); }
                catch { }
            }
        }

        const string REG_BIOS_PATH = @"HARDWARE\DESCRIPTION\System\BIOS";
        const string REG_PROC_PATH = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
        private static string GetHDInfo(Action<string> logcall = null)
        {
            StringBuilder str = new StringBuilder();
            str.Append("{");

            using (var regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REG_BIOS_PATH))
            {
                if (regkey != null)
                {
                    string v = regkey.GetValue("BIOSVendor", null) as string;
                    str.AppendFormat(" BIOSVendor: \"{0}\",", v ?? "");

                    v = regkey.GetValue("SystemVersion", null) as string;
                    str.AppendFormat(" BIOSSystemVersion: \"{0}\",", v ?? "");

                    v = regkey.GetValue("BIOSVersion", null) as string;
                    str.AppendFormat(" BIOSVersion: \"{0}\",", v ?? "");
                }
            }

            using (var regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REG_PROC_PATH))
            {
                if (regkey != null)
                {
                    string v = regkey.GetValue("ProcessorNameString", null) as string;
                    str.AppendFormat(" ProcessorName: \"{0}\",", v ?? "");

                    v = regkey.GetValue("Identifier", null) as string;
                    str.AppendFormat(" ProcessorIdentifier: \"{0}\",", v ?? "");
                }
            }

            var sys = new SYSTEM_INFO();
            GetNativeSystemInfo(ref sys);
            str.AppendFormat(" ProcessorType: {0}, ProcessorLevel: {1}, ProcessorRevision: {2}, NumberOfProcessors: {3}, ActiveProcessorMask: {4}, ProcessorArchitecture: {5},", sys.dwProcessorType, sys.wProcessorLevel, sys.wProcessorRevision, sys.dwNumberOfProcessors, sys.dwActiveProcessorMask, sys.wProcessorArchitecture);

            string rootPathName = Path.GetPathRoot(Environment.SystemDirectory);
            str.AppendFormat(" RootPathName: \"{0}\",", rootPathName);

            var disk = GetDiskFreeSpace(rootPathName);
            if (disk != null)
            {
                str.AppendFormat(" TotalNumberOfBytes: {0},", disk.TotalSize);
            }

            var volumeInfo = GetVolumeInformation(rootPathName);
            if (volumeInfo != null)
            {
                str.AppendFormat(" VolumeSerialNumber: {0}, FileSystemFlags: {1}, FileSystemName: \"{2}\"", volumeInfo.SerialNumber, volumeInfo.FileSystemFlags, volumeInfo.FileSystemName ?? "");
            }

            str.Append(" }");

            OnLogcall(str.ToString(), logcall);

            string s = Md5Utils.GetMd5Hash(str.ToString());

            if (string.IsNullOrEmpty(s))
            {
                string error = "需要重启计算机!";
                OnLogcall(error, logcall);
                throw new Exception(error);
            }
            else
            {
                OnLogcall("HDInfo: " + s, logcall);
            }

            return s;
        }

        const string MyDevice_DES_KEY = "My@Csv+$";
        const string DEFAULT_ID_KEY = "ID";
        const string HD_INFO_KEY = "HDInfo";
        const string REG_PATH = @"SOFTWARE\MyDevice";
        private static string GetMachineID(string appId, string hdInfo)
        {
            string id = null;
            try
            {
                using (var regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REG_PATH))
                {
                    if (regkey != null)
                    {
                        string reg_hdInfo = (string)regkey.GetValue(HD_INFO_KEY, null);
                        if (!string.IsNullOrEmpty(reg_hdInfo)) reg_hdInfo = DesUtils.Decrypt(reg_hdInfo, MyDevice_DES_KEY);
                        if (reg_hdInfo == hdInfo)
                        {
                            id = (string)regkey.GetValue(appId, null);
                            if (!string.IsNullOrEmpty(id))
                            {
                                string deskey = hdInfo.Substring(4, 4) + hdInfo.Substring(12, 4);
                                id = DesUtils.Decrypt(id, deskey);
                            }
                        }
                        regkey.Close();
                    }
                }
            }
            catch { }

            return id;
        }

        private static void SetMachineID(string appId, string id, string hdInfo)
        {
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    using (var regkey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(REG_PATH))
                    {
                        string s = DesUtils.Encrypt(hdInfo, MyDevice_DES_KEY);
                        regkey.SetValue(HD_INFO_KEY, s, Microsoft.Win32.RegistryValueKind.String);
    
                        string deskey = hdInfo.Substring(4, 4) + hdInfo.Substring(12, 4);
                        s = DesUtils.Encrypt(id, deskey);
                        regkey.SetValue(appId, s, Microsoft.Win32.RegistryValueKind.String);

                        regkey.Flush();
                        regkey.Close();
                    }
                }
                catch { }
            }
        }
    }
}
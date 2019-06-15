using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Sockets.Common
{
    /// <summary>
    /// SocketHelper
    /// </summary>
    public static class SocketHelper
    {
        /// <summary>
        /// 数据包头长度
        /// </summary>
        public const int PREFIX_LENGTH = 4;
        /// <summary>
        /// 最大数据包10MB
        /// </summary>
        public const int MAX_PREFIX_LENGTH = 1024 * 1024 * 10;
        /// <summary>
        /// 数据长度byte[] 转换int
        /// </summary>
        /// <param name="prefixBytes">数据长度byte[]</param>
        /// <returns>数据长度</returns>
        public static int ToPrefixLength(byte[] prefixBytes)
        {
            int length = 0;
            if (prefixBytes != null && prefixBytes.Length == 4)
            {
                Array.Reverse(prefixBytes);
                length = BitConverter.ToInt32(prefixBytes, 0);
            }

            return length;
        }
        /// <summary>
        /// int 转换 byte[]
        /// </summary>
        /// <param name="length"></param>
        /// <returns>byte[]</returns>
        public static byte[] ToPrefixBytes(int length)
        {
            byte[] prefixBytes = BitConverter.GetBytes(length);
            Array.Reverse(prefixBytes);
            return prefixBytes;
        }
        /// <summary>
        /// 获取TcpKeepAlive byte[]
        /// </summary>
        /// <param name="keepAliveTime">keepAliveTime</param>
        /// <param name="keepAliveInterval">keepAliveInterval</param>
        /// <returns>byte[]</returns>
        public static byte[] GetTcpKeepAlive(int keepAliveTime, int keepAliveInterval)
        {
            List<byte> inOptionValues = new List<byte>(3 * sizeof(int));
            inOptionValues.AddRange(BitConverter.GetBytes(1u));
            inOptionValues.AddRange(BitConverter.GetBytes(keepAliveTime));
            inOptionValues.AddRange(BitConverter.GetBytes(keepAliveInterval));

            return inOptionValues.ToArray();
        }
        /// <summary>
        /// 获取发送数据的长度byte[] 与数据组成的新byte[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ToSendData(byte[] data)
        {
            byte[] result = null;
            if(data != null && data.Length > 0)
            {
                result = new byte[4 + data.Length];
                byte[] prefixBytes = SocketHelper.ToPrefixBytes(data.Length);
                Array.Copy(prefixBytes, result, prefixBytes.Length);
                Array.Copy(data, 0, result, prefixBytes.Length, data.Length);
            }

            return result;
        }

        public static bool IsWindows()
        {
            bool result = false;
            switch(Environment.OSVersion.Platform)
            {
#if !NETCOREAPP
                case PlatformID.WinCE:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
#endif
                case PlatformID.Win32NT:
                    result = true;
                    break;
            }
            return result;
        }
    }
}

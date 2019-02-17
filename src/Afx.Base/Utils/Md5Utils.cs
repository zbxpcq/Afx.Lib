using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// Md5 Utils
    /// </summary>
    public static class Md5Utils
    {
        /// <summary>
        /// 获取MD5值
        /// </summary>
        public static string GetMd5Hash(string input)
        {
            if (input == null) throw new ArgumentNullException("input");
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] buffer = GetMd5Hash(data);
            var result = StringUtils.ByteToHexString(buffer);

            return result;
        }

        /// <summary>
        /// 获取MD5值
        /// </summary>
        public static byte[] GetMd5Hash(byte[] input)
        {
            if (input == null) throw new ArgumentNullException("input");
            byte[] result = null;
            if (input.Length > 0)
            {
                try
                {
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    {
                        result = md5.ComputeHash(input);
                    }
                }
                catch (Exception ex)
                {
#if !NETCOREAPP && !NETSTANDARD
                    SetFipsAlgorithmPolicy();
#endif
                    throw ex;
                }
            }
            else
            {
                result = new byte[0];
            }

            return result;
        }

#if !NETCOREAPP && !NETSTANDARD
        /// <summary>
        /// 设置安全策略
        /// </summary>
        public static void SetFipsAlgorithmPolicy()
        {
            try
            {
               using(var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa", true))
               {
                   key.SetValue("FipsAlgorithmPolicy", 0, RegistryValueKind.DWord);
                   key.Flush();
                   key.Close();
               }

            }
            catch { }
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa\FipsAlgorithmPolicy", true))
                {
                    key.SetValue("Enabled", 0, RegistryValueKind.DWord);
                    key.Flush();
                    key.Close();
                }

            }
            catch { }
        }
#endif

    }
}

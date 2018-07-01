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
            string result = null;
            try
            {
                if (input != null)
                {
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(input);
                        byte[] buffer = md5.ComputeHash(data);
                        result = StringUtils.ByteToHexString(buffer);
                    }
                }
            }
            catch(Exception ex)
            {
#if !NETCOREAPP && !NETSTANDARD
                SetFipsAlgorithmPolicy();
#endif
                throw ex;
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

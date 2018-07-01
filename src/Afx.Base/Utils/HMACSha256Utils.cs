using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// HMACSha256 哈希值
    /// </summary>
    public static class HMACSha256Utils
    {
        /// <summary>
        /// HMACSha256 哈希值
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] GetHash(byte[] input, byte[] key)
        {
            byte[] buffer = null;
            if(input != null && input.Length> 0 && key != null && key.Length > 0)
            {
                using(var sha = new HMACSHA256(key))
                {
                    buffer = sha.ComputeHash(input);
                }
            }

            return buffer;
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetHash(string input, string key)
        {
            string result = null;
            if (input != null && !string.IsNullOrEmpty(key))
            {
                byte[] keybytes = Encoding.UTF8.GetBytes(key);
                using (var sha = new HMACSHA256(keybytes))
                {
                    byte[] buffer = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                    result = StringUtils.ByteToHexString(buffer);
                }
            }

            return result;
        }
    }
}

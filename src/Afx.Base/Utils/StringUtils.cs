using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// 字符串 Utils
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// 将字符串转换base64字符串
        /// </summary>
        /// <param name="input">字符串</param>
        /// <returns></returns>
        public static string StringToBase64(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);

            return Convert.ToBase64String(data, Base64FormattingOptions.None);
        }

        /// <summary>
        /// 将base64字符串转换字符串
        /// </summary>
        /// <param name="input">base64字符串</param>
        /// <returns></returns>
        public static string Base64ToString(string input)
        {
            byte[] data = Convert.FromBase64String(input);

            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// byte[] 转换成 十六进制 string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ByteToHexString(byte[] input)
        {
            string result = null;
            if (input != null && input.Length > 0)
            {
                StringBuilder str = new StringBuilder(input.Length * 2);
                foreach (var b in input)
                {
                    str.Append(b.ToString("X2"));
                }
                result = str.ToString();
            }

            return result;
        }

        /// <summary>
        /// 十六进制 string 转换成 byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] HexStringToByte(string input)
        {
            byte[] result = null;
            if (!string.IsNullOrEmpty(input) && input.Length % 2 == 0)
            {
                try
                {
                    byte[] outputdata = new byte[input.Length / 2];
                    int i = 0, j = 0;
                    while (i < input.Length)
                    {
                        string s = input.Substring(i, 2);
                        outputdata[j] = Convert.ToByte(s, 16);
                        i += 2;
                        j++;
                    }

                    result = outputdata;
                }
                catch { }
            }

            return result;
        }

        const string _allChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
        /// <summary>
        /// 获取随机字符串(a-z,A-Z,0-9)
        /// </summary>
        /// <param name="count">字符串个数</param>
        /// <returns></returns>
        public static string GetRandomString(int count = 8)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            char[] chars = new char[count];
            for (int i = 0; i < count; i++)
            {
                int index = (random.Next(1, short.MaxValue) << random.Next(1, 5)) % _allChars.Length;
                chars[i] = _allChars[index];
            }

            return new String(chars);
        }
    }
}

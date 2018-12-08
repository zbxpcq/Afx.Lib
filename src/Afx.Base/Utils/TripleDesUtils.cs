using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// 3DES 加密、解密
    /// </summary>
    public static class TripleDesUtils
    {
        /// <summary>
        /// 加密、解密默认 key 24位
        /// </summary>
        private const string DefaultKey = "8fa1ad9983c64a4e8210ee6d";
        /// <summary>
        /// 加密、解密默认 CipherMode
        /// </summary>
        public const CipherMode DefaultMode = CipherMode.CBC;
        /// <summary>
        /// 加密、解密默认CipherMode
        /// </summary>
        public const PaddingMode DefaultPadding = PaddingMode.PKCS7;

        /// <summary>
        /// 生成 24 个ASCII字符的 des key
        /// </summary>
        /// <returns>24 个ASCII字符</returns>
        public static string CreateKey()
        {
            return StringUtils.GetRandomString(24);
        }

        /// <summary>
        /// 生成 8 个ASCII字符的 des iv
        /// </summary>
        /// <returns>8 个ASCII字符</returns>
        public static string CreateIV()
        {
            return StringUtils.GetRandomString(8);
        }

        #region bytes
        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <returns>加密成功返回byte[]</returns>
        public static byte[] Encrypt(byte[] input)
        {
            return Encrypt(input, DefaultKey);
        }

        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <returns>加密成功返回byte[]</returns>
        public static byte[] Encrypt(byte[] input, string key)
        {
            return Encrypt(input, key, null, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <returns>加密成功返回byte[]</returns>
        public static byte[] Encrypt(byte[] input, string key, string iv)
        {
            return Encrypt(input, key, iv, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>加密成功返回byte[]</returns>
        public static byte[] Encrypt(byte[] input, string key, CipherMode mode, PaddingMode padding)
        {
            return Encrypt(input, key, null, mode, padding);
        }

        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>加密成功返回byte[]</returns>
        public static byte[] Encrypt(byte[] input, string key, string iv, CipherMode mode, PaddingMode padding)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length != 24) throw new ArgumentException("key.Length is error", "key");
            byte[] ivBytes = null;
            if (!string.IsNullOrEmpty(iv))
            {
                ivBytes = Encoding.ASCII.GetBytes(iv);
                if (ivBytes.Length != 8) throw new ArgumentException("iv.Length is error!", "iv");
            }
            byte[] output = null;
            if (input != null && input.Length > 0)
            {
                using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
                {
                    des.Mode = mode;
                    des.Padding = padding;
                    des.Key = keyBytes;
                    if (ivBytes != null) des.IV = ivBytes;
                    else des.GenerateIV();
                    using (ICryptoTransform cryptoTransform = des.CreateEncryptor())
                    {
                        byte[] buffer = cryptoTransform.TransformFinalBlock(input, 0, input.Length);
                        if (ivBytes == null)
                        {
                            output = new byte[buffer.Length + des.IV.Length];
                            Array.Copy(buffer, 0, output, 0, buffer.Length);
                            Array.Copy(des.IV, 0, output, output.Length - des.IV.Length, des.IV.Length);
                        }
                        else
                        {
                            output = buffer;
                        }
                    }
                }
            }
            else if (input != null && input.Length == 0)
            {
                output = new byte[0];
            }

            return output;
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <returns>解密成功返回byte[]</returns>
        public static byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, DefaultKey);
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <returns>解密成功返回byte[]</returns>
        public static byte[] Decrypt(byte[] input, string key)
        {
            return Decrypt(input, key, null, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <returns>解密成功返回byte[]</returns>
        public static byte[] Decrypt(byte[] input, string key, string iv)
        {
            return Decrypt(input, key, iv, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>解密成功返回byte[]</returns>
        public static byte[] Decrypt(byte[] input, string key, CipherMode mode, PaddingMode padding)
        {
            return Decrypt(input, key, null, mode, padding);
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>解密成功返回byte[]</returns>
        public static byte[] Decrypt(byte[] input, string key, string iv, CipherMode mode, PaddingMode padding)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length != 24) throw new ArgumentException("key.Length is error!", "key");
            byte[] ivBytes = null;
            if (!string.IsNullOrEmpty(iv))
            {
                ivBytes = Encoding.ASCII.GetBytes(iv);
                if (ivBytes.Length != 8) throw new ArgumentException("iv.Length is error!", "iv");
            }
            else if (input != null && 0 < input.Length && input.Length <= 8)
            {
                throw new ArgumentException("input.Length is error!", "input");
            }
            byte[] output = null;
            if (input != null && input.Length > 0)
            {
                using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
                {
                    des.Mode = mode;
                    des.Padding = padding;
                    des.Key = keyBytes;
                    if (ivBytes != null)
                    {
                        des.IV = ivBytes;
                    }
                    else
                    {
                        var _iv = new byte[8];
                        Array.Copy(input, input.Length - _iv.Length, _iv, 0, _iv.Length);
                        des.IV = _iv;
                    }
                    using (ICryptoTransform cryptoTransform = des.CreateDecryptor())
                    {
                        output = cryptoTransform.TransformFinalBlock(input, 0, ivBytes != null ? input.Length : input.Length - des.IV.Length);
                    }
                }
            }
            else if (input != null && input.Length == 0)
            {
                output = new byte[0];
            }

            return output;
        }
        #endregion

        #region string
        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input)
        {
            return Encrypt(input, DefaultKey);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, string key)
        {
            return Encrypt(input, key, null, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, string key, string iv)
        {
            return Encrypt(input, key, iv, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, string key, CipherMode mode, PaddingMode padding)
        {
            return Encrypt(input, key, null, mode, padding);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, string key, string iv, CipherMode mode, PaddingMode padding)
        {
            string output = null;
            if (!string.IsNullOrEmpty(input))
            {
                byte[] inputdata = Encoding.UTF8.GetBytes(input);
                byte[] outputdata = Encrypt(inputdata, key, iv, mode, padding);
                output = StringUtils.ByteToHexString(outputdata);
            }
            else if (input == string.Empty)
            {
                output = string.Empty;
            }

            return output;
        }


        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input)
        {
            return Decrypt(input, DefaultKey);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, string key)
        {
            return Decrypt(input, key, null, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, string key, string iv)
        {
            return Decrypt(input, key, iv, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, string key, CipherMode mode, PaddingMode padding)
        {
            return Decrypt(input, key, null, mode, padding);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8 个ASCII字符 iv</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, string key, string iv, CipherMode mode, PaddingMode padding)
        {
            string output = null;
            byte[] inputdata = StringUtils.HexStringToByte(input);
            if (inputdata != null && inputdata.Length > 0)
            {
                byte[] outputdata = Decrypt(inputdata, key, iv, mode, padding);
                if (outputdata != null && outputdata.Length > 0)
                {
                    output = Encoding.UTF8.GetString(outputdata);
                }
            }
            else if (input == string.Empty)
            {
                output = string.Empty;
            }

            return output;
        }
        #endregion

        #region Stream
        /// <summary>
        /// 加密Stream
        /// </summary>
        /// <param name="inputStream">要加密Stream</param>
        /// <param name="outputStream">加密输出Stream</param>
        /// <returns>是否成功</returns>
        public static bool Encrypt(Stream inputStream, Stream outputStream)
        {
            return Encrypt(inputStream, outputStream, DefaultKey);
        }

        /// <summary>
        /// 加密Stream
        /// </summary>
        /// <param name="inputStream">要加密Stream</param>
        /// <param name="outputStream">加密输出Stream</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <returns>是否成功</returns>
        public static bool Encrypt(Stream inputStream, Stream outputStream, string key)
        {
            return Encrypt(inputStream, outputStream, key, null, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密Stream
        /// </summary>
        /// <param name="inputStream">要加密Stream</param>
        /// <param name="outputStream">加密输出Stream</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8 个ASCII字符 iv</param>
        /// <returns>是否成功</returns>
        public static bool Encrypt(Stream inputStream, Stream outputStream, string key, string iv)
        {
            return Encrypt(inputStream, outputStream, key, iv, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密Stream
        /// </summary>
        /// <param name="inputStream">要加密Stream</param>
        /// <param name="outputStream">加密输出Stream</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8 个ASCII字符 iv</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>是否成功</returns>
        public static bool Encrypt(Stream inputStream, Stream outputStream, string key,
            CipherMode mode, PaddingMode padding)
        {
            return Encrypt(inputStream, outputStream, key, null, mode, padding);
        }

        /// <summary>
        /// 加密Stream
        /// </summary>
        /// <param name="inputStream">要加密Stream</param>
        /// <param name="outputStream">加密输出Stream</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>是否成功</returns>
        public static bool Encrypt(Stream inputStream, Stream outputStream, string key, string iv,
            CipherMode mode, PaddingMode padding)
        {
            if (inputStream == null) throw new ArgumentNullException("inputStream");
            if (!inputStream.CanRead) throw new ArgumentException("inputStream is not read!", "inputStream");
            if (outputStream == null) throw new ArgumentNullException("outputStream");
            if (!outputStream.CanWrite) throw new ArgumentException("outputStream is not write!", "outputStream");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length != 24) throw new ArgumentException("key.Length is error!", "key");
            byte[] ivBytes = null;
            if (!string.IsNullOrEmpty(iv))
            {
                ivBytes = Encoding.ASCII.GetBytes(iv);
                if (ivBytes.Length != 8) throw new ArgumentException("iv.Length is error!", "iv");
            }
            bool result = false;
            if (inputStream.Length > 0)
            {
                using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
                {
                    des.Mode = mode;
                    des.Padding = padding;
                    des.Key = keyBytes;
                    if (ivBytes != null) des.IV = ivBytes;
                    else des.GenerateIV();
                    using (var cryptoTransform = des.CreateEncryptor())
                    {
                        CryptoStream cryptoStream = new CryptoStream(outputStream, cryptoTransform, CryptoStreamMode.Write);
                        //using (CryptoStream cryptoStream = new CryptoStream(outputStream, cryptoTransform, CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[4 * 1024];
                            do
                            {
                                int readlength = buffer.Length;
                                long _count = inputStream.Length - inputStream.Position;
                                if (_count < readlength)
                                    readlength = (int)_count;
                                int len = inputStream.Read(buffer, 0, readlength);
                                cryptoStream.Write(buffer, 0, len);
                            }
                            while (inputStream.Position < inputStream.Length);

                            cryptoStream.FlushFinalBlock();
                            if (ivBytes == null) outputStream.Write(des.IV, 0, des.IV.Length);
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 解密Stream
        /// </summary>
        /// <param name="inputStream">要解密Stream</param>
        /// <param name="outputStream">解密成功Stream</param>
        /// <returns>是否成功</returns>
        public static bool Decrypt(Stream inputStream, Stream outputStream)
        {
            return Decrypt(inputStream, outputStream, DefaultKey);
        }

        /// <summary>
        /// 解密Stream
        /// </summary>
        /// <param name="inputStream">要解密Stream</param>
        /// <param name="outputStream">解密成功Stream</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <returns>是否成功</returns>
        public static bool Decrypt(Stream inputStream, Stream outputStream, string key)
        {
            return Decrypt(inputStream, outputStream, key, null, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密Stream
        /// </summary>
        /// <param name="inputStream">要解密Stream</param>
        /// <param name="outputStream">解密成功Stream</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">24个ASCII字符 iv</param>
        /// <returns>是否成功</returns>
        public static bool Decrypt(Stream inputStream, Stream outputStream, string key, string iv)
        {
            return Decrypt(inputStream, outputStream, key, iv, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密Stream
        /// </summary>
        /// <param name="inputStream">要解密Stream</param>
        /// <param name="outputStream">解密成功Stream</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>是否成功</returns>
        public static bool Decrypt(Stream inputStream, Stream outputStream, string key, string iv,
            CipherMode mode, PaddingMode padding)
        {
            if (inputStream == null) throw new ArgumentNullException("inputStream");
            if (!inputStream.CanRead) throw new ArgumentException("inputStream is not read!", "inputStream");
            if (outputStream == null) throw new ArgumentNullException("outputStream");
            if (!outputStream.CanWrite) throw new ArgumentException("outputStream is not write!", "outputStream");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length != 24) throw new ArgumentException("key.Length is error!", "key");
            byte[] ivBytes = null;
            if (!string.IsNullOrEmpty(iv))
            {
                ivBytes = Encoding.ASCII.GetBytes(iv);
                if (ivBytes.Length != 8) throw new ArgumentException("iv.Length is error!", "iv");
            }
            else if (0 < inputStream.Length)
            {
                if(inputStream.Length <= 8) throw new ArgumentException("inputStream.Length is error!", "inputStream");
                if(!inputStream.CanSeek) throw new ArgumentException("inputStream is not seek!", "inputStream");
            }
            bool result = false;
            if (inputStream.Length > 0)
            {
                using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
                {
                    des.Mode = mode;
                    des.Padding = padding;
                    des.Key = keyBytes;
                    if (ivBytes != null)
                    {
                        des.IV = ivBytes;
                    }
                    else
                    {
                        var _iv = new byte[8];
                        inputStream.Seek(inputStream.Length - _iv.Length, SeekOrigin.Begin);
                        inputStream.Read(_iv, 0, _iv.Length);
                        inputStream.Seek(0, SeekOrigin.Begin);
                        des.IV = _iv;
                    }
                    using (var cryptoTransform = des.CreateDecryptor())
                    {
                        CryptoStream cryptoStream = new CryptoStream(outputStream, cryptoTransform, CryptoStreamMode.Write);
                        //using (CryptoStream cryptoStream = new CryptoStream(outputStream, cryptoTransform, CryptoStreamMode.Write))
                        {
                            long endlength = inputStream.Length;
                            if (ivBytes == null) endlength = endlength - des.IV.Length;
                            byte[] buffer = new byte[4 * 1024];
                            do
                            {
                                int readlength = buffer.Length;
                                long _count = endlength - inputStream.Position;
                                if (_count < readlength)
                                    readlength = (int)_count;
                                int len = inputStream.Read(buffer, 0, readlength);
                                cryptoStream.Write(buffer, 0, len);
                            }
                            while (inputStream.Position < endlength);

                            cryptoStream.FlushFinalBlock();
                            result = true;
                        }
                    }
                }
            }

            return result;
        }
        #endregion
    }
}

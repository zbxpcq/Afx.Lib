using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// 3DES 加密、解密
    /// add by jerrylai@aliyun.com
    /// https://github.com/jerrylai/Afx.Lib
    /// </summary>
    public static class TripleDesUtils
    {
        /// <summary>
        /// 加密、解密默认 key 24位
        /// </summary>
        private const string DefaultKey = "8fa1ad9983c64a4e8210ee6d";
        private const string DefaultIV = "12345678";
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
            return Encrypt(input, DefaultKey, DefaultIV, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <returns>加密成功返回byte[]</returns>
        public static byte[] Encrypt(byte[] input, string key)
        {
            return Encrypt(input, key, DefaultIV, DefaultMode, DefaultPadding);
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

        public static byte[] Encrypt(byte[] input, byte[] key, byte[] iv)
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
            return Encrypt(input, key, DefaultIV, mode, padding);
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
            if (string.IsNullOrEmpty(iv)) throw new ArgumentNullException("iv");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] ivBytes = Encoding.ASCII.GetBytes(iv);
            byte[] output = Encrypt(input, keyBytes, ivBytes, mode, padding);

            return output;
        }

        public static byte[] Encrypt(byte[] input, byte[] key, byte[] iv, CipherMode mode, PaddingMode padding)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (iv == null) throw new ArgumentNullException("iv");
            if (!(key.Length == 24 || key.Length == 16)) throw new ArgumentException("key.Length is error", "key");
            if (iv.Length != 8) throw new ArgumentException("iv.Length is error!", "iv");
            byte[] output = null;
            if (input != null && input.Length > 0)
            {
                using (var des = TripleDES.Create())
                {
                    des.Mode = mode;
                    des.Padding = padding;
                    using (ICryptoTransform cryptoTransform = des.CreateEncryptor(key, iv))
                    {
                        output = cryptoTransform.TransformFinalBlock(input, 0, input.Length);
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
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, DefaultKey, DefaultIV, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] input, string key)
        {
            return Decrypt(input, key, DefaultIV, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] input, string key, string iv)
        {
            return Decrypt(input, key, iv, DefaultMode, DefaultPadding);
        }

        public static byte[] Decrypt(byte[] input, byte[] key, byte[] iv)
        {
            return Decrypt(input, key, iv, DefaultMode, DefaultPadding);
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
            if (string.IsNullOrEmpty(iv)) throw new ArgumentNullException("iv");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] ivBytes = Encoding.ASCII.GetBytes(iv);
            byte[] output = Decrypt(input, keyBytes, ivBytes, mode, padding);

            return output;
        }

        public static byte[] Decrypt(byte[] input, byte[] key, byte[] iv, CipherMode mode, PaddingMode padding)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (iv == null) throw new ArgumentNullException("iv");
            if (!(key.Length == 24 || key.Length == 16)) throw new ArgumentException("key.Length is error!", "key");
            if (iv.Length != 8) throw new ArgumentException("iv.Length is error!", "iv");
            byte[] output = null;
            if (input != null && input.Length > 0)
            {
                using (var des = TripleDES.Create())
                {
                    des.Mode = mode;
                    des.Padding = padding;
                    using (ICryptoTransform cryptoTransform = des.CreateDecryptor(key, iv))
                    {
                        output = cryptoTransform.TransformFinalBlock(input, 0, input.Length);
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
        /// <param name="resultType"></param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, StringByteType resultType = StringByteType.Hex)
        {
            return Encrypt(input, DefaultKey, resultType);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="resultType"></param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, string key, StringByteType resultType = StringByteType.Hex)
        {
            return Encrypt(input, key, DefaultIV, DefaultMode, DefaultPadding, resultType);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <param name="resultType"></param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, string key, string iv, StringByteType resultType = StringByteType.Hex)
        {
            return Encrypt(input, key, iv, DefaultMode, DefaultPadding, resultType);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <param name="resultType"></param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, string key, CipherMode mode, PaddingMode padding, StringByteType resultType = StringByteType.Hex)
        {
            return Encrypt(input, key, DefaultIV, mode, padding, resultType);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <param name="resultType"></param>
        /// <returns>加密成功返回string</returns>
        public static string Encrypt(string input, string key, string iv, CipherMode mode, PaddingMode padding, StringByteType resultType = StringByteType.Hex)
        {
            string output = null;
            if (!string.IsNullOrEmpty(input))
            {
                byte[] inputdata = Encoding.UTF8.GetBytes(input);
                byte[] outputdata = Encrypt(inputdata, key, iv, mode, padding);
                output = resultType == StringByteType.Hex ? StringUtils.ByteToHexString(outputdata)
                    : Convert.ToBase64String(outputdata);
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
        /// <param name="inputType"></param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, StringByteType inputType = StringByteType.Hex)
        {
            return Decrypt(input, DefaultKey, inputType);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="inputType"></param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, string key, StringByteType inputType = StringByteType.Hex)
        {
            return Decrypt(input, key, DefaultIV, DefaultMode, DefaultPadding, inputType);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8个ASCII字符 iv</param>
        /// <param name="inputType"></param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, string key, string iv, StringByteType inputType = StringByteType.Hex)
        {
            return Decrypt(input, key, iv, DefaultMode, DefaultPadding, inputType);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <param name="inputType"></param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, string key, CipherMode mode, PaddingMode padding, StringByteType inputType = StringByteType.Hex)
        {
            return Decrypt(input, key, DefaultIV, mode, padding, inputType);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">24个ASCII字符 key</param>
        /// <param name="iv">8 个ASCII字符 iv</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <param name="inputType"></param>
        /// <returns>解密成功返回string</returns>
        public static string Decrypt(string input, string key, string iv, CipherMode mode, PaddingMode padding, StringByteType inputType = StringByteType.Hex)
        {
            string output = null;
            byte[] inputdata = inputType == StringByteType.Hex ? StringUtils.HexStringToByte(input)
                : Convert.FromBase64String(input);
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
            return Encrypt(inputStream, outputStream, key, DefaultIV, DefaultMode, DefaultPadding);
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
            return Encrypt(inputStream, outputStream, key, DefaultIV, mode, padding);
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
            if (string.IsNullOrEmpty(iv)) throw new ArgumentNullException("iv");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (!(keyBytes.Length == 24 || keyBytes.Length == 16)) throw new ArgumentException("key.Length is error!", "key");
            byte[] ivBytes = Encoding.ASCII.GetBytes(iv);
            if (ivBytes.Length != 8) throw new ArgumentException("iv.Length is error!", "iv");
            bool result = false;
            if (inputStream.Length > 0)
            {
                using (var des = TripleDES.Create())
                {
                    des.Mode = mode;
                    des.Padding = padding;
                    using (var cryptoTransform = des.CreateEncryptor(keyBytes, ivBytes))
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
            return Decrypt(inputStream, outputStream, key, DefaultIV, DefaultMode, DefaultPadding);
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
            if (string.IsNullOrEmpty(iv)) throw new ArgumentNullException("iv");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (!(keyBytes.Length == 24 || keyBytes.Length == 16)) throw new ArgumentException("key.Length is error!", "key");
            byte[] ivBytes = Encoding.ASCII.GetBytes(iv);
            if (ivBytes.Length != 8) throw new ArgumentException("iv.Length is error!", "iv");
            bool result = false;
            if (inputStream.Length > 0)
            {
                using (var des = TripleDES.Create())
                {
                    des.Mode = mode;
                    des.Padding = padding;
                    using (var cryptoTransform = des.CreateDecryptor(keyBytes, ivBytes))
                    {
                        CryptoStream cryptoStream = new CryptoStream(outputStream, cryptoTransform, CryptoStreamMode.Write);
                        //using (CryptoStream cryptoStream = new CryptoStream(outputStream, cryptoTransform, CryptoStreamMode.Write))
                        {
                            long endlength = inputStream.Length;
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// DES 加密、解密
    /// </summary>
    public static class DesUtils
    {
        /// <summary>
        /// 加密、解密默认 key
        /// </summary>
        private const string DefaultKey = "cj9@i8+&";
        /// <summary>
        /// 加密、解密默认 CipherMode
        /// </summary>
        public const CipherMode DefaultMode = CipherMode.CBC;
        /// <summary>
        /// 加密、解密默认CipherMode
        /// </summary>
        public const PaddingMode DefaultPadding = PaddingMode.PKCS7;

        /// <summary>
        /// 生成 8 个ASCII字符的 des key
        /// </summary>
        /// <returns>8 个ASCII字符</returns>
        public static string CreateKey()
        {
            return StringUtils.GetRandomString(8);
        }

        #region bytes
        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <returns>加密成功返回byte[], 加密失败返回null</returns>
        public static byte[] Encrypt(byte[] input)
        {
            return Encrypt(input, DefaultKey);
        }

        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <returns>加密成功返回byte[], 加密失败返回null</returns>
        public static byte[] Encrypt(byte[] input, string key)
        {
            return Encrypt(input, key, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>加密成功返回byte[], 加密失败返回null</returns>
        public static byte[] Encrypt(byte[] input, string key, CipherMode mode, PaddingMode padding)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is error!");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length != 8) throw new ArgumentException("key is error!");
            byte[] output = null;
            if (input != null && input.Length > 0)
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.Mode = mode;
                des.Padding = padding;
                byte[] ivBytes = new byte[8];
                des.GenerateIV();
                Array.Copy(des.IV, ivBytes, ivBytes.Length);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream encStream = new CryptoStream(ms, des.CreateEncryptor(keyBytes, ivBytes), CryptoStreamMode.Write))
                    {
                        encStream.Write(input, 0, input.Length);
                        encStream.FlushFinalBlock();
                        ms.Seek(0, SeekOrigin.Begin);
                        byte[] buffer = new byte[ivBytes.Length + ms.Length];
                        ms.Read(buffer, 0, buffer.Length - ivBytes.Length);
                        Array.Copy(ivBytes, 0, buffer, buffer.Length - ivBytes.Length, ivBytes.Length);
                        output = buffer;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <returns>解密成功返回byte[], 解密失败返回null</returns>
        public static byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, DefaultKey);
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <returns>解密成功返回byte[], 解密失败返回null</returns>
        public static byte[] Decrypt(byte[] input, string key)
        {
            return Decrypt(input, key, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密 byte[]
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>解密成功返回byte[], 解密失败返回null</returns>
        public static byte[] Decrypt(byte[] input, string key, CipherMode mode, PaddingMode padding)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is error!");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length != 8) throw new ArgumentException("key is error!");
            byte[] output = null;
            if (input != null && input.Length > 0)
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.Mode = mode;
                des.Padding = padding;
                byte[] ivBytes = new byte[8];
                Array.Copy(input, input.Length - ivBytes.Length, ivBytes, 0, ivBytes.Length);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream encStream = new CryptoStream(ms, des.CreateDecryptor(keyBytes, ivBytes), CryptoStreamMode.Write))
                    {
                        encStream.Write(input, 0, input.Length - ivBytes.Length);
                        encStream.FlushFinalBlock();
                        ms.Seek(0, SeekOrigin.Begin);
                        byte[] buffer = ms.ToArray();
                        output = buffer;
                    }
                }
            }

            return output;
        }
        #endregion

        #region string
        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <returns>加密成功返回string, 加密失败返回null</returns>
        public static string Encrypt(string input)
        {
            return Encrypt(input, DefaultKey);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <returns>加密成功返回string, 加密失败返回null</returns>
        public static string Encrypt(string input, string key)
        {
            return Encrypt(input, key, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>加密成功返回string, 加密失败返回null</returns>
        public static string Encrypt(string input, string key, CipherMode mode, PaddingMode padding)
        {
            string output = null;
            if (!string.IsNullOrEmpty(input)
                && !string.IsNullOrEmpty(key) && key.Length == 8)
            {
                byte[] inputdata = Encoding.UTF8.GetBytes(input);
                byte[] outputdata = Encrypt(inputdata, key, mode, padding);
                output = StringUtils.ByteToHexString(outputdata);
            }

            return output;
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <returns>解密成功返回string, 解密失败返回null</returns>
        public static string Decrypt(string input)
        {
            return Decrypt(input, DefaultKey);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <returns>解密成功返回string, 解密失败返回null</returns>
        public static string Decrypt(string input, string key)
        {
            return Decrypt(input, key, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密 string
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>解密成功返回string, 解密失败返回null</returns>
        public static string Decrypt(string input, string key, CipherMode mode, PaddingMode padding)
        {
            string output = null;
            if (!string.IsNullOrEmpty(input) && input.Length % 2 == 0
                && !string.IsNullOrEmpty(key) && key.Length == 8)
            {
                byte[] inputdata = StringUtils.HexStringToByte(input);
                byte[] outputdata = Decrypt(inputdata, key, mode, padding);
                if (outputdata != null && outputdata.Length > 0)
                {
                    output = Encoding.UTF8.GetString(outputdata);
                }
            }

            return output;
        }
        #endregion

        #region File
        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="inputFile">要加密文件路径</param>
        /// <param name="outputFile">加密成功文件存放路径</param>
        /// <returns>是否成功</returns>
        public static bool EncryptFile(string inputFile, string outputFile)
        {
            return EncryptFile(inputFile, outputFile, DefaultKey);
        }

        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="inputFile">要加密文件路径</param>
        /// <param name="outputFile">加密成功文件存放路径</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <returns>是否成功</returns>
        public static bool EncryptFile(string inputFile, string outputFile, string key)
        {
            return EncryptFile(inputFile, outputFile, key, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="inputFile">要加密文件路径</param>
        /// <param name="outputFile">加密成功文件存放路径</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <param name="mode">指定用于加密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>是否成功</returns>
        public static bool EncryptFile(string inputFile, string outputFile, string key,
            CipherMode mode, PaddingMode padding)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is error!");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length != 8) throw new ArgumentException("key is error!");
            bool result = false;
            try
            {
                if (File.Exists(inputFile))
                {
                    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                    des.Mode = mode;
                    des.Padding = padding;
                    byte[] rgbIV = new byte[8];
                    des.GenerateIV();
                    Array.Copy(des.IV, rgbIV, rgbIV.Length);

                    using (FileStream outputfs = File.Create(outputFile))
                    {
                        using (CryptoStream encStream = new CryptoStream(outputfs, des.CreateEncryptor(keyBytes, rgbIV), CryptoStreamMode.Write))
                        {
                            using (FileStream inputfs = File.OpenRead(inputFile))
                            {
                                byte[] buffer = new byte[4 * 1024];
                                do
                                {
                                    int len = inputfs.Read(buffer, 0, buffer.Length);
                                    encStream.Write(buffer, 0, len);
                                }
                                while (inputfs.Position < inputfs.Length);

                                encStream.FlushFinalBlock();
                                outputfs.Flush();
                            }

                            outputfs.Write(rgbIV, 0, rgbIV.Length);
                            outputfs.Flush();
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(outputFile))
                        File.Delete(outputFile);
                }
                catch { }
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="inputFile">要解密文件路径</param>
        /// <param name="outputFile">解密成功文件存放路径</param>
        /// <returns>是否成功</returns>
        public static bool DecryptFile(string inputFile, string outputFile)
        {
            return DecryptFile(inputFile, outputFile, DefaultKey);
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="inputFile">要解密文件路径</param>
        /// <param name="outputFile">解密成功文件存放路径</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <returns>是否成功</returns>
        public static bool DecryptFile(string inputFile, string outputFile, string key)
        {
            return DecryptFile(inputFile, outputFile, key, DefaultMode, DefaultPadding);
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="inputFile">要解密文件路径</param>
        /// <param name="outputFile">解密成功文件存放路径</param>
        /// <param name="key">8 个ASCII字符 key</param>
        /// <param name="mode">指定用于解密的块密码模式</param>
        /// <param name="padding">指定在消息数据块比解密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>是否成功</returns>
        public static bool DecryptFile(string inputFile, string outputFile, string key,
            CipherMode mode, PaddingMode padding)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is error!");
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length != 8) throw new ArgumentException("key is error!");
            bool result = false;
            try
            {
                if (File.Exists(inputFile))
                {
                    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                    des.Mode = mode;
                    des.Padding = padding;
                    byte[] rgbIV = new byte[8];

                    using (FileStream outputfs = File.Create(outputFile))
                    {
                        using (FileStream inputfs = File.OpenRead(inputFile))
                        {
                            long endlength = inputfs.Length - 8;
                            inputfs.Seek(rgbIV.Length, SeekOrigin.End);
                            inputfs.Read(rgbIV, 0, rgbIV.Length);
                            inputfs.Seek(0, SeekOrigin.Begin);

                            using (CryptoStream encStream = new CryptoStream(outputfs, des.CreateDecryptor(keyBytes, rgbIV), CryptoStreamMode.Write))
                            {
                                byte[] buffer = new byte[4 * 1024];
                                do
                                {
                                    int readlength = buffer.Length;
                                    long _count = endlength - inputfs.Position;
                                    if (_count < readlength)
                                        readlength = (int)_count;
                                    int len = inputfs.Read(buffer, 0, readlength);
                                    encStream.Write(buffer, 0, len);
                                }
                                while (inputfs.Position < endlength);

                                encStream.FlushFinalBlock();
                                inputfs.Flush();
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(outputFile))
                        File.Delete(outputFile);
                }
                catch { }
                throw ex;
            }

            return result;
        }
        #endregion
    }
}

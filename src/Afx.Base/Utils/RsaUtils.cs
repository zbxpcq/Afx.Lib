using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Afx.Utils
{
    /// <summary>
    /// RSA算法类型
    /// </summary>
    public enum RsaType
    {
        /// <summary>
        /// SHA1
        /// </summary>
        RSA = 1,
        /// <summary>
        /// RSA2 密钥长度至少为2048
        /// SHA256
        /// </summary>
        RSA2 = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public static class RsaUtils
    {
        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
            if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private static RSAParameters GetRsaParameterByPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);
            var rsaParameters = new RSAParameters();
            using (var ms = new MemoryStream(privateKeyBits))
            {
                using (BinaryReader binr = new BinaryReader(ms))
                {
                    byte bt = 0;
                    ushort twobytes = 0;
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        throw new ArgumentException($"{nameof(privateKey)}格式不正确！", nameof(privateKey)); //Unexpected value read binr.ReadUInt16();

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102)
                        throw new ArgumentException($"{nameof(privateKey)}格式不正确！", nameof(privateKey)); //("Unexpected version");

                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        throw new ArgumentException($"{nameof(privateKey)}格式不正确！", nameof(privateKey)); //("Unexpected value read binr.ReadByte()");

                    rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                    rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
                }
            }

            return rsaParameters;
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        private static RSAParameters GetRsaParameterByPublicKey(string publicKey)
        {
            RSAParameters rsaKeyInfo = new RSAParameters();
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            var x509Key = Convert.FromBase64String(publicKey);

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (MemoryStream mem = new MemoryStream(x509Key))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        throw new ArgumentException($"{nameof(publicKey)}格式不正确！", nameof(publicKey));

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, seqOid))    //make sure Sequence for OID is correct
                        throw new ArgumentException($"{nameof(publicKey)}格式不正确！", nameof(publicKey));

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        throw new ArgumentException($"{nameof(publicKey)}格式不正确！", nameof(publicKey));

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        throw new ArgumentException($"{nameof(publicKey)}格式不正确！", nameof(publicKey));

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        throw new ArgumentException($"{nameof(publicKey)}格式不正确！", nameof(publicKey));

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        throw new ArgumentException($"{nameof(publicKey)}格式不正确！", nameof(publicKey));
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        throw new ArgumentException($"{nameof(publicKey)}格式不正确！", nameof(publicKey));
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    rsaKeyInfo.Modulus = modulus;
                    rsaKeyInfo.Exponent = exponent;
                }

            }

            return rsaKeyInfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="privateKey"></param>
        /// <param name="type"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Sign(string data, string privateKey, RsaType type = RsaType.RSA, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(privateKey)) throw new ArgumentNullException(nameof(privateKey));
            if (encoding == null) encoding = Encoding.UTF8;
#if netstandard
            using (var rsa = RSA.Create())
#else
            using (var rsa = new RSACryptoServiceProvider())
#endif
            {
                var rsaParameters = GetRsaParameterByPrivateKey(privateKey);
                rsa.ImportParameters(rsaParameters);
                var dataBytes = encoding.GetBytes(data);
#if netstandard
                var hashAlgorithmName = type == RsaType.RSA ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;
                var signatureBytes = rsa.SignData(dataBytes, hashAlgorithmName, RSASignaturePadding.Pkcs1);
#else
                var hashAlgorithmName = type == RsaType.RSA ? "SHA1" : "SHA256";
                var signatureBytes = rsa.SignData(dataBytes, hashAlgorithmName);
#endif
                var result = Convert.ToBase64String(signatureBytes);

                return result;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sign"></param>
        /// <param name="publicKey"></param>
        /// <param name="type"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static bool VerifyData(string data, string sign, string publicKey, RsaType type = RsaType.RSA, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(sign)) throw new ArgumentNullException(nameof(sign));
            if (string.IsNullOrEmpty(publicKey)) throw new ArgumentNullException(nameof(publicKey));
            if (encoding == null) encoding = Encoding.UTF8;
            byte[] dataBytes = encoding.GetBytes(data);
            byte[] signBytes = Convert.FromBase64String(sign);
#if netstandard
            using (var rsa = RSA.Create())
#else
            using (var rsa = new RSACryptoServiceProvider() { PersistKeyInCsp = false })
#endif
            {
                RSAParameters rsaKeyInfo = GetRsaParameterByPublicKey(publicKey);
                rsa.ImportParameters(rsaKeyInfo);
#if netstandard
                var hashAlgorithmName = type == RsaType.RSA ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;
                var result = rsa.VerifyData(dataBytes, signBytes, hashAlgorithmName, RSASignaturePadding.Pkcs1);
#else
                //bool result = false;
                //if (type == RsaType.RSA)
                //{
                //    using (var sha1 = new SHA1CryptoServiceProvider())
                //    {
                //        result = rsa.VerifyData(dataBytes, sha1, signBytes);
                //    }
                //}
                //else
                //{
                //    result = rsa.VerifyData(dataBytes, "SHA256", signBytes);
                //}
                var hashAlgorithmName = type == RsaType.RSA ? "SHA1" : "SHA256";
                var result = rsa.VerifyData(dataBytes, hashAlgorithmName, signBytes);
#endif
                return result;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] input, string publicKey)
        {
            if (input == null || input.Length == 0) throw new ArgumentNullException(nameof(input));
            if (string.IsNullOrEmpty(publicKey)) throw new ArgumentNullException(nameof(publicKey));
#if netstandard
            using (var rsa = RSA.Create())
#else
            using (var rsa = new RSACryptoServiceProvider() { PersistKeyInCsp = false })
#endif
            {
                var rsaParameters = GetRsaParameterByPublicKey(publicKey);
                rsa.ImportParameters(rsaParameters);
#if netstandard
                var buffer = rsa.Encrypt(input, RSAEncryptionPadding.Pkcs1);
                return buffer;
#else
                int maxBlockSize = rsa.KeySize / 8 - 11; //加密块最大长度限制
                if (input.Length <= maxBlockSize)
                {
                    var buffer = rsa.Encrypt(input, false);
                    return buffer;
                }
                else
                {
                    using (MemoryStream plaiStream = new MemoryStream(input))
                    {
                        using (MemoryStream crypStream = new MemoryStream())
                        {
                            Byte[] buffer = new Byte[maxBlockSize];
                            int blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                            while (blockSize > 0)
                            {
                                Byte[] toEncrypt = new Byte[blockSize];
                                Array.Copy(buffer, 0, toEncrypt, 0, blockSize);
                                Byte[] cryptograph = rsa.Encrypt(toEncrypt, false);
                                crypStream.Write(cryptograph, 0, cryptograph.Length);
                                blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                            }

                            return crypStream.ToArray();
                        }
                    }
                }
#endif

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] input, string privateKey)
        {
            if (input == null || input.Length == 0) throw new ArgumentNullException(nameof(input));
            if (string.IsNullOrEmpty(privateKey)) throw new ArgumentNullException(nameof(privateKey));
#if netstandard
            using (var rsa = RSA.Create())
#else
            using (var rsa = new RSACryptoServiceProvider())
#endif
            {
                var rsaParameters = GetRsaParameterByPrivateKey(privateKey);
                rsa.ImportParameters(rsaParameters);
#if netstandard
                var buffer = rsa.Decrypt(input, RSAEncryptionPadding.Pkcs1);

                return buffer;
#else
                int maxBlockSize = rsa.KeySize / 8; //解密块最大长度限制
                if (input.Length <= maxBlockSize)
                {
                    byte[] buffer = rsa.Decrypt(input, false);
                    return buffer;
                }
                else
                {
                    using (MemoryStream crypStream = new MemoryStream(input))
                    {
                        using (MemoryStream plaiStream = new MemoryStream())
                        {
                            Byte[] buffer = new Byte[maxBlockSize];
                            int blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                            while (blockSize > 0)
                            {
                                Byte[] toDecrypt = new Byte[blockSize];
                                Array.Copy(buffer, 0, toDecrypt, 0, blockSize);
                                Byte[] cryptograph = rsa.Decrypt(toDecrypt, false);
                                plaiStream.Write(cryptograph, 0, cryptograph.Length);
                                blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                            }

                            return plaiStream.ToArray();
                        }
                    }
                }
#endif
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="publicKey"></param>
        /// <param name="resultType"></param>
        /// <returns></returns>
        public static string Encrypt(string input, string publicKey, StringByteType resultType = StringByteType.Hex)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));
            var arr = Encoding.UTF8.GetBytes(input);
            var buffer = Encrypt(arr, publicKey);
            var result = resultType == StringByteType.Hex ? Afx.Utils.StringUtils.ByteToHexString(buffer)
                : Convert.ToBase64String(buffer);

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="privateKey"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public static string Decrypt(string input, string privateKey, StringByteType inputType = StringByteType.Hex)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));
            var arr = inputType == StringByteType.Hex ? Afx.Utils.StringUtils.HexStringToByte(input)
                : Convert.FromBase64String(input);
            var buffer = Decrypt(arr, privateKey);
            var result = Encoding.UTF8.GetString(buffer);

            return result;
        }
    }
}

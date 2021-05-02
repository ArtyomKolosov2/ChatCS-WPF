using Common.Config;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Common.Utils
{
    public static class AesEncryptionUtil
    {
        public static byte[] GetKey(string keyString)
        {
            var key = GlobalConfig.Encoding.GetBytes(keyString);
            key = SHA256.Create().ComputeHash(key);
            var result = new byte[16];
            Array.Copy(key, result, 16);

            return result;
        }

        public static ReadOnlySpan<byte> EncryptStringToAes(string src)
        {
            var aes = Aes.Create();
            byte[] encrypted;
            aes.GenerateIV();

            var crypt = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, crypt, CryptoStreamMode.Write))
                {
                    using (var streamWriter = new StreamWriter(cryptoStream, GlobalConfig.Encoding))
                    {
                        foreach (var oneByte in GlobalConfig.Encoding.GetBytes(src))
                        {
                            streamWriter.Write(oneByte);
                        }
                    }
                }
                encrypted = memoryStream.ToArray();
            }

            return encrypted.Concat(aes.IV).ToArray();
        }

        public static string DecryptStringFromAes(string src)
        {
            

            var shifr = GlobalConfig.Encoding.GetBytes(src);

            byte[] bytesIv = new byte[16];
            byte[] mess = new byte[shifr.Length - 16];

            for (int i = shifr.Length - 16, j = 0; i < shifr.Length; i++, j++)
            {
                bytesIv[j] = shifr[i];
            }


            for (int i = 0; i < shifr.Length - 16; i++)
            {
                mess[i] = shifr[i];
            }

            var aes = Aes.Create();

            aes.Key = GetKey(GlobalConfig.CypherKey);
            aes.IV = bytesIv;

            var text = string.Empty;
            var data = mess;
            var crypt = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var memoryStream = new MemoryStream(data))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, crypt, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cryptoStream, GlobalConfig.Encoding))
                    {
                        cryptoStream.FlushFinalBlock();
                        text = sr.ReadToEnd();
                    }
                }
            }

            return text;
        }
    }
}

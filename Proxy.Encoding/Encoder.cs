﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Proxy.Encoding
{
    public class Encoder
    {
        private byte[] _key;
        private byte[] _iv;
        public const int DefaultBlockSize = 128;

        public Encoder(byte[] key, byte[] iv)
        {
            _key = key;
            _iv = iv;
        }

        private byte[] Compress(byte[] source)
        {
            var rezStream = new MemoryStream();

            using (var stream = new GZipStream(rezStream, CompressionMode.Compress, true))
            {
                stream.Write(source, 0, source.Length);
                stream.Flush();
            }

            return rezStream.ToArray();
        }

        private byte[] Decompress(byte[] source)
        {
            var rezStream = new MemoryStream();

            using (var stream = new GZipStream(new MemoryStream(source), CompressionMode.Decompress, true))
            {
                stream.CopyTo(rezStream);
            }

            return rezStream.ToArray();
        }

        public byte[] Encode(object source)
        {
            var encoder = new XmlSerializer(source.GetType());
            using (var stream = new MemoryStream())
            {
                encoder.Serialize(stream, source);
                var sRez = stream.ToArray();// System.Text.Encoding.Unicode.GetString(stream.ToArray());
                return Encode(Compress(sRez));// System.Text.Encoding.Unicode.GetBytes(sRez)));
            }
        }

        public T Decode<T>(byte[] encoded)
        {
            var decrypted = Decompress(Decode(encoded));//);System.Text.Encoding.Unicode.GetString(
            using (var stream = new MemoryStream(decrypted))//StringReader
            {
                var encoder = new XmlSerializer(typeof(T));
                return (T)encoder.Deserialize(stream);
            }
        }

        byte[] Encode(byte[] source)
        {
            using (var encryption = AesCryptoServiceProvider.Create())
            {
                //Debug.WriteLine("Current block size: " + encryption.BlockSize.ToString());
                encryption.BlockSize = DefaultBlockSize;
                encryption.Key = _key;
                encryption.IV = _iv;
                using (var encryptor = encryption.CreateEncryptor())
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(source, 0, source.Length);
                            csEncrypt.FlushFinalBlock();
                            return msEncrypt.ToArray();
                        }
                    }
                }
            }
        }

        byte[] Decode(byte[] encrypted)
        {
            using (var encryption = AesCryptoServiceProvider.Create())
            {
                encryption.BlockSize = DefaultBlockSize;
                encryption.Key = _key;
                encryption.IV = _iv;
                using (var encryptor = encryption.CreateDecryptor())
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(encrypted, 0, encrypted.Length);
                            csEncrypt.FlushFinalBlock();
                            return msEncrypt.ToArray();
                        }
                    }
                }
            }
        }
    }
}

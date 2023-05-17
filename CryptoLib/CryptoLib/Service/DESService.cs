﻿using CryptoLib.Algorithm;
using CryptoLib.Algorithm.Key;
using CryptoLib.Service.Padding;
using CryptoLib.Service.Mode;
using CryptoLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service
{
    public class DESService : IKeyGenerator<DESKeyType>, ICryptoService
    {
        public string? Passphrase { get; set; }
        public uint Iteration { get; set; } = 4096;
        public DESPaddingScheme Padding { get; set; } = DESPaddingScheme.PKCS5;
        public BlockCipherMode CipherMode { get; set; } = BlockCipherMode.ECB;
        public byte[]? Salt { get; set; }

        public static readonly int SaltSize = 8;
        public static readonly int IVSize = 8;
        public static readonly int BlockSize = 8;

        static public byte[] DecryptBlock(byte[] data, IKey key)
        {
            var encrypted = Algorithm.DES.Decrypt(data, key.ToByteArray());
            byte[] bytes = new byte[BlockSize];
            encrypted.CopyTo(bytes, 0);
            return bytes;
        }

        public byte[] Decrypt(byte[] data, IKey key, IDictionary<string, object>? param = null)
        {
            DESKey? deskey = key as DESKey;
            if (deskey == null)
            {
                throw new InvalidOperationException();
            }

            if (deskey.IV == null)
            {
                throw new InvalidOperationException();
            }

            bool disable_padding = false;
            if (param != null)
            {
                object? _disable_padding;
                bool success = param.TryGetValue("disable_padding", out _disable_padding);
                if (success && _disable_padding != null)
                {
                    disable_padding = (bool)_disable_padding;
                }
            }

            IPaddingScheme? padding = DESPaddingSchemeFactory.CreateInstance(Padding);
            IBlockCipherMode mode = BlockCipherModeFactory.CreateInstance(CipherMode);
            var func = DecryptBlock;
            if (CipherMode == BlockCipherMode.CFB || CipherMode == BlockCipherMode.CTR)
            {
                func = EncryptBlock;
            }

            Dictionary<string, object> modeParams = new Dictionary<string, object>()
            {
                { "IV", deskey.IV },
            };

            List<byte[]> encrypted = Helper.SplitByCount(data, BlockSize);
            List<byte[]> decrypted = mode.Decrypt(encrypted, key, func, modeParams);

            List<byte> bytes = new List<byte>();
            for (int i = 0; i < decrypted.Count; i++)
            {
                byte[] block = decrypted[i];
                bytes.AddRange(block);
            }
            byte[] result = bytes.ToArray();
            if (padding != null && disable_padding != true)
            {
                result = padding.Decode(result, param);
            }

            return result;
        }

        public string Decrypt(string data, IKey key, IDictionary<string, object>? param = null)
        {
            byte[] encrypted = Convert.FromBase64String(data);
            byte[] decrypted = Decrypt(encrypted, key, param);
            string result = Encoding.UTF8.GetString(decrypted);
            return result;
        }

        static public byte[] EncryptBlock(byte[] data, IKey key)
        {
            var encrypted = Algorithm.DES.Encrypt(data, key.ToByteArray());
            byte[] bytes = new byte[BlockSize];
            encrypted.CopyTo(bytes, 0);
            return bytes;
        }

        public byte[] Encrypt(byte[] data, IKey key, IDictionary<string, object>? param = null)
        {
            DESKey? deskey = key as DESKey;
            if (deskey == null)
            {
                throw new InvalidOperationException();
            }

            if (deskey.IV == null)
            {
                throw new InvalidOperationException();
            }

            bool disable_padding = false;
            if (param != null)
            {
                object? _disable_padding;
                bool success = param.TryGetValue("disable_padding", out _disable_padding);
                if (success && _disable_padding != null)
                {
                    disable_padding = (bool)_disable_padding;
                    Console.WriteLine(disable_padding);
                }
            }

            var func = EncryptBlock;
            IPaddingScheme? padding = DESPaddingSchemeFactory.CreateInstance(Padding);
            IBlockCipherMode mode = BlockCipherModeFactory.CreateInstance(CipherMode);
            Dictionary<string, object> modeParams = new Dictionary<string, object>()
            {
                { "IV", deskey.IV },
            };

            byte[] message = data;
            if (padding != null && disable_padding != true)
            {
                message = padding.Encode(data, param);
            }

            List<byte[]> messageBlocks = Helper.SplitByCount(message, BlockSize);
            List<byte[]> encrypted = mode.Encrypt(messageBlocks, key, func, modeParams);
            List<byte> bytes = new List<byte>();
            foreach (byte[] block in encrypted)
            {
                bytes.AddRange(block);
            }

            return bytes.ToArray();
        }

        public string Encrypt(string data, IKey key, IDictionary<string, object>? param = null)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(data);
            byte[] encrypted = Encrypt(encoded, key, param);
            string result = Convert.ToBase64String(encrypted);
            return result;
        }

        public async Task<Dictionary<DESKeyType, IKey>> GenerateAsync()
        {
            Dictionary<DESKeyType, IKey>? keys = null;
            await Task.Run(() =>
            {
                keys = Generate();
            });

            if (keys == null)
            {
                throw new InvalidOperationException();
            }

            return keys;
        }

        public Dictionary<DESKeyType, IKey> Generate()
        {
            if (Passphrase == null)
            {
                throw new ArgumentNullException();
            }

            byte[] salt;
            if (Salt != null)
            {
                salt = Salt;
            }
            else
            {
                salt = MathHelper.GetRandomBytes(SaltSize);
            }

            byte[] IV = CryptoHelper.GenerateIV(IVSize);
            byte[] _key = KeyDerivation.PBKDF2(Encoding.UTF8.GetBytes(Passphrase), salt, Iteration, 7);
            BitArray ba_key = new BitArray(_key);
            List<BitArray> list = BitHelper.SplitBitsByCount(ba_key, 7);
            List<bool> bools = new List<bool>(64);
            foreach (BitArray ba in list)
            {
                ba.Length = 8;
                bool[] ba2 = ba.ConvertBitsToBools();
                int count = 0;
                foreach (bool b in ba)
                {
                    if (b == true) 
                    {
                        count++;
                    }
                }
                if (count % 2 != 0)
                {
                    ba2[7] = true;
                }
                bools.AddRange(ba2);
            }

            byte[] bytes = BitHelper.ConvertBitsToBytes(new BitArray(bools.ToArray()));
            DESKey key = new DESKey(bytes);
            key.Salt = salt;
            key.IV = IV;
            var keys = new Dictionary<DESKeyType, IKey>
            {
                { DESKeyType.Key, key }
            };

            return keys;
        }
    }
}

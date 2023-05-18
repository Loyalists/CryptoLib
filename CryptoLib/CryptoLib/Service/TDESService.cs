using CryptoLib.Algorithm;
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
    public class TDESService : IKeyGenerator<DESKeyType>, ICryptoService
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
            TDESKey? tdeskey = key as TDESKey;
            if (tdeskey == null)
            {
                throw new InvalidOperationException();
            }
            List<DESKey> keys = tdeskey.ToDESKeys();
            byte[] k3_decrypted = Algorithm.DES.Decrypt(data, keys[2].ToByteArray());
            byte[] k2_encrypted = Algorithm.DES.Encrypt(k3_decrypted, keys[1].ToByteArray());
            byte[] k1_decrypted = Algorithm.DES.Decrypt(k2_encrypted, keys[0].ToByteArray());

            byte[] bytes = new byte[BlockSize];
            k1_decrypted.CopyTo(bytes, 0);
            return bytes;
        }

        static public byte[] EncryptBlock(byte[] data, IKey key)
        {
            TDESKey? tdeskey = key as TDESKey;
            if (tdeskey == null)
            {
                throw new InvalidOperationException();
            }

            List<DESKey> keys = tdeskey.ToDESKeys();
            byte[] k1_encrypted = Algorithm.DES.Encrypt(data, keys[0].ToByteArray());
            byte[] k2_decrypted = Algorithm.DES.Decrypt(k1_encrypted, keys[1].ToByteArray());
            byte[] k3_encrypted = Algorithm.DES.Encrypt(k2_decrypted, keys[2].ToByteArray());

            byte[] bytes = new byte[BlockSize];
            k3_encrypted.CopyTo(bytes, 0);
            return bytes;
        }

        public byte[] Decrypt(byte[] data, IKey key, IDictionary<string, object>? param = null)
        {
            TDESKey? tdeskey = key as TDESKey;
            if (tdeskey == null)
            {
                throw new InvalidOperationException();
            }

            if (tdeskey.IV == null)
            {
                throw new InvalidOperationException();
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
                { "IV", tdeskey.IV },
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
            if (padding != null)
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

        public byte[] Encrypt(byte[] data, IKey key, IDictionary<string, object>? param = null)
        {
            TDESKey? tdeskey = key as TDESKey;
            if (tdeskey == null)
            {
                throw new InvalidOperationException();
            }

            if (tdeskey.IV == null)
            {
                throw new InvalidOperationException();
            }

            var func = EncryptBlock;
            IPaddingScheme? padding = DESPaddingSchemeFactory.CreateInstance(Padding);
            IBlockCipherMode mode = BlockCipherModeFactory.CreateInstance(CipherMode);
            Dictionary<string, object> modeParams = new Dictionary<string, object>()
            {
                { "IV", tdeskey.IV },
            };

            byte[] message = data;
            if (padding != null)
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
            byte[] _key = KeyDerivation.PBKDF2(Encoding.UTF8.GetBytes(Passphrase), salt, Iteration, 24);
            TDESKey key = new TDESKey(_key);
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

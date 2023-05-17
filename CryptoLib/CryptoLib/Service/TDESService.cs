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

        public byte[] Decrypt(byte[] data, IKey key, IDictionary<string, object>? param = null)
        {
            TDESKey? tdeskey = key as TDESKey;
            if (tdeskey == null)
            {
                throw new InvalidOperationException();
            }

            List<DESKey> keys = tdeskey.ToDESKeys();
            DESService service = new DESService();
            service.Padding = Padding;
            service.CipherMode = CipherMode;

            if (param == null)
            {
                param = new Dictionary<string, object>();
            }

            Dictionary<string, object> param_no_padding = new Dictionary<string, object>(param)
            {
                { "disable_padding", true }
            };

            byte[] k3_decrypted = service.Decrypt(data, keys[2], param_no_padding);
            byte[] k2_encrypted = service.Encrypt(k3_decrypted, keys[1], param_no_padding);
            byte[] k1_decrypted = service.Decrypt(k2_encrypted, keys[0], param);

            return k1_decrypted;
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

            List<DESKey> keys = tdeskey.ToDESKeys();
            DESService service = new DESService();
            service.Padding = Padding;
            service.CipherMode = CipherMode;

            if (param == null)
            {
                param = new Dictionary<string, object>();
            }

            Dictionary<string, object> param_no_padding = new Dictionary<string, object>(param)
            {
                { "disable_padding", true }
            };

            byte[] k1_encrypted = service.Encrypt(data, keys[0], param);
            byte[] k2_decrypted = service.Decrypt(k1_encrypted, keys[1], param_no_padding);
            byte[] k3_encrypted = service.Encrypt(k2_decrypted, keys[2], param_no_padding);

            return k3_encrypted;
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

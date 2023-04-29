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
    public class DESService : IKeyGenerator<DESKeyType>, IEncryptor, IDecryptor
    {
        public string? Passphrase { get; set; }
        public uint Iteration { get; set; } = 4096;
        public IPaddingScheme? Padding { get; set; } = null;
        public IBlockCipherMode? BlockCipherMode { get; set; } = null;
        private int blockSize = 8;

        public byte[] Decrypt(byte[] data, IKey key)
        {
            DESKey? deskey = key as DESKey;
            if (deskey == null)
            {
                throw new InvalidOperationException();
            }

            BitArray text = new BitArray(data);
            BitArray mainKey = new BitArray(deskey.Bytes);
            var encrypted = Algorithm.DES.Decrypt(text, mainKey);
            byte[] bytes = new byte[8];
            encrypted.CopyTo(bytes, 0);
            return bytes;
        }

        public string Decrypt(string data, IKey key)
        {
            if (Padding == null)
            {
                throw new InvalidOperationException();
            }

            if (BlockCipherMode == null)
            {
                throw new InvalidOperationException();
            }

            byte[] encrypted = Convert.FromBase64String(data);
            List<byte[]> encryptedBlocks = Helper.SplitByCount(encrypted, blockSize);
            List<byte[]> decrypted = BlockCipherMode.BlockDecrypt(encryptedBlocks, key);
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < decrypted.Count; i++)
            {
                byte[] block = decrypted[i];
                bytes.AddRange(block);
            }

            byte[] decodedMessage = Padding.Decode(bytes.ToArray(), key);
            string decoded = Encoding.UTF8.GetString(decodedMessage);
            return decoded;
        }

        public byte[] Encrypt(byte[] data, IKey key)
        {
            DESKey? deskey = key as DESKey;
            if (deskey == null)
            {
                throw new InvalidOperationException();
            }

            BitArray text = new BitArray(data);
            BitArray mainKey = new BitArray(deskey.Bytes);
            var encrypted = Algorithm.DES.Encrypt(text, mainKey);
            byte[] bytes = new byte[8];
            encrypted.CopyTo(bytes, 0);
            return bytes;
        }

        public string Encrypt(string data, IKey key)
        {
            if (Padding == null)
            {
                throw new InvalidOperationException();
            }

            if (BlockCipherMode == null)
            {
                throw new InvalidOperationException();
            }

            byte[] message = Padding.Encode(Encoding.UTF8.GetBytes(data), key);
            List<byte[]> messageBlocks = Helper.SplitByCount(message, blockSize);
            List<byte[]> encrypted = BlockCipherMode.BlockEncrypt(messageBlocks, key);
            List<byte> bytes = new List<byte>();
            foreach (byte[] block in encrypted)
            {
                bytes.AddRange(block);
            }
            string encoded = Convert.ToBase64String(bytes.ToArray());
            return encoded;
        }

        public Dictionary<DESKeyType, IKey> Generate()
        {
            if (Passphrase == null)
            {
                throw new ArgumentNullException();
            }

            byte[] salt = MathHelper.GetRandomBytes(16);
            byte[] _key = KeyDerivation.PBKDF2(Encoding.UTF8.GetBytes(Passphrase), salt, Iteration, 7);
            BitArray ba_key = new BitArray(_key);
            List<BitArray> list = BitHelper.SplitBitsByCount(ba_key, 7);
            List<bool> bools = new List<bool>(64);
            foreach (BitArray ba in list)
            {
                ba.Length = 8;
                bool[] ba2 = BitHelper.ConvertBitsToBoolArray(ba);
                bools.AddRange(ba2);
            }

            byte[] bytes = BitHelper.ConvertBitsToBytes(new BitArray(bools.ToArray()));
            DESKey key = new DESKey(bytes, salt);
            var keys = new Dictionary<DESKeyType, IKey>
            {
                { DESKeyType.Key, key }
            };

            return keys;
        }
    }
}

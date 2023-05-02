using CryptoLib.Algorithm.Key;
using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Mode
{
    public class CBCMode : IBlockCipherMode
    {
        public List<byte[]> Decrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> encryptFunc, IDictionary<string, object>? properties = null)
        {
            if (properties == null)
            {
                throw new InvalidOperationException();
            }

            byte[] IV = (byte[])properties["IV"];
            byte[] to_be_xored = IV;
            List<byte[]> decryptedBlocks = new List<byte[]>(blocks.Count);

            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] text = blocks[i];
                byte[] xor = text.XORBytes(to_be_xored);
                byte[] encrypted = encryptFunc(xor, key);
                to_be_xored = encrypted;
                decryptedBlocks.Add(encrypted);
            }

            return decryptedBlocks;
        }

        public List<byte[]> Encrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> decryptFunc, IDictionary<string, object>? properties = null)
        {
            if (properties == null)
            {
                throw new InvalidOperationException();
            }

            byte[] IV = (byte[])properties["IV"];
            byte[] to_be_xored = IV;
            List<byte[]> encryptedBlocks = new List<byte[]>(blocks.Count);

            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] text = blocks[i];
                byte[] decrypted = decryptFunc(text, key);
                byte[] xor = decrypted.XORBytes(to_be_xored);
                to_be_xored = text;
                encryptedBlocks.Add(xor);
            }

            return encryptedBlocks;
        }
    }
}

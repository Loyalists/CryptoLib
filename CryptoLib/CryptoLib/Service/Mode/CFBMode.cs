using CryptoLib.Algorithm.Key;
using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Mode
{
    public class CFBMode : IBlockCipherMode
    {
        public List<byte[]> Encrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> encryptFunc, IDictionary<string, object>? param = null)
        {
            if (param == null)
            {
                throw new InvalidOperationException();
            }

            byte[] IV = (byte[])param["IV"];
            byte[] input = IV;
            List<byte[]> encryptedBlocks = new List<byte[]>(blocks.Count);

            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] text = blocks[i];
                byte[] encrypted = encryptFunc(input, key);
                byte[] xor = text.XORBytes(encrypted);
                input = xor;
                encryptedBlocks.Add(xor);
            }

            return encryptedBlocks;
        }

        public List<byte[]> Decrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> decryptFunc, IDictionary<string, object>? param = null)
        {
            if (param == null)
            {
                throw new InvalidOperationException();
            }

            byte[] IV = (byte[])param["IV"];
            byte[] input = IV;
            List<byte[]> decryptedBlocks = new List<byte[]>(blocks.Count);

            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] text = blocks[i];
                byte[] decrypted = decryptFunc(input, key);
                byte[] xor = text.XORBytes(decrypted);
                input = text;
                decryptedBlocks.Add(xor);
            }

            return decryptedBlocks;
        }
    }
}

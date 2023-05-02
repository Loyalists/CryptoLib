using CryptoLib.Algorithm.Key;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Mode
{
    public class ECBMode : IBlockCipherMode
    {
        public List<byte[]> Encrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> decryptFunc, IDictionary<string, object>? properties = null)
        {
            List<byte[]> encryptedBlocks = new List<byte[]>(blocks.Count);

            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] block = decryptFunc(blocks[i], key);
                encryptedBlocks.Add(block);
            }

            return encryptedBlocks;
        }

        public List<byte[]> Decrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> encryptFunc, IDictionary<string, object>? properties = null)
        {
            List<byte[]> decryptedBlocks = new List<byte[]>(blocks.Count);
            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] block = encryptFunc(blocks[i], key);
                decryptedBlocks.Add(block);
            }

            return decryptedBlocks;
        }
    }
}

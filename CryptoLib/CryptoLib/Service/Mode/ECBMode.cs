using CryptoLib.Algorithm.Key;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Mode
{
    public class ECBMode : IBlockCipherMode
    {
        public List<byte[]> Encrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> decryptFunc, IDictionary<string, object>? properties = null)
        {
            List<byte[]> encryptedBlocks = new List<byte[]>(new byte[blocks.Count][]);
            var tasks = new List<Task>();
            for (int i = 0; i < blocks.Count; i++)
            {
                int idx = i;
                var task = Task.Run(() =>
                {
                    byte[] block = decryptFunc(blocks[idx], key);
                    encryptedBlocks[idx] = block;
                });
                tasks.Add(task);
                //byte[] block = decryptFunc(blocks[i], key);
                //encryptedBlocks.Add(block);
            }
            Task.WhenAll(tasks).Wait();
            return encryptedBlocks;
        }

        public List<byte[]> Decrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> encryptFunc, IDictionary<string, object>? properties = null)
        {
            List<byte[]> decryptedBlocks = new List<byte[]>(new byte[blocks.Count][]);
            var tasks = new List<Task>();
            for (int i = 0; i < blocks.Count; i++)
            {
                int idx = i;
                var task = Task.Run(() =>
                {
                    byte[] block = encryptFunc(blocks[idx], key);
                    decryptedBlocks[idx] = block;
                });
                tasks.Add(task);
                //byte[] block = encryptFunc(blocks[i], key);
                //decryptedBlocks.Add(block);
            }
            Task.WhenAll(tasks).Wait();
            return decryptedBlocks;
        }
    }
}

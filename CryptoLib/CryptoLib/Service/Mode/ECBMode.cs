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
        public object Service { get; set; }
        public ECBMode(object service)
        {
            Service = service;
        }

        public List<byte[]> BlockDecrypt(List<byte[]> blocks, IKey key)
        {
            IDecryptor? decryptor = Service as IDecryptor;
            if (decryptor == null)
            {
                throw new InvalidOperationException();
            }

            List<byte[]> decryptedBlocks = new List<byte[]>(blocks.Count);
            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] block = decryptor.Decrypt(blocks[i], key);
                decryptedBlocks.Add(block);
            }

            return decryptedBlocks;
        }

        public List<byte[]> BlockEncrypt(List<byte[]> blocks, IKey key)
        {
            IEncryptor? encryptor = Service as IEncryptor;
            if (encryptor == null) 
            {
                throw new InvalidOperationException();
            }

            List<byte[]> encryptedBlocks = new List<byte[]>(blocks.Count);

            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] block = encryptor.Encrypt(blocks[i], key);
                encryptedBlocks.Add(block);
            }

            return encryptedBlocks;
        }
    }
}

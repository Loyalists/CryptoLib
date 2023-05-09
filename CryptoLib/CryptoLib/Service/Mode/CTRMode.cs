using CryptoLib.Algorithm.Key;
using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Mode
{
    public class CTRMode : IBlockCipherMode
    {
        public List<byte[]> Encrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> encryptFunc, IDictionary<string, object>? properties = null)
        {
            if (properties == null)
            {
                throw new InvalidOperationException();
            }

            byte[] IV = (byte[])properties["IV"];
            List<byte[]> encryptedBlocks = new List<byte[]>(blocks.Count);

            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] counter = BitConverter.GetBytes((ulong)i);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(counter);
                }
                //BigInteger _iv = new BigInteger(IV, true, true);
                //BigInteger _counter = new BigInteger(counter, true, true);
                //byte[] input = (_iv + _counter).ToByteArray(true, true);
                byte[] input = IV.XORBytes(counter);
                byte[] text = blocks[i];
                byte[] encrypted = encryptFunc(input, key);
                byte[] xor = text.XORBytes(encrypted);
                encryptedBlocks.Add(xor);
            }

            return encryptedBlocks;
        }

        public List<byte[]> Decrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> decryptFunc, IDictionary<string, object>? properties = null)
        {
            if (properties == null)
            {
                throw new InvalidOperationException();
            }

            byte[] IV = (byte[])properties["IV"];
            List<byte[]> decryptedBlocks = new List<byte[]>(blocks.Count);

            for (int i = 0; i < blocks.Count; i++)
            {
                byte[] counter = BitConverter.GetBytes((ulong)i);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(counter);
                }

                //BigInteger _iv = new BigInteger(IV, true, true);
                //BigInteger _counter = new BigInteger(counter, true, true);
                //byte[] input = (_iv + _counter).ToByteArray(true, true);
                byte[] input = IV.XORBytes(counter);
                byte[] text = blocks[i];
                byte[] decrypted = decryptFunc(input, key);
                byte[] xor = text.XORBytes(decrypted);
                decryptedBlocks.Add(xor);
            }

            return decryptedBlocks;
        }
    }
}

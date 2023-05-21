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
            List<byte[]> encryptedBlocks = new List<byte[]>(new byte[blocks.Count][]);
            var tasks = new List<Task>();

            for (int i = 0; i < blocks.Count; i++)
            {
                int idx = i;
                var task = Task.Run(() =>
                {
                    byte[] counter = BitConverter.GetBytes((ulong)idx);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(counter);
                    }

                    //BigInteger _iv = new BigInteger(IV, true, true);
                    //BigInteger _counter = new BigInteger(counter, true, true);
                    //byte[] input = (_iv + _counter).ToByteArray(true, true);
                    byte[] input = IV.XORBytes(counter);
                    byte[] text = blocks[idx];
                    byte[] encrypted = encryptFunc(input, key);
                    byte[] xor = text.XORBytes(encrypted);
                    encryptedBlocks[idx] = xor;
                });
                tasks.Add(task);
            }

            Task.WhenAll(tasks).Wait();
            return encryptedBlocks;
        }

        public List<byte[]> Decrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> decryptFunc, IDictionary<string, object>? properties = null)
        {
            if (properties == null)
            {
                throw new InvalidOperationException();
            }

            byte[] IV = (byte[])properties["IV"];
            List<byte[]> decryptedBlocks = new List<byte[]>(new byte[blocks.Count][]);
            var tasks = new List<Task>();

            for (int i = 0; i < blocks.Count; i++)
            {
                int idx = i;
                var task = Task.Run(() =>
                {
                    byte[] counter = BitConverter.GetBytes((ulong)idx);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(counter);
                    }

                    //BigInteger _iv = new BigInteger(IV, true, true);
                    //BigInteger _counter = new BigInteger(counter, true, true);
                    //byte[] input = (_iv + _counter).ToByteArray(true, true);
                    byte[] input = IV.XORBytes(counter);
                    byte[] text = blocks[idx];
                    byte[] decrypted = decryptFunc(input, key);
                    byte[] xor = text.XORBytes(decrypted);
                    decryptedBlocks[idx] = xor;
                });
                tasks.Add(task);

            }
            Task.WhenAll(tasks).Wait();
            return decryptedBlocks;
        }
    }
}

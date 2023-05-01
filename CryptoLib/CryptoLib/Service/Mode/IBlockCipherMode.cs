using CryptoLib.Algorithm.Key;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Mode
{
    public interface IBlockCipherMode
    {
        List<byte[]> Encrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> func);
        List<byte[]> Decrypt(List<byte[]> blocks, IKey key, Func<byte[], IKey, byte[]> func);
    }
}

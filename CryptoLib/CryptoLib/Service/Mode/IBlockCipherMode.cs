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
        List<byte[]> BlockEncrypt(List<byte[]> blocks, IKey key);
        List<byte[]> BlockDecrypt(List<byte[]> blocks, IKey key);
    }
}

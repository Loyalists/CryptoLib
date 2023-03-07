using CryptoLib.Algorithm.Key;
using CryptoLib.Service.Padding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service
{
    public interface IDecryptor
    {
        byte[] Decrypt(byte[] data, IKey key, IPaddingScheme? padding = null);
    }
}

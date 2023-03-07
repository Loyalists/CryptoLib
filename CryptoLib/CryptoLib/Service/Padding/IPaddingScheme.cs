using CryptoLib.Algorithm.Key;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Padding
{
    public interface IPaddingScheme
    {
        byte[] Encode(byte[] data, IKey key);
        byte[] Decode(byte[] data, IKey key);
    }
}

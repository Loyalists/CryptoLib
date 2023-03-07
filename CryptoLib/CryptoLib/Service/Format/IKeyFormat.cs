using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoLib.Algorithm.Key;

namespace CryptoLib.Service.Format
{
    public interface IKeyFormat
    {
        string ToString(IKey key, bool isFormatted);
        byte[] ToByteArray(IKey key);
        IKey FromByteArray(byte[] bytes);
        IKey FromString(string str);
    }
}

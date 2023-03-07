using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoLib.Service.Format;

namespace CryptoLib.Algorithm.Key
{
    public interface IKey
    {
        string ToString(IKeyFormat? keyFormat, bool isFormatted);
        byte[] ToByteArray(IKeyFormat? keyFormat);
    }
}

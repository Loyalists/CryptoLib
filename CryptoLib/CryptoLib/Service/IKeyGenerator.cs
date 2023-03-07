using CryptoLib.Algorithm.Key;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service
{
    public interface IKeyGenerator<T> where T : Enum
    {
        Dictionary<T, IKey> Generate();
    }
}

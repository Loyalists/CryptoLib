using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CryptoLib.Service.Format;
using CryptoLib.Utility;

namespace CryptoLib.Algorithm.Key
{
    public class RSAPublicKey : IKey
    {
        public BigInteger Modulus { get; set; }
        public BigInteger PublicExponent { get; set; }

        public byte[] ToByteArray(IKeyFormat? keyFormat)
        {
            if (keyFormat == null)
            {
                throw new InvalidOperationException();
            }

            return keyFormat.ToByteArray(this);
        }

        public string ToString(IKeyFormat? keyFormat, bool isFormatted = true)
        {
            if (keyFormat == null)
            {
                throw new InvalidOperationException();
            }

            return keyFormat.ToString(this, isFormatted);
        }

        public static RSAPublicKey FromString(string formatted, IKeyFormat? keyFormat)
        {
            if (keyFormat == null)
            {
                throw new InvalidOperationException();
            }

            RSAPublicKey key = (RSAPublicKey)keyFormat.FromString(formatted);
            return key;
        }

        public int GetKeySize()
        {
            return Modulus.GetByteCount(true) * 8;
            //return (int)Modulus.GetBitLength();
        }
    }
}

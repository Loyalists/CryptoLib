using CryptoLib.Service.Format;
using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Algorithm.Key
{
    public class RSAPrivateKey : IKey
    {
        public int Version { get; set; } = 0;
        public BigInteger Modulus { get; set; } //n
        public BigInteger PublicExponent { get; set; } //e
        public BigInteger PrivateExponent { get; set; } //d
        public BigInteger Prime1 { get; set; } //p
        public BigInteger Prime2 { get; set; } //q
        public BigInteger Exponent1 { get; set; } //d mod (p-1)
        public BigInteger Exponent2 { get; set; } //d mod (q-1)
        public BigInteger Coefficient { get; set; } //q^(-1) mod p
        public List<object>? OtherPrimeInfos { get; set; } //optional

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

        public static RSAPrivateKey FromString(string formatted, IKeyFormat? keyFormat)
        {
            if (keyFormat == null)
            {
                throw new InvalidOperationException();
            }

            RSAPrivateKey key = (RSAPrivateKey)keyFormat.FromString(formatted);
            return key;
        }

        public int GetKeySize()
        {
            //return Modulus.GetByteCount(true) * 8;
            return (int)Modulus.GetBitLength();
        }
    }
}

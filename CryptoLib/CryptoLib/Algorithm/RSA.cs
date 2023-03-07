using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Algorithm
{
    public static class RSA
    {
        public static BigInteger GetLambda(BigInteger a, BigInteger b)
        {
            var numerator = BigInteger.Abs((a - 1) * (b - 1));
            var gcd = BigInteger.GreatestCommonDivisor(a - 1, b - 1);
            BigInteger lambada = numerator / gcd;
            return lambada;
        }

        public static BigInteger GetModulus(BigInteger a, BigInteger b)
        {
            BigInteger result = a * b;
            return result;
        }

        public static BigInteger GetPublicKeyExponent(BigInteger lambda)
        {
            // https://en.wikipedia.org/wiki/RSA_(cryptosystem)#Key_generation
            BigInteger result = 65537;
            return result;
        }

        public static BigInteger GetPrivateKeyExponent(BigInteger e, BigInteger lambda)
        {
            BigInteger d = MathHelper.ExtendedEuclidean(e, lambda);
            return d;
        }

        public static BigInteger GetExponent(BigInteger a, BigInteger b)
        {
            BigInteger result = a % (b - 1);
            return result;
        }

        public static BigInteger GetCoefficient(BigInteger a, BigInteger b)
        {
            //BigInteger result = BigInteger.ModPow(a, b - 2, b);
            BigInteger result = MathHelper.ExtendedEuclidean(a, b);
            return result;
        }

        public static BigInteger Encrypt(BigInteger encodedText, BigInteger publicKeyExponent, BigInteger modulus)
        {
            BigInteger c = BigInteger.ModPow(encodedText, publicKeyExponent, modulus);
            return c;
        }

        public static BigInteger Decrypt(BigInteger encryptedText, BigInteger privateKeyExponent, BigInteger modulus)
        {
            BigInteger m = BigInteger.ModPow(encryptedText, privateKeyExponent, modulus);
            return m;
        }
    }
}

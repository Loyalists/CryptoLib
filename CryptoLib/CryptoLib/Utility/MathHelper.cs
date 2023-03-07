using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Utility
{
    public static class MathHelper
    {
        // https://stackoverflow.com/questions/20802857/xor-function-for-two-hex-byte-arrays
        public static byte[] XORByteArray(byte[] key, byte[] PAN)
        {
            if (key.Length != PAN.Length)
            {
                throw new ArgumentException();
            }

            byte[] result = new byte[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                result[i] = (byte)(key[i] ^ PAN[i]);
            }
            return result;
        }

        public static byte[] GetRandomBytes(int n)
        {
            var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[n];
            rng.GetBytes(bytes);
            return bytes;
        }

        public static byte[] GetRandomBytesWithoutZero(int n)
        {
            byte[] bytes = GetRandomBytes(n);
            for (int i = 0; i < n; i++)
            {
                if (bytes[i] == 0x00)
                {
                    bytes[i] = 0x01;
                }
            }
            return bytes;
        }

        public static BigInteger Randomize(int bitSize)
        {
            // https://stackoverflow.com/questions/2965707/c-sharp-a-random-bigint-generator
            var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[bitSize / 8];
            rng.GetBytes(bytes);

            BigInteger p = new BigInteger(bytes);
            BigInteger bitwise = BigInteger.Pow(new BigInteger(2), bitSize - 1) + 1;
            p = p | bitwise;
            return p;
        }

        public static BigInteger GetRandomPrime(int n)
        {
            BigInteger num = BigInteger.One;
            bool test = false;
            while (true)
            {
                if (test)
                {
                    break;
                }

                num = Randomize(n);

                if (num < 0)
                {
                    continue;
                }

                test = RabinMiller.IsPrime(num, 2);
            }
            return num;
        }

        public static BigInteger ExtendedEuclidean(BigInteger a, BigInteger b)
        {
            // https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm#Pseudocode
            // ax+by=gcd(a,b)
            var old_r = a;
            var r = b;
            var old_s = BigInteger.One;
            var s = BigInteger.Zero;
            var old_t = BigInteger.Zero;
            var t = BigInteger.One;

            while (r != 0)
            {
                BigInteger quotient = old_r / r;
                var new_r = old_r - quotient * r;
                old_r = r;
                r = new_r;

                var new_s = old_s - quotient * s;
                old_s = s;
                s = new_s;

                var new_t = old_t - quotient * t;
                old_t = t;
                t = new_t;
            }

            if (old_s < 0)
            {
                old_s += b;
            }
            return old_s;
        }

        public static int GetDigits(BigInteger n)
        {
            int digits = (int)Math.Floor(BigInteger.Log10(n) + 1);
            return digits;
        }

        public static string HexToBinary(string hexValue, string separator = "")
        {
            // https://stackoverflow.com/questions/9482420/c-sharp-hex-to-bit-conversion
            var lup = new Dictionary<char, string>{
            { '0', "0000"},
            { '1', "0001"},
            { '2', "0010"},
            { '3', "0011"},

            { '4', "0100"},
            { '5', "0101"},
            { '6', "0110"},
            { '7', "0111"},

            { '8', "1000"},
            { '9', "1001"},
            { 'A', "1010"},
            { 'B', "1011"},

            { 'C', "1100"},
            { 'D', "1101"},
            { 'E', "1110"},
            { 'F', "1111"}};

            var ret = string.Join(separator, from character in hexValue
                                             select lup[character]);
            return ret;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Utility
{
    public static class BitHelper
    {
        // https://stackoverflow.com/questions/4854207/get-a-specific-bit-from-byte
        public static bool GetBit(this byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        // https://stackoverflow.com/questions/5283180/how-can-i-convert-bitarray-to-single-int
        public static T ConvertBitsToNumeric<T>(BitArray bits, bool isBigEndian = false)
        {
            if (bits.Length > 32)
            {
                throw new ArgumentException("Argument length shall be at most 32 bits.");
            }

            BitArray ba = bits;
            if (isBigEndian)
            {
                ba = new BitArray(bits.Cast<bool>().Reverse().ToArray());
            }

            T[] bytes = new T[1];
            ba.CopyTo(bytes, 0);
            return bytes[0];
        }

        public static bool[] ConvertBitsToBoolArray(BitArray bits)
        {
            bool[] ret = new bool[bits.Length];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static byte[] ConvertBitsToBytes(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static List<BitArray> SplitBitsByCount(this BitArray source, int chunkMaxSize)
        {
            bool[] _block = ConvertBitsToBoolArray(source);
            List<bool[]> split = Helper.SplitByCount(_block, chunkMaxSize);
            List<BitArray> bitArrays = new List<BitArray>(split.Count);
            for (int i = 0; i < split.Count; i++)
            {
                bitArrays.Add(new BitArray(split[i]));
            }
            return bitArrays;
        }
    }
}

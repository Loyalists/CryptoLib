using CryptoLib.Algorithm;
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
        // https://github.com/pStrachota/DES_KRYPTO_PROJECT
        public static byte[] SetBit(this byte[] self, int index, bool value)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;
            int val = Convert.ToInt32(value);

            byte oldByte = self[byteIndex];
            oldByte = (byte)(((0xFF7F >> bitIndex) & oldByte) & 0x00FF);
            byte newByte = (byte)((val << (7 - bitIndex)) | oldByte);
            self[byteIndex] = newByte;
            return self;
        }

        public static bool GetBit(this byte[] self, int index)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;
            byte valByte = self[byteIndex];
            int valInt = valByte >> (7 - bitIndex) & 1;
            bool b = Convert.ToBoolean(valInt);
            return b;
        }

        public static byte[] SelectBits(byte[] inner, int pos, int len)
        {
            int numOfBytes = (len - 1) / 8 + 1;
            byte[] outer = new byte[numOfBytes];
            for (int i = 0; i < len; i++)
            {
                bool val = inner.GetBit(pos + i);
                outer.SetBit(i, val);
            }

            return outer;
        }

        public static byte[] LeftRotate(byte[] inner, int len, int step)
        {
            byte[] outer = new byte[(len - 1) / 8 + 1];
            for (int i = 0; i < len; i++)
            {
                bool val = inner.GetBit((i + step) % len);
                outer.SetBit(i, val);
            }

            return outer;
        }

        // https://stackoverflow.com/questions/5283180/how-can-i-convert-bitarray-to-single-int
        public static T ConvertBitsToNumeric<T>(this BitArray bits, bool isBigEndian = false)
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

        public static bool[] ConvertBitsToBools(this BitArray bits)
        {
            bool[] ret = new bool[bits.Length];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static byte[] ConvertBitsToBytes(this BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static List<BitArray> SplitBitsByCount(this BitArray source, int chunkMaxSize)
        {
            bool[] _block = source.ConvertBitsToBools();
            List<bool[]> split = Helper.SplitByCount(_block, chunkMaxSize);
            List<BitArray> bitArrays = new List<BitArray>(split.Count);
            for (int i = 0; i < split.Count; i++)
            {
                bitArrays.Add(new BitArray(split[i]));
            }
            return bitArrays;
        }

        public static void PrintBitArray(BitArray bits)
        {
            byte[] bytes = bits.ConvertBitsToBytes();
            Console.WriteLine(Convert.ToHexString(bytes));
        }
    }
}

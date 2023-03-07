using CryptoLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Algorithm
{
    public static class DES
    {
        public static BitArray Encrypt(BitArray plainText, BitArray key, bool isDecryption = false)
        {
            if (plainText.Count != 64)
            {
                throw new InvalidOperationException();
            }

            if (key.Count != 64)
            {
                throw new InvalidOperationException();
            }

            List<BitArray> subkeys = KeySchedule(key);
            if (isDecryption)
            {
                subkeys.Reverse();
            }

            BitArray leftBlock;
            BitArray rightBlock;
            InitialPermutation(plainText, out leftBlock, out rightBlock);
            for (int i = 0; i < subkeys.Count; i++)
            {
                BitArray subkey = subkeys[i];
                BitArray retLeftBlock;
                BitArray retRightBlock;
                EncryptRound(leftBlock, rightBlock, subkey, out retLeftBlock, out retRightBlock);
                leftBlock = retLeftBlock;
                rightBlock = retRightBlock;
            }

            BitArray result = FinalPermutation(rightBlock, leftBlock);
            return result;
        }

        public static BitArray Decrypt(BitArray plainText, BitArray key)
        {
            BitArray result = Encrypt(plainText, key, true);
            return result;
        }

        private static void EncryptRound(BitArray leftBlock, BitArray rightBlock, BitArray subkey, out BitArray retLeftBlock, out BitArray retRightBlock)
        {
            BitArray result = leftBlock.Xor(FeistelFunction(rightBlock, subkey));
            retLeftBlock = new BitArray(rightBlock);
            retRightBlock = result;
        }

        public static void InitialPermutation(BitArray block, out BitArray leftBlock, out BitArray rightBlock)
        {
            BitArray permuted = new BitArray(block.Length);
            for (int i = 0; i < block.Length; i++)
            {
                int position = IPTable[i] - 1;
                permuted[i] = block[position];
            }

            List<BitArray> bitArrays = BitHelper.SplitBitsByCount(permuted, permuted.Length / 2);
            leftBlock = bitArrays[0];
            rightBlock = bitArrays[1];
        }

        public static BitArray FinalPermutation(BitArray leftBlock, BitArray rightBlock)
        {
            bool[] _L = BitHelper.ConvertBitsToBoolArray(leftBlock);
            bool[] _R = BitHelper.ConvertBitsToBoolArray(rightBlock);
            bool[] _merged = _L.Concat(_R).ToArray();
            BitArray merged = new BitArray(_merged);
            BitArray permuted = new BitArray(merged.Length);
            for (int i = 0; i < merged.Length; i++)
            {
                int position = FPTable[i] - 1;
                permuted[i] = merged[position];
            }
            return permuted;
        }

        public static BitArray FeistelFunction(BitArray block, BitArray subkey)
        {
            BitArray expanded = Expansion(block);
            BitArray mixed = KeyMixing(expanded, subkey);
            BitArray sub = Substitution(mixed);
            BitArray permuted = Permutation(sub);
            return permuted;
        }

        public static BitArray Expansion(BitArray block)
        {
            int size = 48;
            BitArray permuted = new BitArray(size);
            for (int i = 0; i < size; i++)
            {
                int position = ETable[i] - 1;
                permuted[i] = block[position];
            }
            return permuted;
        }

        public static BitArray KeyMixing(BitArray block, BitArray subkey)
        {
            BitArray xor = block.Xor(subkey);
            return xor;
        }

        public static BitArray Substitution(BitArray block)
        {
            List<BitArray> blocks = BitHelper.SplitBitsByCount(block, 6);
            if (blocks.Count != 8)
            {
                throw new Exception();
            }

            List<bool> boolList = new List<bool>(32);
            for (int i = 0; i < blocks.Count; i++)
            {
                BitArray output = SubstitutionLookup(blocks[i], Sboxes[i]);
                boolList.AddRange(BitHelper.ConvertBitsToBoolArray(output));
            }

            if (boolList.Count != 32)
            {
                throw new Exception();
            }

            BitArray result = new BitArray(boolList.ToArray());
            return result;
        }

        private static BitArray SubstitutionLookup(BitArray block, byte[,] sbox)
        {
            if (block.Length != 6)
            {
                throw new Exception();
            }

            BitArray outer = new BitArray(new bool[] { block[0], block[5] });
            BitArray inner = new BitArray(new bool[] { block[1], block[2], block[3], block[4] });
            byte row = BitHelper.ConvertBitsToNumeric<byte>(outer, true);
            byte column = BitHelper.ConvertBitsToNumeric<byte>(inner, true);
            byte target = sbox[row, column];
            BitArray result = new BitArray(new byte[] { target });
            result.Length = 4;
            return result;
        }

        public static BitArray Permutation(BitArray block)
        {
            BitArray permuted = new BitArray(block.Count);
            for (int i = 0; i < block.Count; i++)
            {
                int position = Pbox[i] - 1;
                permuted[i] = block[position];
            }

            return permuted;
        }

        public static List<BitArray> KeySchedule(BitArray key)
        {
            bool[] leftKey = new bool[28];
            bool[] rightKey = new bool[28];
            for (int i = 0; i < leftKey.Length; i++)
            {
                int positionL = PC1LTable[i] - 1;
                int positionR = PC1RTable[i] - 1;
                leftKey[i] = key[positionL];
                rightKey[i] = key[positionR];
            }

            int subkeyCount = 16;
            List<BitArray> subkeys = new List<BitArray>(subkeyCount);
            for (int i = 0; i < subkeyCount; i++)
            {
                int rotation = BitsRotation[i];
                Helper.LeftRotateArray(leftKey, rotation);
                Helper.LeftRotateArray(rightKey, rotation);
                BitArray subkey = KeyScheduleGetSubkey(leftKey, rightKey);
                subkeys.Add(subkey);
            }
            return subkeys;
        }

        private static BitArray KeyScheduleGetSubkey(bool[] leftKey, bool[] rightKey)
        {
            List<bool> _key = new List<bool>(leftKey.Length + rightKey.Length);
            _key.AddRange(leftKey);
            _key.AddRange(rightKey);
            int subkeySize = 48;
            bool[] subkey = new bool[subkeySize];
            for (int i = 0; i < subkeySize; i++)
            {
                int position = PC2Table[i] - 1;
                subkey[i] = _key[position];
            }
            BitArray subkeyBA = new BitArray(subkey);
            return subkeyBA;
        }

        public static readonly byte[,] Sbox1 = new byte[,]
        {
            { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7, },
            { 0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8, },
            { 4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0, },
            { 15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13, },
        };

        public static readonly byte[,] Sbox2 = new byte[,]
        {
            { 15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10, },
            { 3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5, },
            { 0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15, },
            { 13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9, },
        };

        public static readonly byte[,] Sbox3 = new byte[,]
        {
            { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8, },
            { 13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1, },
            { 13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7 },
            { 1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12, },
        };

        public static readonly byte[,] Sbox4 = new byte[,]
        {
            { 7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15, },
            { 13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9, },
            { 10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4, },
            { 3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14, },
        };

        public static readonly byte[,] Sbox5 = new byte[,]
        {
            { 2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9, },
            { 14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6, },
            { 4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14, },
            { 11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3, },
        };

        public static readonly byte[,] Sbox6 = new byte[,]
        {
            { 12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11, },
            { 10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8, },
            { 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6, },
            { 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13, },
        };

        public static readonly byte[,] Sbox7 = new byte[,]
        {
            { 4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1, },
            { 13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6, },
            { 1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2, },
            { 6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12, },
        };

        public static readonly byte[,] Sbox8 = new byte[,]
        {
            { 13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7, },
            { 1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2, },
            { 7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8, },
            { 2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11, },
        };

        public static readonly List<byte[,]> Sboxes = new List<byte[,]>()
        {
            Sbox1, Sbox2, Sbox3, Sbox4,
            Sbox5, Sbox6, Sbox7, Sbox8,
        };

        public static readonly int[] IPTable = new int[]
        {
            58, 50, 42, 34, 26, 18, 10, 2,
            60, 52, 44, 36, 28, 20, 12, 4,
            62, 54, 46, 38, 30, 22, 14, 6,
            64, 56, 48, 40, 32, 24, 16, 8,
            57, 49, 41, 33, 25, 17, 9, 1,
            59, 51, 43, 35, 27, 19, 11, 3,
            61, 53, 45, 37, 29, 21, 13, 5,
            63, 55, 47, 39, 31, 23, 15, 7,
        };

        public static readonly int[] FPTable = new int[]
        {
            40, 8, 48, 16, 56, 24, 64, 32,
            39, 7, 47, 15, 55, 23, 63, 31,
            38, 6, 46, 14, 54, 22, 62, 30,
            37, 5, 45, 13, 53, 21, 61, 29,
            36, 4, 44, 12, 52, 20, 60, 28,
            35, 3, 43, 11, 51, 19, 59, 27,
            34, 2, 42, 10, 50, 18, 58, 26,
            33, 1, 41, 9, 49, 17, 57, 25,
        };

        public static readonly int[] ETable = new int[]
        {
            32, 1, 2, 3, 4, 5,
            4, 5, 6, 7, 8, 9,
            8, 9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32, 1,
        };

        public static readonly int[] Pbox = new int[]
        {
            16, 7, 20, 21, 29, 12, 28, 17,
            1, 15, 23, 26, 5, 18, 31, 10,
            2, 8, 24, 14, 32, 27, 3, 9,
            19, 13, 30, 6, 22, 11, 4, 25,
        };

        public static readonly int[] PC1LTable = new int[]
        {
            57, 49, 41, 33, 25, 17, 9,
            1, 58, 50, 42, 34, 26, 18,
            10, 2, 59, 51, 43, 35, 27,
            19, 11, 3, 60, 52, 44, 36,
        };

        public static readonly int[] PC1RTable = new int[]
        {
            63, 55, 47, 39, 31, 23, 15,
            7, 62, 54, 46, 38, 30, 22,
            14, 6, 61, 53, 45, 37, 29,
            21, 13, 5, 28, 20, 12, 4,
        };

        public static readonly int[] PC2Table = new int[]
        {
            14, 17, 11, 24, 1, 5,
            3, 28, 15, 6, 21, 10,
            23, 19, 12, 4, 26, 8,
            16, 7, 27, 20, 13, 2,
            41, 52, 31, 37, 47, 55,
            30, 40, 51, 45, 33, 48,
            44, 49, 39, 56, 34, 53,
            46, 42, 50, 36, 29, 32,
        };

        public static readonly int[] BitsRotation = new int[]
        {
            1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1
        };

        public static readonly int[,] PermutationMatrix = new int[,]
        {
            { 57, 49, 41, 33, 25, 17, 9, 1 },
            { 58, 50, 42, 34, 26, 18, 10, 2 },
            { 59, 51, 43, 35, 27, 19, 11, 3 },
            { 60, 52, 44, 36, 63, 55, 47, 39 },
            { 31, 23, 15, 7, 62, 54, 46, 38 },
            { 30, 22, 14, 6, 61, 53, 45, 37 },
            { 29, 21, 13, 5, 28, 20, 12, 4 },
        };

        public static readonly int[,] KeyCompressionMatrix = new int[,]
        {
            { 14, 17, 11, 24, 1, 5 },
            { 3, 28, 15, 6, 21, 10 },
            { 23, 19, 12, 4, 26, 8 },
            { 16, 7, 27, 20, 13, 2 },
            { 41, 52, 31, 37, 47, 55 },
            { 30, 40, 51, 45, 33, 48 },
            { 44, 49, 39, 56, 34, 53 },
            { 46, 42, 50, 36, 29, 32 },
        };
    }
}

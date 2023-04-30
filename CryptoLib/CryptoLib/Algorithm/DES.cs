using CryptoLib.Algorithm.Key;
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
        public static byte[] Encrypt(byte[] plainText, byte[] key, bool isDecryption = false)
        {
            if (plainText.Length != 8)
            {
                throw new InvalidOperationException();
            }

            if (key.Length != 8)
            {
                throw new InvalidOperationException();
            }

            List<byte[]> subkeys = KeySchedule(key);
            if (isDecryption)
            {
                subkeys.Reverse();
            }

            byte[] leftBlock;
            byte[] rightBlock;
            InitialPermutation(plainText, out leftBlock, out rightBlock);
            for (int i = 0; i < subkeys.Count; i++)
            {
                byte[] subkey = subkeys[i];
                byte[] retLeftBlock;
                byte[] retRightBlock;
                EncryptRound(leftBlock, rightBlock, subkey, out retLeftBlock, out retRightBlock);
                leftBlock = retLeftBlock;
                rightBlock = retRightBlock;
            }

            byte[] result = FinalPermutation(rightBlock, leftBlock);
            return result;
        }

        public static byte[] Decrypt(byte[] plainText, byte[] key)
        {
            byte[] result = Encrypt(plainText, key, true);
            return result;
        }

        private static void EncryptRound(byte[] leftBlock, byte[] rightBlock, byte[] subkey, out byte[] retLeftBlock, out byte[] retRightBlock)
        {
            byte[] f = FeistelFunction(rightBlock, subkey);
            byte[] result = MathHelper.XORByteArray(leftBlock, f);
            retLeftBlock = new byte[rightBlock.Length];
            rightBlock.CopyTo(retLeftBlock, 0);
            retRightBlock = result;
        }

        public static void InitialPermutation(byte[] block, out byte[] leftBlock, out byte[] rightBlock)
        {
            int bitSize = block.Length * 8;
            byte[] permuted = new byte[block.Length];
            for (int i = 0; i < bitSize; i++)
            {
                int position = IPTable[i] - 1;
                bool value = block.GetBit(position);
                permuted.SetBit(i, value);
            }
            leftBlock = BitHelper.SelectBits(permuted, 0, bitSize / 2);
            rightBlock = BitHelper.SelectBits(permuted, bitSize / 2, bitSize / 2);
        }

        public static byte[] FinalPermutation(byte[] leftBlock, byte[] rightBlock)
        {
            int byteSize = leftBlock.Length + rightBlock.Length;
            int bitSize = byteSize * 8;
            byte[] merged = leftBlock.Concat(rightBlock).ToArray();
            byte[] permuted = new byte[byteSize];
            for (int i = 0; i < bitSize; i++)
            {
                int position = FPTable[i] - 1;
                bool value = merged.GetBit(position);
                permuted.SetBit(i, value);
            }
            return permuted;
        }

        public static byte[] FeistelFunction(byte[] block, byte[] subkey)
        {
            byte[] expanded = Expansion(block);
            byte[] mixed = KeyMixing(expanded, subkey);
            byte[] sub = Substitution(mixed);
            byte[] permuted = Permutation(sub);
            return permuted;
        }

        public static byte[] Expansion(byte[] block)
        {
            int byteSize = 6;
            int bitSize = byteSize * 8;
            byte[] permuted = new byte[byteSize];
            for (int i = 0; i < bitSize; i++)
            {
                int position = ETable[i] - 1;
                bool value = block.GetBit(position);
                permuted.SetBit(i, value);
            }
            return permuted;
        }

        public static byte[] KeyMixing(byte[] block, byte[] subkey)
        {
            byte[] xor = MathHelper.XORByteArray(block, subkey);
            return xor;
        }

        public static byte[] Substitution(byte[] block)
        {
            int byteSize = 4;
            int bitSize = byteSize * 8;
            int blockCount = 8;
            List<BitArray> blocks = new List<BitArray>(blockCount);
            for (int i = 0; i < blockCount; i++)
            {
                bool[] num = new bool[6];
                for (int j = 0; j < 6; j++)
                {
                    bool value = block.GetBit(i * 6 + j);
                    num[j] = value;
                }
                BitArray blockBA = new BitArray(num);
                blocks.Add(blockBA);
            }

            byte[] result = new byte[byteSize];
            List<byte> outputs = new List<byte>(8);
            for (int i = 0; i < blockCount; i++)
            {
                byte output = SubstitutionLookup(blocks[i], Sboxes[i]);
                outputs.Add(output);
            }

            int idx = 0;
            int[] idx_list = new int[] { 0, 2, 4, 6 };
            foreach (int i in idx_list)
            {
                byte num = (byte)(outputs[i] << 4 | outputs[i + 1]);
                result[idx] = num;
                idx++;
            }

            return result;
        }

        private static byte SubstitutionLookup(BitArray block, byte[,] sbox)
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
            return target;
        }

        public static byte[] Permutation(byte[] block)
        {
            int byteSize = 4;
            int bitSize = byteSize * 8;
            byte[] permuted = new byte[byteSize];
            for (int i = 0; i < bitSize; i++)
            {
                int position = Pbox[i] - 1;
                bool value = block.GetBit(position);
                permuted.SetBit(i, value);
            }

            return permuted;
        }

        public static List<byte[]> KeySchedule(byte[] key)
        {
            int keySize = 7;
            byte[] LRKey = new byte[keySize];
            for (int i = 0; i < keySize * 8; i++)
            {
                int position = PC1Table[i] - 1;
                bool value = key.GetBit(position);
                LRKey.SetBit(i, value);
            }

            byte[] leftKey = BitHelper.SelectBits(LRKey, 0, 28);
            byte[] rightKey = BitHelper.SelectBits(LRKey, 28, 28);

            int subkeyCount = 16;
            List<byte[]> subkeys = new List<byte[]>(subkeyCount);
            for (int i = 0; i < subkeyCount; i++)
            {
                int rotation = BitsRotation[i];
                leftKey = BitHelper.LeftRotate(leftKey, 28, rotation);
                rightKey = BitHelper.LeftRotate(rightKey, 28, rotation);
                byte[] subkey = GetSubkey(leftKey, rightKey);
                subkeys.Add(subkey);
            }
            return subkeys;
        }

        private static byte[] GetSubkey(byte[] leftKey, byte[] rightKey)
        {
            byte[] concat = MergeKey(leftKey, rightKey);
            int subkeySize = 6;
            byte[] subkey = new byte[subkeySize];
            for (int i = 0; i < subkeySize * 8; i++)
            {
                int position = PC2Table[i] - 1;
                bool value = concat.GetBit(position);
                subkey.SetBit(i, value);
            }
            return subkey;
        }

        private static byte[] MergeKey(byte[] leftKey, byte[] rightKey)
        {
            byte[] result = new byte[7];
            for (int i = 0; i < 3; i++)
            {
                result[i] = leftKey[i];
            }
            for (int i = 0; i < 4; i++)
            {
                bool val = leftKey.GetBit(24 + i);
                result.SetBit(24 + i, val);
            }
            for (int i = 0; i < 28; i++)
            {
                bool val = rightKey.GetBit(i);
                result.SetBit(28 + i, val);
            }
            return result;
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

        public static readonly int[] PC1Table = new int[]
        {
            57, 49, 41, 33, 25, 17, 9,
            1, 58, 50, 42, 34, 26, 18,
            10, 2, 59, 51, 43, 35, 27,
            19, 11, 3, 60, 52, 44, 36,
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
    }
}

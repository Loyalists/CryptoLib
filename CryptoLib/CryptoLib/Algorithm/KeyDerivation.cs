using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Algorithm
{
    public static class KeyDerivation
    {
        // https://datatracker.ietf.org/doc/html/rfc2898#section-5.2
        public static byte[] PBKDF2(byte[] password, byte[] salt, uint iteration, uint keyLength)
        {
            uint hLen = HMACSHA256.HashSizeInBytes;
            uint l = (uint)Math.Ceiling((float)keyLength / hLen);
            uint r = keyLength - (l - 1) * hLen;
            List<byte> bytes = new List<byte>();
            for (uint i = 1; i <= l; i++)
            {
                byte[] block = PBKDF2_F(password, salt, iteration, i);
                bytes.AddRange(block);
            }

            byte[] bytesArray = bytes.ToArray();
            byte[] result = new byte[keyLength];
            Array.Copy(bytesArray, result, keyLength);
            return result;
        }

        private static byte[] PBKDF2_F(byte[] password, byte[] salt, uint iteration, uint blockIndex)
        {
            byte[] INT_32_BE = BitConverter.GetBytes(blockIndex);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(INT_32_BE);
            }

            byte[] u_init = salt.Concat(INT_32_BE).ToArray();
            List<byte[]> uList = new List<byte[]>();

            for (int i = 0; i < iteration; i++)
            {
                if (i == 0)
                {
                    byte[] u1 = HMACSHA256.HashData(password, u_init);
                    uList.Add(u1);
                    continue;
                }
                byte[] u = HMACSHA256.HashData(password, uList[i - 1]);
                uList.Add(u);
            }

            byte[] xor_result = uList[0];

            for (int i = 1; i < iteration; i++)
            {
                byte[] u = uList[i];
                xor_result = MathHelper.XORBytes(xor_result, u);
            }

            return xor_result;
        }
    }
}

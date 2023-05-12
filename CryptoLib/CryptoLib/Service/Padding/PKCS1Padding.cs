using CryptoLib.Algorithm.Key;
using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Padding
{
    public class PKCS1Padding : IPaddingScheme
    {
        // https://www.rfc-editor.org/rfc/rfc3447
        private byte[] AddPadding(byte[] data, int randomBytesSize) {
            byte[] randomBytes = MathHelper.GetRandomBytesWithoutZero(randomBytesSize);
            List<byte> padded = new List<byte>() { 0x00, 0x02 };
            padded.AddRange(randomBytes);
            padded.Add(0x00);
            padded.AddRange(data);
            byte[] result = padded.ToArray();
            return result;
        }

        public byte[] Encode(byte[] data, IKey? key)
        {
            if (key is not RSAPublicKey)
            {
                throw new InvalidCastException();
            }

            // https://crypto.stackexchange.com/questions/66521/why-does-adding-pkcs1-v1-5-padding-make-rsa-encryption-non-deterministic
            RSAPublicKey publicKey = (RSAPublicKey)key;
            BigInteger n = publicKey.Modulus;

            int k = n.GetByteCount(true);
            if (data.Length > k - 11)
            {
                throw new ArgumentException("message too long");
            }

            int r = k - 3 - data.Length;

            if (r <= 0)
            {
                throw new ArgumentException();
            }

            byte[] padded = AddPadding(data, r);
            return padded;
        }

        public byte[] Decode(byte[] data, IKey? key = null)
        {
            List<byte> decryptedBytes = data.ToList();

            int pos = 0;
            for (int i = 0; i < decryptedBytes.Count; i++)
            {
                byte value = decryptedBytes[i];
                if (i == 0 && value != 0x00)
                {
                    throw new ArgumentException("error decoding, probably a corrupt key or wrong padding format");
                }

                if (value == 0x00 && i != 0)
                {
                    pos = i + 1;
                    break;
                }
            }

            byte[] message = decryptedBytes.GetRange(pos, decryptedBytes.Count - pos).ToArray();
            return message;
        }
    }
}

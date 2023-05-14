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

        public byte[] Encode(byte[] data, IDictionary<string, object>? param = null)
        {
            if (param == null)
            {
                throw new ArgumentNullException(nameof(param));
            }

            IKey key = (IKey)param["RSAKey"];

            if (key is not RSAPublicKey)
            {
                throw new InvalidCastException();
            }

            // https://crypto.stackexchange.com/questions/66521/why-does-adding-pkcs1-v1-5-padding-make-rsa-encryption-non-deterministic
            int k = key.GetKeySize() / 8;
            if (data.Length > k - 11)
            {
                throw new Exception("message too long");
            }

            int r = k - 3 - data.Length;

            if (r <= 0)
            {
                throw new Exception();
            }

            byte[] padded = AddPadding(data, r);
            return padded;
        }

        public byte[] Decode(byte[] data, IDictionary<string, object>? param = null)
        {
            if (param == null)
            {
                throw new ArgumentNullException(nameof(param));
            }

            IKey key = (IKey)param["RSAKey"];

            if (key is not RSAPrivateKey)
            {
                throw new InvalidCastException();
            }

            List<byte> decryptedBytes = data.ToList();
            if (decryptedBytes.Count == key.GetKeySize() && decryptedBytes[0] != 0x00)
            {
                throw new Exception("error decoding, probably a corrupt key or an invalid padding format");
            }

            if (decryptedBytes.Count < key.GetKeySize() && decryptedBytes[0] != 0x02)
            {
                throw new Exception("error decoding, probably a corrupt key or an invalid padding format");
            }

            int pos = 0;
            for (int i = 0; i < decryptedBytes.Count; i++)
            {
                byte value = decryptedBytes[i];
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

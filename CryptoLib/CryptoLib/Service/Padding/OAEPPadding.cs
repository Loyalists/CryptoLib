using CryptoLib.Algorithm.Key;
using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Padding
{
    public class OAEPPadding : IPaddingScheme
    {
        private byte[] AddPadding(HashAlgorithm hash, byte[] data, int length) {
            Encoding enc = Encoding.UTF8;
            byte[] lHash = hash.ComputeHash(enc.GetBytes(""));

            int mLen = data.Length;
            int hLen = hash.HashSize / 8;
            int psLen = length - mLen - 2 * hLen - 2;
            int maskLen = length - hLen - 1;
            byte[] ps = new byte[psLen];
            List<byte> DB = new List<byte>(length);
            DB.AddRange(lHash);
            DB.AddRange(ps);
            DB.Add(0x01);
            DB.AddRange(data);
            if (DB.Count != maskLen)
            {
                throw new Exception();
            }

            byte[] seed = MathHelper.GetRandomBytes(hLen);
            var maskGen = new PKCS1MaskGenerationMethod();
            byte[] dbMask = maskGen.GenerateMask(seed, maskLen);
            byte[] maskedDB = DB.ToArray().XORBytes(dbMask);
            byte[] seedMask = maskGen.GenerateMask(maskedDB, hLen);
            byte[] maskedSeed = seed.XORBytes(seedMask);

            List<byte> result = new List<byte>
            {
                0x00
            };
            result.AddRange(maskedSeed);
            result.AddRange(maskedDB);
            return result.ToArray();
        }

        private HashAlgorithm GetHashAlgorithm(IDictionary<string, object>? param)
        {
            string default_hash_name = "SHA1";
            string? hash_name = default_hash_name;
            if (param != null)
            {
                object? result;
                bool success = param.TryGetValue("HashAlgorithm", out result);
                if (success)
                {
                    hash_name = (string?)result;
                }
            }

            if (string.IsNullOrEmpty(hash_name))
            {
                hash_name = default_hash_name;
            }

            var hash = HashAlgorithm.Create(hash_name);
            if (hash == null)
            {
                throw new Exception();
            }
            return hash;
        }

        public byte[] Encode(byte[] data, IKey? key, IDictionary<string, object>? param = null)
        {
            if (key is not RSAPublicKey)
            {
                throw new InvalidCastException();
            }

            var hash = GetHashAlgorithm(param);

            int k = key.GetKeySize() / 8;
            int hLen = hash.HashSize / 8;
            if (data.Length > k - 2 * hLen - 2)
            {
                throw new Exception("message too long");
            }

            byte[] padded = AddPadding(hash, data, k);
            return padded;
        }

        public byte[] Decode(byte[] data, IKey? key = null, IDictionary<string, object>? param = null)
        {
            if (key is not RSAPrivateKey)
            {
                throw new InvalidCastException();
            }

            List<byte> decryptedBytes = data.ToList();
            if (decryptedBytes.Count == key.GetKeySize() && decryptedBytes[0] != 0x00)
            {
                throw new Exception("error decoding, probably a corrupt key or an invalid padding format");
            }

            Encoding enc = Encoding.UTF8;
            var hash = GetHashAlgorithm(param);

            int k = key.GetKeySize() / 8;
            byte[] lHash = hash.ComputeHash(enc.GetBytes(""));
            int hLen = hash.HashSize / 8;
            int maskLen = k - hLen - 1;

            byte[] maskedSeed = decryptedBytes.GetRange(0, hLen).ToArray();
            byte[] maskedDB = decryptedBytes.GetRange(hLen, k - hLen - 1).ToArray();

            var maskGen = new PKCS1MaskGenerationMethod();
            byte[] seedMask = maskGen.GenerateMask(maskedDB, hLen);
            byte[] seed = maskedSeed.XORBytes(seedMask);
            byte[] dbMask = maskGen.GenerateMask(seed, maskLen);
            byte[] DB = maskedDB.XORBytes(dbMask);

            int pos = 0;
            for (int i = lHash.Length; i < DB.Length; i++)
            {
                byte value = DB[i];
                if (value == 0x01)
                {
                    pos = i + 1;
                    break;
                }
            }

            if (pos == 0)
            {
                throw new Exception("error decoding, probably a corrupt key or an invalid padding format");
            }

            byte[] message = DB.ToList().GetRange(pos, DB.Length - pos).ToArray();
            return message;
        }
    }
}

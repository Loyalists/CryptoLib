using CryptoLib.Algorithm;
using CryptoLib.Algorithm.Key;
using CryptoLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service
{
    public class DESService : IKeyGenerator<DESKeyType>
    {
        public string? Passphrase { get; set; }
        public uint Iteration { get; set; } = 4096;
        public Dictionary<DESKeyType, IKey> Generate()
        {
            if (Passphrase == null)
            {
                throw new ArgumentNullException();
            }

            byte[] salt = MathHelper.GetRandomBytes(16);
            byte[] _key = KeyDerivation.PBKDF2(Encoding.UTF8.GetBytes(Passphrase), salt, Iteration, 7);
            BitArray ba_key = new BitArray(_key);
            List<BitArray> list = BitHelper.SplitBitsByCount(ba_key, 7);
            List<bool> bools = new List<bool>(64);
            foreach (BitArray ba in list)
            {
                ba.Length = 8;
                bool[] ba2 = BitHelper.ConvertBitsToBoolArray(ba);
                bools.AddRange(ba2);
            }

            byte[] bytes = BitHelper.ConvertBitsToBytes(new BitArray(bools.ToArray()));
            DESKey key = new DESKey(bytes, salt);
            var keys = new Dictionary<DESKeyType, IKey>
            {
                { DESKeyType.Key, key }
            };

            return keys;
        }
    }
}

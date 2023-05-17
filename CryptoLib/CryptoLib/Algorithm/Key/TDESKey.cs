using CryptoLib.Service.Format;
using CryptoLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Algorithm.Key
{
    public class TDESKey : IKey
    {
        public byte[] Bytes { get; set; }
        public byte[]? Salt { get; set; }
        public byte[]? IV { get; set; }

        public TDESKey(byte[] bytes)
        {
            Bytes = bytes;
        }

        public override string ToString()
        {
            string keyString = Convert.ToHexString(Bytes);
            return keyString;
        }

        public string ToString(IKeyFormat? keyFormat, bool isFormatted = true)
        {
            return ToString();
        }

        public byte[] ToByteArray(IKeyFormat? keyFormat = null)
        {
            return Bytes;
        }
        public List<DESKey> ToDESKeys()
        {
            List<byte> bytes = new List<byte>(Bytes);
            List<DESKey> keys = new List<DESKey>();
            for (int i = 0; i< 3; i++)
            {
                DESKey key = new DESKey(bytes.GetRange(i * 8, 8).ToArray());
                if (IV != null)
                {
                    key.IV = new byte[IV.Length];
                    IV.CopyTo(key.IV, 0);
                }
                keys.Add(key);
            }

            return keys;
        }

        public int GetKeySize()
        {
            return 168;
        }
    }
}

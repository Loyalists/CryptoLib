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
    public class DESKey : IKey
    {
        public byte[] Bytes { get; set; }
        public byte[] Salt { get; set; }

        public DESKey(byte[] bytes, byte[] salt)
        {
            Bytes = bytes;
            Salt = salt;
        }

        public override string ToString()
        {
            string keyString = Convert.ToHexString(Bytes);
            return keyString;
        }

        public string ToString(IKeyFormat? keyFormat = null, bool isFormatted = true)
        {
            return ToString();
        }

        public byte[] ToByteArray(IKeyFormat? keyFormat = null)
        {
            return Bytes;
        }
    }
}

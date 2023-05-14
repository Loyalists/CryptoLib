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
    public class PKCS5Padding : IPaddingScheme
    {
        public byte[] Encode(byte[] data, IDictionary<string, object>? param = null)
        {
            int blockSize = 8;
            int padLength = blockSize - data.Length % blockSize;
            List<byte> padded = new List<byte>(data);
            for (int i = 0; i < padLength; i++)
            {
                padded.Add((byte)padLength);
            }
            return padded.ToArray();
        }

        public byte[] Decode(byte[] data, IDictionary<string, object>? param = null)
        {
            List<byte> decrypted = data.ToList();
            int padLength = decrypted[decrypted.Count - 1];
            if (padLength < 0 || padLength > 8)
            {
                throw new InvalidOperationException();
            }

            int start = decrypted.Count - padLength;
            decrypted.RemoveRange(start, padLength);
            byte[] message = decrypted.ToArray();
            return message;
        }
    }
}

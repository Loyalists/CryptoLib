using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Utility
{
    public static class CryptoHelper
    {
        public static byte[] GenerateIV(int size)
        {
            byte[] iv = MathHelper.GetRandomBytes(size);
            return iv;
        }
    }
}

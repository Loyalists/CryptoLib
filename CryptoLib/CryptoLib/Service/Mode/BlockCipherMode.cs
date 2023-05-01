using CryptoLib.Service.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Mode
{
    public enum SupportedBlockCipherMode
    {
        ECB,
    }

    public static class BlockCipherMode
    {
        public static Dictionary<SupportedBlockCipherMode, Type> ModeMap = new Dictionary<SupportedBlockCipherMode, Type>()
        {
            { SupportedBlockCipherMode.ECB, typeof(ECBMode) },
        };

        public static IBlockCipherMode CreateInstance(SupportedBlockCipherMode padding)
        {
            Type type = ModeMap[padding];

            IBlockCipherMode? instance = (IBlockCipherMode?)Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException();
            }

            return instance;
        }
    }
}

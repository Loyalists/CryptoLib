using CryptoLib.Service.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Mode
{
    public enum BlockCipherMode
    {
        ECB,
        CBC,
        CFB,
        CTR,
    }

    public static class BlockCipherModeFactory
    {
        public static Dictionary<BlockCipherMode, Type> ModeMap = new Dictionary<BlockCipherMode, Type>()
        {
            { BlockCipherMode.ECB, typeof(ECBMode) },
            { BlockCipherMode.CBC, typeof(CBCMode) },
            { BlockCipherMode.CFB, typeof(CFBMode) },
            { BlockCipherMode.CTR, typeof(CTRMode) },
        };

        public static IBlockCipherMode CreateInstance(BlockCipherMode padding)
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

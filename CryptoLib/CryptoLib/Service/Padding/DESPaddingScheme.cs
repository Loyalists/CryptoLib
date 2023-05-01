using CryptoLib.Service.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Padding
{
    public enum SupportedDESPaddingScheme
    {
        PKCS5,
    }

    public static class DESPaddingScheme
    {
        public static Dictionary<SupportedDESPaddingScheme, Type> PaddingSchemeMap = new Dictionary<SupportedDESPaddingScheme, Type>()
        {
            { SupportedDESPaddingScheme.PKCS5, typeof(PKCS5Padding) },
        };

        public static IPaddingScheme CreateInstance(SupportedDESPaddingScheme padding)
        {
            Type? type = PaddingSchemeMap[padding];
            if (type == null)
            {
                return null;
            }

            IPaddingScheme? instance = (IPaddingScheme?)Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException();
            }

            return instance;
        }
    }
}

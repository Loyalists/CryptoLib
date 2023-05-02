using CryptoLib.Service.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Padding
{
    public enum DESPaddingScheme
    {
        None,
        PKCS5,
    }

    public static class DESPaddingSchemeFactory
    {
        public static Dictionary<DESPaddingScheme, Type?> PaddingSchemeMap = new Dictionary<DESPaddingScheme, Type?>()
        {
            { DESPaddingScheme.None, null },
            { DESPaddingScheme.PKCS5, typeof(PKCS5Padding) },
        };

        public static IPaddingScheme? CreateInstance(DESPaddingScheme padding)
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

using CryptoLib.Service.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Padding
{
    public enum RSAPaddingScheme
    {
        Textbook,
        PKCS1,
    }

    public static class RSAPaddingSchemeFactory
    {
        public static Dictionary<RSAPaddingScheme, Type?> PaddingSchemeMap = new Dictionary<RSAPaddingScheme, Type?>()
        {
            { RSAPaddingScheme.Textbook, null },
            { RSAPaddingScheme.PKCS1, typeof(PKCS1Padding) },
        };

        public static IPaddingScheme? CreateInstance(RSAPaddingScheme padding)
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

using CryptoLib.Service.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Padding
{
    public enum SupportedRSAPaddingScheme
    {
        Textbook,
        PKCS1,
    }

    public static class RSAPaddingScheme
    {
        public static Dictionary<SupportedRSAPaddingScheme, Type?> PaddingSchemeMap = new Dictionary<SupportedRSAPaddingScheme, Type?>()
        {
            { SupportedRSAPaddingScheme.Textbook, null },
            { SupportedRSAPaddingScheme.PKCS1, typeof(PKCS1Padding) },
        };

        public static IPaddingScheme? CreateInstance(SupportedRSAPaddingScheme padding)
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

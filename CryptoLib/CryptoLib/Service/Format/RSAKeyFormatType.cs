using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Format
{
    public enum SupportedRSAPublicKeyFormat
    {
        PKCS1,
        PKCS8,
    }

    public enum SupportedRSAPrivateKeyFormat
    {
        PKCS1,
    }

    public static class RSAKeyFormatType
    {
        public static Dictionary<SupportedRSAPublicKeyFormat, Type> PublicKeyFormatMap = new Dictionary<SupportedRSAPublicKeyFormat, Type>()
        {
            { SupportedRSAPublicKeyFormat.PKCS1, typeof(RSAPublicKeyPKCS1) },
            { SupportedRSAPublicKeyFormat.PKCS8, typeof(RSAPublicKeyPKCS8) },
        };

        public static Dictionary<SupportedRSAPrivateKeyFormat, Type> PrivateKeyFormatMap = new Dictionary<SupportedRSAPrivateKeyFormat, Type>()
        {
            { SupportedRSAPrivateKeyFormat.PKCS1, typeof(RSAPrivateKeyPKCS1) },
        };

        public static IKeyFormat CreatePublicKeyFormatInstance(SupportedRSAPublicKeyFormat publicKeyFormat)
        {
            Type type = PublicKeyFormatMap[publicKeyFormat];
            IKeyFormat? instance = (IKeyFormat?)Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException();
            }

            return instance;
        }

        public static IKeyFormat CreatePrivateKeyFormatInstance(SupportedRSAPrivateKeyFormat privateKeyFormat)
        {
            Type type = PrivateKeyFormatMap[privateKeyFormat];
            IKeyFormat? instance = (IKeyFormat?)Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException();
            }

            return instance;
        }
    }
}

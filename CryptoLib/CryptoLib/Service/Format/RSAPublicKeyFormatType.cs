using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Format
{
    public enum RSAPublicKeyFormat
    {
        PKCS1,
        PKCS8,
    }

    public static class RSAPublicKeyFormatType
    {
        public static Dictionary<RSAPublicKeyFormat, Type> PublicKeyFormatMap = new Dictionary<RSAPublicKeyFormat, Type>()
        {
            { RSAPublicKeyFormat.PKCS1, typeof(RSAPublicKeyPKCS1) },
            { RSAPublicKeyFormat.PKCS8, typeof(RSAPublicKeyPKCS8) },
        };

        public static IKeyFormat CreateInstance(RSAPublicKeyFormat publicKeyFormat)
        {
            Type type = PublicKeyFormatMap[publicKeyFormat];
            IKeyFormat? instance = (IKeyFormat?)Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException();
            }

            return instance;
        }
    }
}

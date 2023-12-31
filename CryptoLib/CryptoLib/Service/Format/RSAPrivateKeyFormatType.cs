﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Format
{
    public enum RSAPrivateKeyFormat
    {
        PKCS1,
        PKCS8,
    }

    public static class RSAPrivateKeyFormatType
    {
        public static Dictionary<RSAPrivateKeyFormat, Type> PrivateKeyFormatMap = new Dictionary<RSAPrivateKeyFormat, Type>()
        {
            { RSAPrivateKeyFormat.PKCS1, typeof(RSAPrivateKeyPKCS1) },
            { RSAPrivateKeyFormat.PKCS8, typeof(RSAPrivateKeyPKCS8) },
        };

        public static IKeyFormat CreateInstance(RSAPrivateKeyFormat privateKeyFormat)
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

using CryptoLib.Algorithm.Key;
using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Format
{
    public class RSAPublicKeyPKCS1 : IKeyFormat
    {
        // PKCS #8
        public static readonly string Header = "-----BEGIN RSA PUBLIC KEY-----";
        public static readonly string Footer = "-----END RSA PUBLIC KEY-----";
        public byte[] ToByteArray(IKey key)
        {
            if (key is not RSAPublicKey)
            {
                throw new InvalidCastException();
            }

            RSAPublicKey publicKey = (RSAPublicKey)key;
            var writer = new AsnWriter(AsnEncodingRules.DER);

            using (writer.PushSequence())
            {
                writer.WriteInteger(publicKey.Modulus);
                writer.WriteInteger(publicKey.PublicExponent);
            }

            byte[] value = writer.Encode();
            return value;
        }

        public static string Format(string encoded)
        {
            int size = 64;
            var splitted = Helper.SplitString(encoded, size);

            StringBuilder sb = new StringBuilder();
            sb.Append(Header);
            sb.Append(Environment.NewLine);
            for (int i = 0; i < splitted.Count(); i++)
            {
                string s = splitted[i];
                sb.Append(s);
                if (i < splitted.Count() - 1)
                {
                    sb.Append(Environment.NewLine);
                }
            }
            sb.Append(Environment.NewLine);
            sb.Append(Footer);

            return sb.ToString();
        }

        public string ToString(IKey key, bool isFormatted = true)
        {
            var value = ToByteArray(key);
            string encoded = Convert.ToBase64String(value);
            string result = encoded;
            if (isFormatted)
            {
                result = Format(encoded);
            }

            return result;
        }

        public IKey FromByteArray(byte[] bytes)
        {
            try
            {
                AsnReader reader = new AsnReader(bytes, AsnEncodingRules.DER);
                AsnReader contents = reader.ReadSequence();
                BigInteger modulus = contents.ReadInteger();
                BigInteger publicExponent = contents.ReadInteger();

                RSAPublicKey publicKey = new RSAPublicKey();
                publicKey.Modulus = modulus;
                publicKey.PublicExponent = publicExponent;
                return publicKey;
            }
            catch (Exception)
            {
                throw new Exception("error reading key");
            }
        }

        public IKey FromString(string str)
        {
            string headerRemoved = str.Replace(Header, "").Replace(Footer, "");
            string breaksRemoved = headerRemoved.Replace(Environment.NewLine, "");
            byte[] bytes = Convert.FromBase64String(breaksRemoved);
            IKey key = FromByteArray(bytes);
            return key;
        }
    }
}

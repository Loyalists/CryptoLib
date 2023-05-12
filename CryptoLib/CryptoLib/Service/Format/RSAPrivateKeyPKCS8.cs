using CryptoLib.Algorithm.Key;
using CryptoLib.Utility;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Service.Format
{
    public class RSAPrivateKeyPKCS8 : IKeyFormat
    {
        public static readonly string Header = "-----BEGIN PRIVATE KEY-----";
        public static readonly string Footer = "-----END PRIVATE KEY-----";
        public static readonly string Oid = "1.2.840.113549.1.1.1";
        public byte[] ToByteArray(IKey key)
        {
            if (key is not RSAPrivateKey)
            {
                throw new InvalidCastException();
            }

            RSAPrivateKey privateKey = (RSAPrivateKey)key;
            var writer = new AsnWriter(AsnEncodingRules.DER);

            using (writer.PushSequence())
            {
                writer.WriteInteger(privateKey.Version);
                using (writer.PushSequence())
                {
                    writer.WriteObjectIdentifier(Oid);
                    writer.WriteNull();
                }

                var writerInner = new AsnWriter(AsnEncodingRules.DER);
                using (writerInner.PushSequence())
                {
                    writerInner.WriteInteger(privateKey.Version);
                    writerInner.WriteInteger(privateKey.Modulus);
                    writerInner.WriteInteger(privateKey.PublicExponent);
                    writerInner.WriteInteger(privateKey.PrivateExponent);
                    writerInner.WriteInteger(privateKey.Prime1);
                    writerInner.WriteInteger(privateKey.Prime2);
                    writerInner.WriteInteger(privateKey.Exponent1);
                    writerInner.WriteInteger(privateKey.Exponent2);
                    writerInner.WriteInteger(privateKey.Coefficient);

                }
                byte[] valueInner = writerInner.Encode();
                writer.WriteOctetString(valueInner);
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
                if (!contents.TryReadInt32(out int _version) || _version != 0)
                {
                    throw new Exception();
                }

                contents.ReadSequence();
                byte[] octetString = contents.ReadOctetString();

                AsnReader osReader = new AsnReader(octetString, AsnEncodingRules.DER);
                AsnReader osContents = osReader.ReadSequence();

                BigInteger version = osContents.ReadInteger();
                BigInteger modulus = osContents.ReadInteger();
                BigInteger publicExponent = osContents.ReadInteger();
                BigInteger privateExponent = osContents.ReadInteger();
                BigInteger prime1 = osContents.ReadInteger();
                BigInteger prime2 = osContents.ReadInteger();
                BigInteger exponent1 = osContents.ReadInteger();
                BigInteger exponent2 = osContents.ReadInteger();
                BigInteger coefficient = osContents.ReadInteger();

                RSAPrivateKey privateKey = new RSAPrivateKey();
                privateKey.Version = version;
                privateKey.Modulus = modulus;
                privateKey.PublicExponent = publicExponent;
                privateKey.PrivateExponent = privateExponent;
                privateKey.Prime1 = prime1;
                privateKey.Prime2 = prime2;
                privateKey.Exponent1 = exponent1;
                privateKey.Exponent2 = exponent2;
                privateKey.Coefficient = coefficient;

                return privateKey;
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

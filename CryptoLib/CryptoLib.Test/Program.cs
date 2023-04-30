using System;
using System.Buffers.Text;
using System.Collections;
using System.Formats.Asn1;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using CryptoLib.Algorithm;
using CryptoLib.Algorithm.Key;
using CryptoLib.Service;
using CryptoLib.Service.Format;
using CryptoLib.Service.Padding;
using CryptoLib.Utility;
using CryptoLib.Service.Mode;

namespace CryptoLib.Test
{
    internal class Program
    {
        static void TestRSA()
        {
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} started");
            RSAService service = new RSAService();
            service.KeySize = 1024;
            service.Padding = new PKCS1Padding();
            var publicKeyFormat = new RSAPublicKeyPKCS8();
            var privateKeyFormat = new RSAPrivateKeyPKCS1();

            var sw = Stopwatch.StartNew();
            var keys = service.Generate();
            double time_key = sw.Elapsed.TotalSeconds;


            var publicKey = (RSAPublicKey)keys[RSAKeyType.PublicKey];
            var privateKey = (RSAPrivateKey)keys[RSAKeyType.PrivateKey];
            Console.WriteLine("publicKey:");
            Console.WriteLine(publicKey.ToString(publicKeyFormat));
            Console.WriteLine("privateKey:");
            Console.WriteLine(privateKey.ToString(privateKeyFormat));

            Console.WriteLine($"key size:{publicKey.GetKeySize()}");
            Console.WriteLine($"n:{publicKey.Modulus}");
            Console.WriteLine($"e:{publicKey.PublicExponent}");
            Console.WriteLine($"d:{privateKey.PrivateExponent}");
            Console.WriteLine($"p:{privateKey.Prime1}");
            Console.WriteLine($"q:{privateKey.Prime2}");
            Console.WriteLine();

            string plainText = "The quick brown fox jumps over the lazy dog";
            Console.WriteLine("plainText:");
            Console.WriteLine(plainText);

            sw.Restart();
            string encrypted = service.Encrypt(plainText, publicKey);
            double time_enc = sw.Elapsed.TotalSeconds;
            Console.WriteLine("encrypted:");
            Console.WriteLine(encrypted);

            sw.Restart();
            string decoded = service.Decrypt(encrypted, privateKey);
            double time_dec = sw.Elapsed.TotalSeconds;
            Console.WriteLine("decoded:");
            Console.WriteLine(decoded);

            if (decoded == plainText)
            {
                Console.WriteLine("RSA implementation is valid.");
            }
            else
            {
                Console.WriteLine("RSA implementation is NOT valid.");
            }
            Console.WriteLine();
            Console.WriteLine($"time elapsed for generating RSA key pair:{time_key}");
            Console.WriteLine($"time elapsed for encryption:{time_enc}");
            Console.WriteLine($"time elapsed for decryption:{time_dec}");
            sw.Stop();
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} ended");
        }

        static void TestDES()
        {
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} started");
            string passphrase = "lol";
            string message = "The quick brown fox jumps over the lazy dog";
            DESService service = new DESService();
            service.Passphrase = passphrase;
            service.Padding = new PKCS5Padding();
            service.BlockCipherMode = new ECBMode(service);
            var keys = service.Generate();
            DESKey key = (DESKey)keys[DESKeyType.Key];
            Console.WriteLine("key:");
            Console.WriteLine(key);
            Console.WriteLine("message:");
            Console.WriteLine(message);

            var encrypted = service.Encrypt(message, key);
            Console.WriteLine("encrypted:");
            Console.WriteLine(encrypted);
            Console.WriteLine(Convert.ToHexString(Convert.FromBase64String(encrypted)));

            var decrypted = service.Decrypt(encrypted, key);
            Console.WriteLine("decrypted:");
            Console.WriteLine(decrypted);

            if (decrypted == message)
            {
                Console.WriteLine("DES implementation is valid.");
            }
            else
            {
                Console.WriteLine("DES implementation is NOT valid.");
            }
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} ended");
        }

        static void Main(string[] args)
        {
            //TestRSA();
            TestDES();
        }
    }
}
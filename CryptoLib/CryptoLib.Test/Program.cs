using System;
using System.Buffers.Text;
using System.Collections;
using System.Formats.Asn1;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using CryptoLib.Algorithm;
using CryptoLib.Algorithm.Key;
using CryptoLib.Service;
using CryptoLib.Service.Format;
using CryptoLib.Service.Padding;
using CryptoLib.Utility;
using CryptoLib.Service.Mode;
using DES = System.Security.Cryptography.DES;

namespace CryptoLib.Test
{
    internal class Program
    {
        static void TestRSA()
        {
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} started");
            RSAService service = new RSAService();
            service.KeySize = 1024;
            service.Padding = RSAPaddingScheme.OAEP;
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

            Console.WriteLine($"padding:{Enum.GetName(typeof(RSAPaddingScheme), service.Padding)}");

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
            Console.WriteLine($"time elapsed for generating a RSA key pair:{time_key}");
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
            //service.Padding = DESPaddingScheme.None;
            service.CipherMode = BlockCipherMode.ECB;
            Console.WriteLine($"padding:{Enum.GetName(typeof(DESPaddingScheme), service.Padding)}");
            Console.WriteLine($"mode:{Enum.GetName(typeof(BlockCipherMode), service.CipherMode)}");

            var keys = service.Generate();
            DESKey key = (DESKey)keys[DESKeyType.Key];
            Console.WriteLine($"key:{key}");
            Console.WriteLine($"salt:{Convert.ToHexString(key.Salt)}");
            Console.WriteLine($"IV:{Convert.ToHexString(key.IV)}");
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
            Console.WriteLine();

            DES des = DES.Create();
            des.Padding = PaddingMode.PKCS7;
            des.Mode = CipherMode.ECB;
            ICryptoTransform encryptor = des.CreateEncryptor(key.Bytes, key.IV);
            ICryptoTransform decryptor = des.CreateDecryptor(key.Bytes, key.IV);

            byte[] encrypted_bytes_test;
            string encrypted_test;
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(message);
                    }
                    encrypted_bytes_test = msEncrypt.ToArray();
                    encrypted_test = Convert.ToHexString(encrypted_bytes_test);
                }
            }
            Console.WriteLine("encrypted_test:");
            Console.WriteLine(encrypted_test);

            string decrypted_test;
            using (MemoryStream msDecrypt = new MemoryStream(encrypted_bytes_test))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        decrypted_test = srDecrypt.ReadToEnd();
                    }
                }
            }
            Console.WriteLine("decrypted_test:");
            Console.WriteLine(decrypted_test);

            if (decrypted == decrypted_test)
            {
                Console.WriteLine($"The result is identical to what is produced by the .NET implementation.");
            }
            else
            {
                Console.WriteLine($"The result is NOT identical to what is produced by the .NET implementation.");
            }

            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} ended");
        }

        static void Test3DES()
        {
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} started");
            string passphrase = "lol";
            string message = "The quick brown fox jumps over the lazy dog";
            TDESService service = new TDESService();
            service.Passphrase = passphrase;
            service.Padding = DESPaddingScheme.PKCS5;
            service.CipherMode = BlockCipherMode.ECB;
            Console.WriteLine($"padding:{Enum.GetName(typeof(DESPaddingScheme), service.Padding)}");
            Console.WriteLine($"mode:{Enum.GetName(typeof(BlockCipherMode), service.CipherMode)}");

            var keys = service.Generate();
            TDESKey key = (TDESKey)keys[DESKeyType.Key];
            Console.WriteLine($"key:{key}");
            Console.WriteLine($"salt:{Convert.ToHexString(key.Salt)}");
            Console.WriteLine($"IV:{Convert.ToHexString(key.IV)}");
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
                Console.WriteLine("3DES implementation is valid.");
            }
            else
            {
                Console.WriteLine("3DES implementation is NOT valid.");
            }

            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} ended");
        }

        static void TestRSAGenerateKey()
        {
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} started");
            int count = 10;
            int keySize = 1024;
            double totalTime = 0;
            Console.WriteLine($"key size:{keySize}");
            Console.WriteLine($"count:{count}");
            var tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                var task = Task.Run(() =>
                {
                    RSAService service = new RSAService();
                    service.KeySize = keySize;
                    var sw = Stopwatch.StartNew();
                    service.Generate();
                    sw.Stop();
                    double time = sw.Elapsed.TotalSeconds;
                    totalTime += time;
                    Console.WriteLine($"time elapsed for generating a RSA key pair:{time}");
                });
                tasks.Add(task);
            }
            Task t = Task.WhenAll(tasks);
            t.Wait();
            Console.WriteLine($"average:{totalTime / count} secs");

            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} ended");
        }

        static void TestRSAEncryptAndDecrypt()
        {
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} started");
            int count = 10;
            int keySize = 2048;
            double totalTime = 0;
            string plainText = "Lorem ipsum dolor sit amet";
            string encrypted = "";
            string decrypted = "";
            RSAService service = new RSAService();
            service.KeySize = keySize;
            var keys = service.Generate();
            var publicKey = (RSAPublicKey)keys[RSAKeyType.PublicKey];
            var privateKey = (RSAPrivateKey)keys[RSAKeyType.PrivateKey];

            Console.WriteLine($"key size:{keySize}");
            Console.WriteLine($"count:{count}");
            Console.WriteLine($"plainText:{plainText}");
            var tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                var task = Task.Run(() =>
                {
                    var sw = Stopwatch.StartNew();
                    encrypted = service.Encrypt(plainText, publicKey);
                    sw.Stop();
                    double time = sw.Elapsed.TotalSeconds;
                    totalTime += time;
                    Console.WriteLine($"time elapsed for encryption:{time}");
                });
                tasks.Add(task);
            }
            Task t = Task.WhenAll(tasks);
            t.Wait();
            tasks.Clear();
            Console.WriteLine($"average:{totalTime / count} secs");
            Console.WriteLine($"encrypted:{encrypted}");
            Console.WriteLine();

            totalTime = 0;
            for (int i = 0; i < count; i++)
            {
                var task = Task.Run(() =>
                {
                    var sw = Stopwatch.StartNew();
                    decrypted = service.Decrypt(encrypted, privateKey);
                    sw.Stop();
                    double time = sw.Elapsed.TotalSeconds;
                    totalTime += time;
                    Console.WriteLine($"time elapsed for decryption:{time}");
                });
                tasks.Add(task);
            }
            Task t2 = Task.WhenAll(tasks);
            t2.Wait();
            Console.WriteLine($"average:{totalTime / count} secs");
            Console.WriteLine($"decrypted:{decrypted}");
            Console.WriteLine($"{MethodBase.GetCurrentMethod().Name} ended");
        }

        static void Main(string[] args)
        {
            //TestRSA();
            //TestRSAGenerateKey();
            //TestRSAEncryptAndDecrypt();
            //TestDES();
            Test3DES();
        }
    }
}
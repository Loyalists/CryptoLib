﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CryptoLib.Algorithm;
using CryptoLib.Algorithm.Key;
using CryptoLib.Service.Padding;
using CryptoLib.Utility;

namespace CryptoLib.Service
{
    public class RSAService : IKeyGenerator<RSAKeyType>, ICryptoService
    {
        public int KeySize { get; set; } = 1024;
        public RSAPaddingScheme Padding { get; set; } = RSAPaddingScheme.Textbook;
        public int GenerateThreadCount = 4;

        public async Task<Dictionary<RSAKeyType, IKey>> GenerateAsync()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var tasks = new List<Task<(BigInteger p, BigInteger q)>>();

            int numSize = KeySize / 2;
            BigInteger p = BigInteger.One;
            BigInteger q = BigInteger.One;
            try
            {
                for (int i = 0; i < GenerateThreadCount; i++)
                {
                    var task = Task.Run(() =>
                    {
                        (BigInteger first, BigInteger second) = GenerateAsyncCore(numSize, token);
                        return (first, second);
                    }, token);
                    tasks.Add(task);
                }
                var result = await Task.WhenAny(tasks);
                tokenSource.Cancel();
                p = result.Result.p;
                q = result.Result.q;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                tokenSource.Dispose();
            }

            BigInteger n = Algorithm.RSA.GetModulus(p, q);
            BigInteger lambda = Algorithm.RSA.GetLambda(p, q);
            BigInteger e = Algorithm.RSA.GetPublicKeyExponent(lambda);
            BigInteger d = Algorithm.RSA.GetPrivateKeyExponent(e, lambda);

            var publicKey = new RSAPublicKey();
            publicKey.Modulus = n;
            publicKey.PublicExponent = e;

            var privateKey = new RSAPrivateKey();
            privateKey.Modulus = n;
            privateKey.PublicExponent = e;
            privateKey.PrivateExponent = d;
            privateKey.Prime1 = p;
            privateKey.Prime2 = q;
            privateKey.Exponent1 = Algorithm.RSA.GetExponent(d, p);
            privateKey.Exponent2 = Algorithm.RSA.GetExponent(d, q);
            privateKey.Coefficient = Algorithm.RSA.GetCoefficient(q, p);

            var keys = new Dictionary<RSAKeyType, IKey>
            {
                { RSAKeyType.PublicKey, publicKey },
                { RSAKeyType.PrivateKey, privateKey }
            };

            return keys;
        }

        public Dictionary<RSAKeyType, IKey> Generate()
        {
            int numSize = KeySize / 2;
            BigInteger p = BigInteger.One;
            BigInteger q = BigInteger.One;

            (p, q) = GenerateCore(numSize);

            BigInteger n = Algorithm.RSA.GetModulus(p, q);
            BigInteger lambda = Algorithm.RSA.GetLambda(p, q);
            BigInteger e = Algorithm.RSA.GetPublicKeyExponent(lambda);
            BigInteger d = Algorithm.RSA.GetPrivateKeyExponent(e, lambda);

            var publicKey = new RSAPublicKey();
            publicKey.Modulus = n;
            publicKey.PublicExponent = e;

            var privateKey = new RSAPrivateKey();
            privateKey.Modulus = n;
            privateKey.PublicExponent = e;
            privateKey.PrivateExponent = d;
            privateKey.Prime1 = p;
            privateKey.Prime2 = q;
            privateKey.Exponent1 = Algorithm.RSA.GetExponent(d, p);
            privateKey.Exponent2 = Algorithm.RSA.GetExponent(d, q);
            privateKey.Coefficient = Algorithm.RSA.GetCoefficient(q, p);

            var keys = new Dictionary<RSAKeyType, IKey>
            {
                { RSAKeyType.PublicKey, publicKey },
                { RSAKeyType.PrivateKey, privateKey }
            };

            return keys;
        }

        private (BigInteger p, BigInteger q) GenerateAsyncCore(int numSize, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }

            BigInteger p = BigInteger.One;
            BigInteger q = BigInteger.One;
            while (true)
            {
                var tasks = new List<Task>
                {
                    Task.Run(() =>
                    {
                        p = MathHelper.GetRandomPrime(numSize);
                    }),
                    Task.Run(() =>
                    {
                        q = MathHelper.GetRandomPrime(numSize);
                    })
                };
                Task t = Task.WhenAll(tasks);
                try
                {
                    t.Wait();
                }
                catch { }

                BigInteger _n = Algorithm.RSA.GetModulus(p, q);
                if (_n.GetBitLength() == KeySize)
                {
                    break;
                }

                if (ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                }
            }

            return (p, q);
        }

        private (BigInteger p, BigInteger q) GenerateCore(int numSize)
        {
            BigInteger p = BigInteger.One;
            BigInteger q = BigInteger.One;
            while (true)
            {
                var tasks = new List<Task>
                {
                    Task.Run(() =>
                    {
                        p = MathHelper.GetRandomPrime(numSize);
                    }),
                    Task.Run(() =>
                    {
                        q = MathHelper.GetRandomPrime(numSize);
                    })
                };
                Task t = Task.WhenAll(tasks);
                try
                {
                    t.Wait();
                }
                catch { }

                BigInteger _n = Algorithm.RSA.GetModulus(p, q);
                if (_n.GetBitLength() == KeySize)
                {
                    break;
                }
            }

            return (p, q);
        }

        public byte[] Encrypt(byte[] data, IKey key, IDictionary<string, object>? param = null)
        {
            if (key is not RSAPublicKey)
            {
                throw new InvalidCastException();
            }

            IPaddingScheme? padding = RSAPaddingSchemeFactory.CreateInstance(Padding);
            RSAPublicKey publicKey = (RSAPublicKey)key;
            BigInteger n = publicKey.Modulus;
            BigInteger e = publicKey.PublicExponent;

            if (param == null)
            {
                param = new Dictionary<string, object>();
            }
            param["RSAKey"] = publicKey;

            // OS2IP is in unsigned big endian
            byte[] padded = data;
            if (padding != null)
            {
                padded = padding.Encode(data, param);
            }

            BigInteger encoded = new BigInteger(padded, isUnsigned: true, isBigEndian: true);
            if (encoded > n)
            {
                throw new ArgumentException("message too long");
            }

            BigInteger encrypted = RSA.Encrypt(encoded, e, n);
            byte[] encryptedBytes = encrypted.ToByteArray(isUnsigned: true, isBigEndian: true);
            return encryptedBytes;
        }

        public string Encrypt(string data, IKey key, IDictionary<string, object>? param = null)
        {
            byte[] message = Encoding.UTF8.GetBytes(data);
            byte[] encrypted = Encrypt(message, key, param);
            string encoded = Convert.ToBase64String(encrypted);
            return encoded;
        }

        public byte[] Decrypt(byte[] data, IKey key, IDictionary<string, object>? param = null)
        {
            if (key is not RSAPrivateKey)
            {
                throw new InvalidCastException();
            }

            IPaddingScheme? padding = RSAPaddingSchemeFactory.CreateInstance(Padding);
            RSAPrivateKey privateKey = (RSAPrivateKey)key;
            BigInteger n = privateKey.Modulus;
            BigInteger d = privateKey.PrivateExponent;
            BigInteger encrypted = new BigInteger(data, isUnsigned: true, isBigEndian: true);
            if (encrypted > n)
            {
                throw new ArgumentException("message too long");
            }

            if (param == null)
            {
                param = new Dictionary<string, object>();
            }
            param["RSAKey"] = privateKey;

            BigInteger decrypted = RSA.Decrypt(encrypted, d, n);
            byte[] decryptedBytes = decrypted.ToByteArray(isUnsigned: true, isBigEndian: true);
            byte[] decoded = decryptedBytes;
            if (padding != null)
            {
                decoded = padding.Decode(decryptedBytes, param);
            }

            return decoded;
        }

        public string Decrypt(string data, IKey key, IDictionary<string, object>? param = null)
        {
            byte[] padded = Convert.FromBase64String(data);
            byte[] decrypted = Decrypt(padded, key, param);
            string decoded = Encoding.UTF8.GetString(decrypted);
            return decoded;
        }
    }
}

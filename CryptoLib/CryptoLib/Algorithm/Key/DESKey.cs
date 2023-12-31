﻿using CryptoLib.Service.Format;
using CryptoLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Algorithm.Key
{
    public class DESKey : IKey
    {
        public byte[] Bytes { get; set; }
        public byte[]? Salt { get; set; }
        public byte[]? IV { get; set; }

        public DESKey(byte[] bytes)
        {
            Bytes = bytes;
        }

        public override string ToString()
        {
            string keyString = Convert.ToHexString(Bytes);
            return keyString;
        }

        public string ToString(IKeyFormat? keyFormat, bool isFormatted = true)
        {
            return ToString();
        }

        public byte[] ToByteArray(IKeyFormat? keyFormat = null)
        {
            return Bytes;
        }

        public int GetKeySize()
        {
            return 56;
        }
    }
}

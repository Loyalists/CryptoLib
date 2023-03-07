using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.Utility
{
    public static class Helper
    {
        public static void LeftRotateArray<T>(T[] arr, int shift)
        {
            shift = shift % arr.Length;
            T[] buffer = new T[shift];
            Array.Copy(arr, buffer, shift);
            Array.Copy(arr, shift, arr, 0, arr.Length - shift);
            Array.Copy(buffer, 0, arr, arr.Length - shift, shift);
        }
        public static void SplitByIndex<T>(T[] array, int index, out T[] first, out T[] second)
        {
            first = array.Take(index).ToArray();
            second = array.Skip(index).ToArray();
        }

        public static void SplitMidPoint<T>(T[] array, out T[] first, out T[] second)
        {
            SplitByIndex(array, array.Length / 2, out first, out second);
        }

        public static List<T[]> SplitByCount<T>(this T[] source, int chunkMaxSize)
        {
            var chunks = source.Length / chunkMaxSize;
            var leftOver = source.Length % chunkMaxSize;
            var result = new List<T[]>(chunks + 1);
            var offset = 0;

            for (var i = 0; i < chunks; i++)
            {
                result.Add(new ArraySegment<T>(source,
                                               offset,
                                               chunkMaxSize).ToArray());
                offset += chunkMaxSize;
            }

            if (leftOver > 0)
            {
                result.Add(new ArraySegment<T>(source,
                                               offset,
                                               leftOver).ToArray());
            }

            return result;
        }

        public static List<string> SplitString(string str, int maxChunkSize)
        {
            // https://stackoverflow.com/questions/1450774/splitting-a-string-into-chunks-of-a-certain-size
            if (string.IsNullOrEmpty(str)) throw new ArgumentException();
            if (maxChunkSize < 1) throw new ArgumentException();

            List<string> result = new List<string>();
            for (int i = 0; i < str.Length; i += maxChunkSize)
            {
                string section = str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
                result.Add(section);
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WCSCompress.Core;
using WCSCompress.Core.CPCompressor.Helpers;

namespace WCSCompress
{
    public class TrieTester
    {
        public static byte[][] GenerateTestStrings(int count, int minContext, int maxContext, int sizeAlhpabet)
        {
            byte[][] result = new byte[count][];

            Random rnd = new Random(0);

            for (int i = 0; i < result.Length; i++)
            {
                int len = rnd.Next(minContext, maxContext);
                result[i] = new byte[len];

                for (int k = 0; k < len; k++)
                {
                    result[i][k] = (byte)rnd.Next(sizeAlhpabet);
                }
            }

            return result;
        }

        public static void AddToDictionary(Dictionary<byte[], BinaryCounterLight> dic, byte[][] testData)
        {
            for (int i = 0; i < testData.Length; i++)
            {
                BinaryCounterLight bcl;
                if (!(dic.TryGetValue(testData[i], out bcl)))
                {
                    byte[] tmp = new byte[testData[i].Length];
                    Buffer.BlockCopy(testData[i], 0, tmp, 0, testData[i].Length);
                    dic.Add(testData[i], new BinaryCounterLight());
                }
            }
        }

        public static int SearchInDictionary(Dictionary<byte[], BinaryCounterLight> dic, byte[][] testData)
        {
            int result = 0;
            for (int c = 0; c < 3; c++)
                for (int i = 0; i < testData.Length; i++)
            {
                BinaryCounterLight bcl;
                //dic.TryGetValue(testData[i], out bcl);
                //dic.TryGetValue(testData[i], out bcl);
                if (dic.TryGetValue(testData[i], out bcl))
                {
                    result++;
                }

            }

            return result;
        }


        public static void AddTo_Trie(Trie<BinaryCounterLight> dic, byte[][] testData)
        {
            for (int i = 0; i < testData.Length; i++)
            {
                BinaryCounterLight tmp;
                if (!dic.TryGetValue(testData[i], out tmp))
                {
                    dic.Add(testData[i], new BinaryCounterLight());
                }
            }
        }

        public static int SearchInTrie(Trie<BinaryCounterLight> dic, byte[][] testData)
        {
            int result = 0;
            for (int c = 0; c < 3; c++)
                for (int i = 0; i < testData.Length; i++)
            {
                BinaryCounterLight bcl;
                if (dic.TryGetValue(testData[i], out bcl))
                {
                    result++;
                }

            }

            return result;
        }

        public static void AddTo_TrieStruct(TrieStruct<BinaryCounterLight> dic, byte[][] testData)
        {
            for (int i = 0; i < testData.Length; i++)
            {
               
                BinaryCounterLight tmp;
                if (!dic.TryGetValue(testData[i], out tmp))
                {
                    dic.Add(testData[i], new BinaryCounterLight());
                }
                
            }

        }

        public static int SearchInTrieStruct(TrieStruct<BinaryCounterLight> dic, byte[][] testData)
        {
            int result = 0;
            for (int c = 0; c < 3 ; c++) 
            for (int i = 0; i < testData.Length; i++)
            {
                BinaryCounterLight bcl;
                if (dic.TryGetValue(testData[i], out bcl))
                {
                    result++;
                }
            }

            return result;
        }

        public static void AddTo_TrieStructFast(TrieStructFast<BinaryCounterLight> dic, byte[][] testData)
        {
            for (int i = 0; i < testData.Length; i++)
            {

                BinaryCounterLight tmp;
                if (!dic.TryGetValue(testData[i], out tmp))
                {
                    dic.Add(testData[i], new BinaryCounterLight());
                }

            }

        }

        public static int SearchInTrieStructFast(TrieStructFast<BinaryCounterLight> dic, byte[][] testData)
        {
            int result = 0;
            for (int c = 0; c < 3; c++)
                for (int i = 0; i < testData.Length; i++)
                {
                    BinaryCounterLight bcl;
                    if (dic.TryGetValue(testData[i], out bcl))
                    {
                        result++;
                    }
                }

            return result;
        }



    }
}

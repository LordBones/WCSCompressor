using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.Basic;
using WCSCompress.Core.CSPCompressor;

namespace SPCCompressLib
{
    public class SPCCompress
    {
        private ArraySegmentComparer _acpComparer = new ArraySegmentComparer();

        private StringBuilder _sb = new StringBuilder();

        public void EncodeStream(Stream input, Stream output)
        {
            BufferedStream bfInput = new BufferedStream(input,4096);
            BufferedStream bfOutput = new BufferedStream(output, 4096);


            int blockSize = 64024;// 1024;
            byte[] block = new byte[blockSize];
            int countRead = 0;
            do
            {
                countRead = bfInput.Read(block, 0, block.Length);
                if (countRead != blockSize)
                {
                    byte[] endBlock = new byte[countRead];
                    Buffer.BlockCopy(block, 0, endBlock, 0, countRead);
                    block = endBlock;
                }

                if (countRead > 0)
                {
                    byte[] encodedBlock = EncodeBlock(block,0);
                    bfOutput.Write(encodedBlock, 0, encodedBlock.Length);
                }
            }
            while (countRead == blockSize);

            bfOutput.Flush();
        }


        public byte [] EncodeBlock(byte [] data, int recursiveLevel)
        {
            if (data.Length == 0) return data;

            BlockAnalyzer ba = new BlockAnalyzer();
            

            AnalyzeResult analyzeBlock = ba.Analyze(data);
            _sb.Clear();

            DebugAnalyzeBlock(analyzeBlock);

            //StatByte choosedByte = analyzeBlock.GetMaxMatchNZDistance();
            //StatByte choosedByte = analyzeBlock.GetMinMaxDistance();
            StatByte choosedByte = analyzeBlock.GetMostCount();
            //StatByte choosedByte = analyzeBlock.GetMaxMatchDistance();
            //StatByte choosedByte = analyzeBlock.GetMinStdDevDistance();


            byte[] encodedData =  //Encode
                EncodeAdvance
                

                (data, //analyzeBlock.GetMinMaxDistance().Byte
               // analyzeBlock.GetMaxMatchedWords().Byte
                
                choosedByte.Byte
                // analyzeBlock.GetMostCount().Byte
                , //((int)choosedByte.AvgDistance + choosedByte.DistanceMax)/2
               //(choosedByte.DistanceMax - (int)(choosedByte.AvgDistance+choosedByte.AvgStdDiffDistance))
                 //((choosedByte.DistanceMax - ((int)choosedByte.AvgDistance))/4 + ((int)choosedByte.AvgDistance))
                 choosedByte.DistanceMax
                 , choosedByte.CountByte
                 , recursiveLevel
                );

            if (_sb.Length > 0)
                Console.Write(_sb.ToString());

            return encodedData;
            //StatByte mostCount = analyzeBlock.GetMostCount();
            //StatByte mostCount = analyzeBlock.GetMinDistance();
            //StatByte mostCount = analyzeBlock.GetMinMaxDistance();

        }

        

        private void DebugAnalyzeBlock(AnalyzeResult ar)
        {
            StatByte stb;
            stb = ar.GetMostCount();
            DebugAnalyzeBlockWrite(stb, "most Count ");
            stb = ar.GetMinAvgDistance();
            DebugAnalyzeBlockWrite(stb, "minAvgDist ");
            stb = ar.GetMinMaxDistance();
            DebugAnalyzeBlockWrite(stb, "minMaxDist ");
            stb = ar.GetMaxMatchNZDistance();
            DebugAnalyzeBlockWrite(stb, "MNZMinDist ");
            //stb = ar.GetMinStdDevDistance();
            //DebugAnalyzeBlockWrite(stb, "stdMinDist ");


            _sb.AppendLine();
            
        }

        private void DebugAnalyzeBlockWrite(StatByte sb,string prefixDebug)
        {
            if (sb != null)
                _sb.AppendLine($"{prefixDebug} byte:{sb.Byte,3} char:{(char)sb.Byte} count:{sb.CountByte,6}" +
                    $" avg dist: {sb.AvgDistance} Max dist:{sb.DistanceMax} matchD:{sb.DistancesMatchSame} matchDNZ: {sb.DistancesMatchSameNotZero} stddev:{sb.AvgStdDiffDistance}");
        }

        private void DebugMatchedWord(ArraySegmentEx_Byte data)
        {
            string outtput = string.Empty;
            for(int i =0;i< data.Count;i++)
            {
                outtput = outtput + ((char)data.Array[data.Offset +i]);
            }

            _sb.AppendLine($"Match word \"{outtput}\" position: {data.Offset}");
        }

        private void DebugNotMatchWord(ArraySegmentEx_Byte word)
        {
            char[] test = new char[word.Count];

            for (int i = 0; i < word.Count; i++)
            {
                test[i] = (char)word.Array[word.Offset + i];
            }
            _sb.Append(test);
            _sb.AppendLine();

            
        }
       

        private byte [] Encode(byte [] data, byte splitBy)
        {
            Encoder encoder = new Encoder(splitBy);
            //EncoderSplited encoder = new EncoderSplited(splitBy);

            Dictionary<ArraySegmentEx_Byte, short> lookup = new Dictionary<ArraySegmentEx_Byte, short>(_acpComparer);
            
            foreach(TokenWord token in SplitWords(data,splitBy))
            {

                if(token.word.Count == 0)
                {
                    encoder.EncodeTokenZeroLenght();
                   
                    continue;
                }

                //if (token.word.Count == 1)
                //{
                //    EncodeTokenNotMatch(token.word, result,splitBy);
                //    continue;
                //}


                if (lookup.ContainsKey(token.word))
                {
                    //DebugMatchedWord(token.word);
                    if (encoder.GetMatchEncodedLenght(lookup[token.word]) <= token.word.Count)
                    {
                        encoder.EncodeMatchToken(lookup[token.word], token.word);
                        
                    }
                    else
                    {
                        //AddWordToLookupAdvance(lookup, token.word);
                        EncodeTokenNotMatch(token.word,encoder);
                        
                    }
                }
                else
                {
                    AddWordToLookup(lookup, token.word);

                    //AddWordToLookupAdvance(lookup, token.word);
                    EncodeTokenNotMatch(token.word,  encoder);
                    DebugNotMatchWord(token.word);
                }
            }

            encoder.EncodeFinish();

            return encoder.Output;
        }

       
       

        private byte[] EncodeAdvance(byte[] data, byte splitBy, int maxEncodedLength, int hintCountWords, int recursiveLevel)
        {
            // Encoder encoder = new Encoder(splitBy);
            EncoderSplited encoder = new EncoderSplited(splitBy, maxEncodedLength, data.Length);

            //encoder.CountIndexEncodedAsOneByte
            TokenDictionary tokenDic = new TokenDictionary(
                encoder.CountIndexEncodedAsOneByte,
                hintCountWords
                );

            foreach (TokenWord token in SplitWords(data, splitBy))
            //SplitWordsTest swt = new SplitWordsTest(data, splitBy);

            //TokenWord token;
            //while (null != (token = swt.Next()))
            {
                ArraySegmentEx_Byte word = token.word;
                if (word.Count == 0)
                {
                    encoder.EncodeTokenZeroLenght();

                    continue;
                }
                            

                int matchIndex = tokenDic.MatchToken(word);
                if (matchIndex >= 0)
                {
                    //DebugMatchedWord(token.word);
                    if (encoder.GetMatchEncodedLenght(matchIndex) <= word.Count)
                    {
                        encoder.EncodeMatchToken(matchIndex, word);

                    }
                    else
                    {
                        //AddWordToLookupAdvance(lookup, token.word);
                        EncodeTokenNotMatch(word, encoder);
                    }

                    // aktualizuje cetnost nalezeni tokenu
                    tokenDic.AddOrUpdateCountToken(word);
                }
                else
                {
                    tokenDic.AddOrUpdateCountToken(word);

                    //AddWordToLookupAdvance(tokenDic, token.word);
                    EncodeTokenNotMatch(word, encoder);

                    //DebugNotMatchWord(token.word);
                }
            }

            encoder.EncodeFinish(recursiveLevel);

            return encoder.Output;
        }

        



        private static void AddWordToLookup(Dictionary<ArraySegmentEx_Byte, short> lookup, ArraySegmentEx_Byte word )
        {
            lookup.Add(word, (short)lookup.Count);
        }

        private static void AddWordToLookupAdvance(Dictionary<ArraySegmentEx_Byte, short> lookup, ArraySegmentEx_Byte word)
        {

            if (word.Count - 2 > 2)
            {
                ArraySegmentEx_Byte partWord = new ArraySegmentEx_Byte(word.Array, word.Offset + 1, word.Count - 2);

                AddWordToLookupIfNotExist(lookup, partWord);
            }

            if (word.Count - 1 > 2)
            {
                ArraySegmentEx_Byte partWord = new ArraySegmentEx_Byte(word.Array, word.Offset + 1, word.Count - 1);

                AddWordToLookupIfNotExist(lookup, partWord);

                partWord = new ArraySegmentEx_Byte(word.Array, word.Offset , word.Count-1);

                AddWordToLookupIfNotExist(lookup, partWord);

            }
        }

        private static void AddWordToLookupAdvance(TokenDictionary lookup, ArraySegmentEx_Byte word)
        {

            if (word.Count - 2 > 2)
            {
                ArraySegmentEx_Byte partWord = new ArraySegmentEx_Byte(word.Array, word.Offset + 1, word.Count - 2);

                AddWordToLookupIfNotExist(lookup, partWord);
            }

            if (word.Count - 1 > 2)
            {
                ArraySegmentEx_Byte partWord = new ArraySegmentEx_Byte(word.Array, word.Offset + 1, word.Count - 1);

                AddWordToLookupIfNotExist(lookup, partWord);

                partWord = new ArraySegmentEx_Byte(word.Array, word.Offset, word.Count - 1);

                AddWordToLookupIfNotExist(lookup, partWord);

            }
        }

        private static void AddWordToLookupIfNotExist(Dictionary<ArraySegmentEx_Byte, short> lookup, ArraySegmentEx_Byte word)
        {
            if (!lookup.ContainsKey(word))
            {
                lookup.Add(word, (short)lookup.Count);
            }
        }

        private static void AddWordToLookupIfNotExist(TokenDictionary tokenDic, ArraySegmentEx_Byte word)
        {
            if (! (tokenDic.MatchToken(word) < 0))
            {
                tokenDic.AddOrUpdateCountToken(word);
            }
        }



        private void EncodeTokenNotMatch(ArraySegmentEx_Byte word, Encoder encoder)
        {
            

            encoder.EncodeNotMatchToken(word);
        }

        private void EncodeTokenNotMatch(ArraySegmentEx_Byte word, EncoderSplited encoder)
        {
            

            encoder.EncodeNotMatchToken(word);
        }

        private void EncodeTokenZeroLenght(List<byte> result)
        {
                result.Add(0);
        }


        class SplitWordsTest
        {
            private byte[] _data;
            private byte _splitBy;
            private int _wordStart = 0;
            private int _wordLength = 0;
            private bool _end;

            public SplitWordsTest(byte[] data, byte splitBy)
            {
                _wordLength = 0;
                _wordStart = 0;
                _data = data;
                _splitBy = splitBy;
                _end = false;
            }

            public TokenWord Next()
            {
                if (_end) return null;

                int i = _wordStart;
                while (i < _data.Length)
                {
                    if (_data[i] != _splitBy)
                    {
                        _wordLength++;
                        i++;
                    }
                    else
                    {
                        TokenWord tokenWord = new TokenWord();
                        tokenWord.word = new ArraySegmentEx_Byte(_data, _wordStart, _wordLength);
                        _wordStart = i + 1;
                        _wordLength = 0;

                        return tokenWord;

                    }
                }

                {
                    TokenWord tokenWord = new TokenWord();
                    tokenWord.word = new ArraySegmentEx_Byte(_data, _wordStart, _wordLength);
                    tokenWord.isLastWord = true;
                    this._end = true;
                    return tokenWord;
                }
            }
        }

        public IEnumerable<TokenWord> SplitWords(byte [] data, byte splitBy)
        {
            int wordStart = 0;
            int wordLength = 0;

            for(int i = 0;i < data.Length;)
            {
                if(data[i] != splitBy)
                {
                    wordLength++;
                    i++;
                }
                else
                {
                    TokenWord tokenWord = new TokenWord();
                    tokenWord.word = new ArraySegmentEx_Byte(data, wordStart, wordLength);
                    yield return tokenWord;

                    wordStart = i + 1;
                    wordLength = 0;
                    i++;
                }
            }

            // posledni slovo konci delkou pole nikoliv odelovacem
            if(wordStart < data.Length)
            {
                TokenWord tokenWord = new TokenWord();
                tokenWord.word = new ArraySegmentEx_Byte(data, wordStart, wordLength);
                tokenWord.isLastWord = true;
                yield return tokenWord;

            }
        }

        public class TokenWord
        {
            public ArraySegmentEx_Byte word;
            public bool isLastWord;
        }
    }
}

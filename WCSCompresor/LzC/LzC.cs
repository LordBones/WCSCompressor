using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using ToolsLib.Basic;

namespace WCSCompress.LzC
{
    public class LzC
    {
        const int CONST_SizeLookAheadBuffer = 512;
        const int CONST_MaxMatchLenght = 250;

        class Stats
        {
            public int countMatched = 0;
            public int countMissMatched = 0;
            public int countPure = 0;
            public int countXTolerance = 0;
            public int countLBRedundant = 0;
            public int countLBMissing = 0;
            public int sumMatched = 0;
            public int sumReduceSize = 0;
            public int countPositionLess128 = 0;
            public int countLengthLess19 = 19;

        }

        public void Compress(Stream input, Stream output, int windowSize)
        {
            
            SlidingWindow sw = new SlidingWindow(windowSize);
            HashSSlideWindow hashSW = new HashSSlideWindow(12, windowSize);
            
            Stats stat = new Stats();

            SlidingWindow lookAhead = new SlidingWindow(CONST_SizeLookAheadBuffer);
            byte[] tmpBuffer = new byte[CONST_SizeLookAheadBuffer];
            
            int countReaded = 0;

            LzC_Core lzCore = new LzC_Core();
            
            Encode encoder = new Encode(output);
            LzC_Core.MatchResult fmResult = new LzC_Core.MatchResult();
            do
            {
                if (CONST_MaxMatchLenght > lookAhead.GetCurrWindowSize())
                {
                    countReaded = input.Read(tmpBuffer, 0, CONST_SizeLookAheadBuffer - lookAhead.GetCurrWindowSize());

                    lookAhead.AddByte(tmpBuffer,0, countReaded);

                    //for (int i = 0; i < countReaded; i++)
                    //{
                    //    lookAhead.AddByte(tmpBuffer[i]);
                    //}
                }

                //if(countReaded == 0)
                //{
                //    for (int i = 0; i < countReaded; i++)
                //    {
                //        output.WriteByte(lookAhead.GetWindowByte(tmpBuffer[i]));
                //    }

                //    lookAhead.Clear();
                //}
                //else if (lookAhead.GetCurrWindowSize() < CONST_SizeLookAheadBuffer)
                //{
                //    for (int i = 0; i < countReaded; i++)
                //    {
                //        output.WriteByte(lookAhead.GetWindowByte(tmpBuffer[i]));
                //    }

                //    lookAhead.Clear();
                //}
                //else
                {
                   

                   
                    if(lzCore.FindMatch2(sw,lookAhead, hashSW, CONST_MaxMatchLenght, ref fmResult))
                    {
                        //if(fmResult.OffsetOneUnMChar < 0)
                        //{
                        //    encoder.EncodeRaw(lookAhead.GetWindowFirstByte());
                        //    MoveFromLAheadToSlideWindowBytes(sw, lookAhead, 1);
                        //    continue;
                        //}
                        
                        if (fmResult.OffsetOneUnMChar >= 0)
                        {
                            stat.countMissMatched++;

                            if(fmResult.Type == LzC_Core.MatchResult.MatchType.LABXSeqMissing) stat.sumReduceSize += fmResult.Length - Encode.GetEncodeSize_lbXMissing_Match(fmResult.Offset, fmResult.Length, fmResult.OffsetOneUnMChar, fmResult.CountMissMatch, sw.GetCurrWindowSize());
                            else if (fmResult.Type == LzC_Core.MatchResult.MatchType.XSeqDifferent) stat.sumReduceSize += fmResult.Length - Encode.GetEncodeSize_XTolerance_Match(fmResult.Offset, fmResult.Length,fmResult.OffsetOneUnMChar,fmResult.CountMissMatch, sw.GetCurrWindowSize());
                            else stat.sumReduceSize += fmResult.Length - Encode.GetEncodeSize_lbXRedundant_Match(fmResult.Offset, fmResult.Length, fmResult.OffsetOneUnMChar, fmResult.CountMissMatch, sw.GetCurrWindowSize());
                        }
                        else
                        {
                            stat.sumReduceSize += fmResult.Length - Encode.GetEncodeSize_PureMatch(fmResult.Offset, fmResult.Length, sw.GetCurrWindowSize()); 
                        }


                        if (fmResult.Type == LzC_Core.MatchResult.MatchType.Pure) ++stat.countPure ;
                        if (fmResult.Type == LzC_Core.MatchResult.MatchType.XSeqDifferent) ++stat.countXTolerance;
                        if (fmResult.Type == LzC_Core.MatchResult.MatchType.LABXSeqRedundant) ++stat.countLBRedundant;
                        if (fmResult.Type == LzC_Core.MatchResult.MatchType.LABXSeqMissing) ++stat.countLBMissing;


                        stat.countMatched++;
                        
                        stat.sumMatched += fmResult.Length;

                        int reverseOffset = sw.GetCurrWindowSize()  - fmResult.Offset- fmResult.Length;
                        if (fmResult.Type == LzC_Core.MatchResult.MatchType.LABXSeqMissing) reverseOffset = sw.GetCurrWindowSize() - fmResult.Offset - fmResult.Length - fmResult.CountMissMatch;
                        else if (fmResult.Type == LzC_Core.MatchResult.MatchType.LABXSeqRedundant) reverseOffset = sw.GetCurrWindowSize() - fmResult.Offset - fmResult.Length + fmResult.CountMissMatch;


                        if (reverseOffset < 257) stat.countPositionLess128++;
                        if (fmResult.Length < 20) stat.countLengthLess19++;

                        encoder.EncodeMatch(reverseOffset, fmResult.Length, fmResult.OffsetOneUnMChar >= 0, fmResult.OffsetOneUnMChar,fmResult.CountMissMatch, fmResult.Type);

                        //lookAhead.RemoveLeft(1);
                        MoveFromLAheadToSlideWindowBytes(sw, lookAhead, hashSW, fmResult.Length);

                        //MoveFromLAheadToSlideWindowBytes(sw, lookAhead, fmResult.Length);
                        //lookAhead.RemoveLeft(fmResult.Length);

                        // nutne protoze pri pokud najde slovo delky lookahead ta skonci predcasne
                        
                    }
                    else
                    {
                        //encoder.EncodeRaw(lookAhead,0,2);
                        //MoveFromLAheadToSlideWindowBytes(sw, lookAhead, 2, ref lookAheadCount);

                        encoder.EncodeRaw(lookAhead.GetWindowFirstByte());
                        MoveFromLAheadToSlideWindowBytes(sw, lookAhead,hashSW, 1);
                    }
                }

                // podminka je slozena protoze kdyz najdeme slovo velikosti lookahead bufferu skoncili bychom predcasne
            } while (lookAhead.GetCurrWindowSize() > 0 || countReaded > 0 );

            PrintStat(stat);
        }

        private void PrintStat(Stats stat)
        {
            Console.WriteLine($"Mcount: {stat.countMatched}  Mlen: {stat.sumMatched} Save: {stat.sumReduceSize}");
            Console.WriteLine($"MissC: {stat.countMissMatched}  PureC: {stat.countPure} XDiff:{stat.countXTolerance} LBRedd:{stat.countLBRedundant} LBMiss:{stat.countLBMissing}");

            Console.WriteLine($"PosLess256: {stat.countPositionLess128}  LenLess19: {stat.countLengthLess19}");

        }

        private void MoveFromLAheadToSlideWindowBytes(SlidingWindow sw, SlidingWindow lookAhead, HashSSlideWindow hashSW , int moveCountBytes)
        {
            //var tmp = lookAhead.GetAsSegmentArray();
            //sw.AddByte(tmp.Array, tmp.Offset, moveCountBytes);

            //hashSW.MoveAddEmpty(moveCountBytes, sw);

            for (int i = 0; i < moveCountBytes; i++)
            {

                hashSW.AchjoAddByte(sw, lookAhead.GetWindowByte(i));
            }

            //for (int i = 0; i < moveCountBytes; i++)
            //{
            
            //    hashSW.MoveAddEmpty(1, sw);
            //    sw.AddByte(lookAhead.GetWindowByte(i));
            //}

            //hashSW.UpdateAllEmpty(sw);

            lookAhead.RemoveLeft(moveCountBytes);
        }

        private void MoveFromLAheadToSlideWindowBytes2(SlidingWindow sw, byte[] lookAhead, int moveCountBytes, ref int lookAheadCount)
        {
            //for (int i = 0; i < moveCountBytes; i++)
            //{
            //    sw.AddByte(lookAhead[i]);
            //}

            for (int i = 0, i2 = moveCountBytes; i2 < lookAheadCount; i++, i2++)
            {
                lookAhead[i] = lookAhead[i2];
            }

            lookAheadCount -= moveCountBytes;
        }
    }
    public class LzC_Core
    {
        public struct MatchResult
        {
            public short EncodeSize;
            public enum MatchType { Pure = 0, XSeqDifferent, LABXSeqRedundant, LABXSeqMissing }
            public MatchType Type;
            public int Offset;
            public short Length;
            public short OffsetOneUnMChar;
            public short CountMissMatch;
            public byte UnMChar;

           

            public MatchResult(int offset, short lenght, short offsetOneUnMChar, byte unMChar, short countMissmatch, MatchType type)
            {
                this.Length = lenght;
                this.Offset = offset;
                this.OffsetOneUnMChar = offsetOneUnMChar;
                this.UnMChar = unMChar;
                this.Type = type;
                this.CountMissMatch = countMissmatch;
                this.EncodeSize = short.MaxValue;
            }

            public void Set(ref MatchResult data)
            {
                this.CountMissMatch = data.CountMissMatch;
                this.EncodeSize = data.EncodeSize;
                this.Length = data.Length;
                this.Offset = data.Offset;
                this.OffsetOneUnMChar = data.OffsetOneUnMChar;
                this.Type = data.Type;
                this.UnMChar = data.UnMChar;
            }

            public void Set(int offset, short lenght, short offsetOneUnMChar, byte unMChar, short countMissmatch, MatchType type)
            {
                this.Length = lenght;
                this.Offset = offset;
                this.OffsetOneUnMChar = offsetOneUnMChar;
                this.UnMChar = unMChar;
                this.Type = type;
                this.CountMissMatch = countMissmatch;
            }

            public void Set_Pure(int offset, short lenght)
            {
                this.Length = lenght;
                this.Offset = offset;
                this.CountMissMatch = 0;
                this.OffsetOneUnMChar = -1;
                this.Type =MatchType.Pure;
            }

            public void Set_XTolerance(int offset, short lenght, short offsetMissmatch, short countMissMatch)
            {
                this.Length = lenght;
                this.Offset = offset;
                this.CountMissMatch = countMissMatch;
                this.OffsetOneUnMChar = offsetMissmatch;
                this.Type = MatchType.XSeqDifferent;
            }

            public void Set_LABXSeqRedundant(int offset, short lenght, short offsetMissmatch, short countMissMatch)
            {
                this.Length = lenght;
                this.Offset = offset;
                this.CountMissMatch = countMissMatch;
                this.OffsetOneUnMChar = offsetMissmatch;
                this.Type = MatchType.LABXSeqRedundant;
            }

            public void Set_LABXSeqMissing(int offset, short lenght, short offsetMissmatch, short countMissMatch)
            {
                this.Length = lenght;
                this.Offset = offset;
                this.CountMissMatch = countMissMatch;
                this.OffsetOneUnMChar = offsetMissmatch;
                this.Type = MatchType.LABXSeqMissing;
            }

            public void Set_EndcodeSize(short  encodeSize)
            {
                this.EncodeSize = encodeSize;
            }
        }

        struct SingleMatch_Pure_Result
        {
            public short Length;
            public short EncodedSize;

            public void Set(short lenght, short encodedSize)
            {
                this.Length = lenght;
                this.EncodedSize = encodedSize;
            }
        }

        struct SingleMatch_XTolerance_Result
        {
            public short Length;
            public short EncodedSize;
            public short OffsetOneUnMChar;
            public short CountMissMatch;

            public void Set(short lenght, short offsetOneUnMchar, short countMissMatch,  short encodedSize)
            {
                this.Length = lenght;
                this.EncodedSize = encodedSize;
                this.OffsetOneUnMChar = offsetOneUnMchar;
                this.CountMissMatch = countMissMatch;
            }
        }

        struct SingleMatch_lbXRedundant_Result
        {
            public short Length;
            public short EncodedSize;
            public short OffsetOneUnMChar;
            public short CountMissMatch;

            public void Set(short lenght, short offsetOneUnMchar, short countMissMatch, short encodedSize)
            {
                this.Length = lenght;
                this.EncodedSize = encodedSize;
                this.OffsetOneUnMChar = offsetOneUnMchar;
                this.CountMissMatch = countMissMatch;
            }
        }

        struct SingleMatch_lbXMissing_Result
        {
            public short Length;
            public short EncodedSize;
            public short OffsetOneUnMChar;
            public short CountMissMatch;

            public void Set(short lenght, short offsetOneUnMchar, short countMissMatch, short encodedSize)
            {
                this.Length = lenght;
                this.EncodedSize = encodedSize;
                this.OffsetOneUnMChar = offsetOneUnMchar;
                this.CountMissMatch = countMissMatch;
            }
        }
        

        public bool FindMatch(SlidingWindow sw, SlidingWindow lookAheadBuffer, HashSSlideWindow hashSW, int maxLenMatch, ref MatchResult resultMatch)
        {
            MatchResult best = new MatchResult();
            int bestLength = 0;
            MatchResult tempResultMatch = new MatchResult();

            if (FindMatch_XTolerance_Faster(sw, lookAheadBuffer, hashSW, maxLenMatch, ref tempResultMatch))
            {

                int positionSize = Encode.GetEncodePosition_Size(sw.GetCurrWindowSize() - (tempResultMatch.Offset + tempResultMatch.Length) );
                if (tempResultMatch.OffsetOneUnMChar >= 0 && tempResultMatch.Length - positionSize - 1 - tempResultMatch.CountMissMatch > bestLength)
                {
                    best = tempResultMatch;
                    bestLength = tempResultMatch.Length - positionSize - 1 - tempResultMatch.CountMissMatch;
                }
            }

            //if (FindMatch_lbXRedundant_Faster(sw, lookAheadBuffer, hashSW, maxLenMatch, ref tempResultMatch))
            //    {
            //    int positionSize = Encode.GetEncodePosition_Size(tempResultMatch.Offset, tempResultMatch.Length, sw.GetCurrWindowSize());
            //    if (tempResultMatch.OffsetOneUnMChar >= 0 && tempResultMatch.Length - positionSize - 2 > bestLength)
            //    {
            //        best = tempResultMatch;
            //        bestLength = tempResultMatch.Length - 2 - positionSize;
            //    }
            //}

            //if (FindMatch_lbXMissing_Faster(sw, lookAheadBuffer, hashSW, maxLenMatch, ref tempResultMatch))
            //{
            //    int positionSize = Encode.GetEncodePosition_Size(tempResultMatch.Offset, tempResultMatch.Length, sw.GetCurrWindowSize());
            //    if (tempResultMatch.OffsetOneUnMChar >= 0 && tempResultMatch.Length - 1 - positionSize > bestLength)
            //    {
            //        best = tempResultMatch;
            //        bestLength = tempResultMatch.Length - 1 - positionSize;
            //    }
            //}



            if (FindMatch_PureFaster(sw, lookAheadBuffer, hashSW, maxLenMatch, ref tempResultMatch))
                {
                int positionSize = Encode.GetEncodePosition_Size(sw.GetCurrWindowSize() - (tempResultMatch.Offset + tempResultMatch.Length));
                if (tempResultMatch.Length - positionSize >= bestLength)
                {
                    best = tempResultMatch;
                    bestLength = tempResultMatch.Length - positionSize;
                }
            }

            if (bestLength > 0)
            {
                resultMatch = best;
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool FindMatch2(SlidingWindow sw, SlidingWindow lookAheadBuffer, HashSSlideWindow hashSW, int maxLenMatch, ref MatchResult resultMatch)
        {
            MatchResult best = new MatchResult();
            int bestLength = 0;
            MatchResult tempResultMatch = new MatchResult();

           

            if(FindMatch3_Basic(sw, lookAheadBuffer, hashSW, maxLenMatch, ref tempResultMatch))
            {
                //int positionSize = Encode.GetEncodePosition_Size(tempResultMatch.Offset, tempResultMatch.Length, sw.GetCurrWindowSize());
                //if (tempResultMatch.Length - positionSize >= bestLength)
                {
                    best = tempResultMatch;
                    bestLength = tempResultMatch.Length;// - positionSize;
                }
            }
            
            if (bestLength > 0)
            {
                resultMatch = best;
                return true;
            }
            else
            {
                return false;
            }

        }


        public bool FindMatch3_Basic(SlidingWindow sw, SlidingWindow lookAheadBuffer, HashSSlideWindow hashSW, int maxLenMatch, ref MatchResult resultMatch)
        {

            MatchResult bestresultMatch = new MatchResult();
           
            int currWindowSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();
            // vypocet max bezpecne lookAheadBufferIterace
            lookAheadBufferSize = (lookAheadBufferSize > maxLenMatch) ? maxLenMatch : lookAheadBufferSize;

            int hash = hashSW.ComputeHash(lookAheadBuffer, 0);

            int hashMatchIndex = (hash < 0) ? -1 : hashSW.Get_StartMatchIndex(hash);

            SingleMatch_Pure_Result sm_p_r = new SingleMatch_Pure_Result();
            SingleMatch_XTolerance_Result sm_xt_r = new SingleMatch_XTolerance_Result();
            SingleMatch_lbXRedundant_Result sm_lbxr_r = new SingleMatch_lbXRedundant_Result();
            SingleMatch_lbXMissing_Result sm_lbxm_r = new SingleMatch_lbXMissing_Result();


            while (hashMatchIndex >= 0)
            {
                int i = hashMatchIndex;


                if (Match_Pure(sw, currWindowSize, i, lookAheadBuffer, maxLenMatch, ref sm_p_r))
                {

                    int diffSaveByte = (bestresultMatch.Length - bestresultMatch.EncodeSize) - (sm_p_r.Length - sm_p_r.EncodedSize);
                    if (diffSaveByte < 0 || (diffSaveByte == 0 && bestresultMatch.Length > sm_p_r.Length))
                    {
                        bestresultMatch.Set_Pure(i, sm_p_r.Length);
                        bestresultMatch.Set_EndcodeSize(sm_p_r.EncodedSize);
                    }

                }

                //if (Match_XTolerance(sw, i, lookAheadBuffer, maxLenMatch, ref sm_xt_r))
                //{

                //    int diffSaveByte = (bestresultMatch.Length - bestresultMatch.EncodeSize) - (sm_xt_r.Length - sm_xt_r.EncodedSize);
                //    if (diffSaveByte < 0 || (diffSaveByte == 0 && bestresultMatch.Length > sm_xt_r.Length))
                //    {
                //        bestresultMatch.Set_XTolerance(i, sm_xt_r.Length, sm_xt_r.OffsetOneUnMChar, sm_xt_r.CountMissMatch);
                //        bestresultMatch.Set_EndcodeSize(sm_xt_r.EncodedSize);
                //    }

                //}

                //if (Match_lbXRedundant(sw, i, lookAheadBuffer, maxLenMatch, ref sm_lbxr_r))
                //{

                //    int diffSaveByte = (bestresultMatch.Length - bestresultMatch.EncodeSize) - (sm_lbxr_r.Length - sm_lbxr_r.EncodedSize);
                //    if (diffSaveByte < 0 || (diffSaveByte == 0 && bestresultMatch.Length > sm_lbxr_r.Length))
                //    {
                //        bestresultMatch.Set_LABXSeqRedundant(i, sm_lbxr_r.Length, sm_lbxr_r.OffsetOneUnMChar, sm_lbxr_r.CountMissMatch);
                //        bestresultMatch.Set_EndcodeSize(sm_lbxr_r.EncodedSize);
                //    }

                //}

                if (FindMatch_lbXMissing(sw, i, lookAheadBuffer, maxLenMatch, ref sm_lbxm_r))
                {

                    int diffSaveByte = (bestresultMatch.Length - bestresultMatch.EncodeSize) - (sm_lbxm_r.Length - sm_lbxm_r.EncodedSize);
                    if (diffSaveByte < 0 || (diffSaveByte == 0 && bestresultMatch.Length > sm_lbxm_r.Length))
                    {
                        bestresultMatch.Set_LABXSeqMissing(i, sm_lbxm_r.Length, sm_lbxm_r.OffsetOneUnMChar, sm_lbxm_r.CountMissMatch);
                        bestresultMatch.Set_EndcodeSize(sm_lbxm_r.EncodedSize);
                    }

                }

                int distance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                hashMatchIndex = (distance > 0) ? hashMatchIndex + distance : -1;
            }

            if (bestresultMatch.Length > 2)
            {

                resultMatch = bestresultMatch;
                resultMatch.Set(ref bestresultMatch);
                return true;
            }

            //resultMatch = new MatchResult();
            return false;
        }


        /// <summary>
        ///  min match 3
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="swIndex"></param>
        /// <param name="lookAheadBuffer"></param>
        /// <param name="maxLenMatch"></param>
        /// <param name="resultMatch"></param>
        /// <returns></returns>
        private bool Match_Pure(SlidingWindow sw, int swSize, int swIndex, SlidingWindow lookAheadBuffer, int maxLenMatch, ref SingleMatch_Pure_Result resultMatch)
        {
            //int swSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();
            int lenghtToEnd = swSize - swIndex;

            int lookAheadBufferMax = (lenghtToEnd < lookAheadBufferSize) ? lenghtToEnd : lookAheadBufferSize;

            int tmpDiff = (sw.GetUshort(swIndex) ^ lookAheadBuffer.GetUshort(0))
                       | (sw.GetWindowByte(swIndex + 2) ^ lookAheadBuffer.GetWindowByte(2));

            int sequenceLength = 0;
            if (tmpDiff == 0)
            {
                sequenceLength = 3;

                for (int m = 3; m < lookAheadBufferMax; ++m)
                {
                    int index = swIndex + m;
                    if (sw.GetWindowByte(index) == lookAheadBuffer.GetWindowByte(m))
                    //if (sw[index] == lookAheadBuffer[m])
                    {
                        ++sequenceLength;
                    }
                    else
                    {
                        break;
                    }
                }

                //if (sequenceLength > 0)
                {
                    short encodedSize = Encode.GetEncodeSize_PureMatch(swIndex, sequenceLength, swSize);
                    resultMatch.Set((short)sequenceLength, encodedSize);
                    return true;
                }
            }

           

            return false;

        }

        /// <summary>
        ///  min match 3
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="swIndex"></param>
        /// <param name="lookAheadBuffer"></param>
        /// <param name="maxLenMatch"></param>
        /// <param name="resultMatch"></param>
        /// <returns></returns>
        private bool Match_XTolerance(SlidingWindow sw, int swIndex, SlidingWindow lookAheadBuffer, int maxLenMatch, ref SingleMatch_XTolerance_Result resultMatch)
        {
            int swSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();
            int lenghtToEnd = swSize - swIndex;

            int lookAheadBufferMax = (lenghtToEnd < lookAheadBufferSize) ? lenghtToEnd : lookAheadBufferSize;

            //int tmpDiff = (sw.GetWindowByte(swIndex) ^ lookAheadBuffer.GetWindowByte(0))
            //            | (sw.GetWindowByte(swIndex + 1) ^ lookAheadBuffer.GetWindowByte(1))
            //            | (sw.GetWindowByte(swIndex + 2) ^ lookAheadBuffer.GetWindowByte(2));

            int tmpDiff = (sw.GetUshort(swIndex) ^ lookAheadBuffer.GetUshort(0))
                        | (sw.GetWindowByte(swIndex + 2) ^ lookAheadBuffer.GetWindowByte(2));


            int sequenceLength = 0;
            if (tmpDiff == 0)
            {
                sequenceLength = 3;

                int countMissMatch = 0;
                int offsetMissMatch = -1;
                bool forbiddenmiss = false;
                // najde nejdelsi sekvenci
                for (int m = 3; m < lookAheadBufferMax; m++)
                {
                    int index = swIndex + m;
                    //if (index < currWindowSize   && sequenceLength <= maxLenMatch)
                    {
                        int diff = sw.GetWindowByte(index) - lookAheadBuffer.GetWindowByte(m);

                        if (diff != 0 && countMissMatch < 8 && !forbiddenmiss)
                        {
                            countMissMatch++;
                            offsetMissMatch = m;
                            sequenceLength++;

                            if (m < 2) break;
                        }
                        else if (diff == 0)
                        {
                            if (countMissMatch > 0)
                            {
                                forbiddenmiss = true;
                            }

                            sequenceLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                if (offsetMissMatch > 32 || offsetMissMatch < 0)
                {
                    return false;
                }
                // podminka rusi ssssssXXXXs nebo ssssssXXXX kdy je konec prilis kratky nebo jeho zakodovani neprinasi zisk
                else if (offsetMissMatch + countMissMatch  >= sequenceLength -1)
                {
                    return false;
                }


                short encodedSize = Encode.GetEncodeSize_XTolerance_Match(swIndex, sequenceLength,offsetMissMatch,countMissMatch, swSize);
                    resultMatch.Set((short)sequenceLength, (short)offsetMissMatch,(short) countMissMatch, encodedSize);
                    return true;
            }
            
            return false;

        }

        /// <summary>
        ///  min match 3
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="swIndex"></param>
        /// <param name="lookAheadBuffer"></param>
        /// <param name="maxLenMatch"></param>
        /// <param name="resultMatch"></param>
        /// <returns></returns>
        private bool Match_lbXRedundant(SlidingWindow sw, int swIndex, SlidingWindow lookAheadBuffer, int maxLenMatch, ref SingleMatch_lbXRedundant_Result resultMatch)
        {
            int swSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();
            int lenghtToEnd = swSize - swIndex;

            int lookAheadBufferMax = (lenghtToEnd < lookAheadBufferSize) ? lenghtToEnd : lookAheadBufferSize;

            int tmpDiff = (sw.GetUshort(swIndex) ^ lookAheadBuffer.GetUshort(0))
                     | (sw.GetWindowByte(swIndex + 2) ^ lookAheadBuffer.GetWindowByte(2));

            int sequenceLength = 0;
            if (tmpDiff == 0)
            {
                sequenceLength = 3;

                int countMissMatch = 0;
                int offsetMissMatch = -1;
                int cwIndex = swIndex+3;

                bool forbiddenmiss = false;
                // najde nejdelsi sekvenci
                for (int m = 3; m < lookAheadBufferMax; m++)
                {
                    if (cwIndex < swSize //&& sequenceLength <= maxLenMatch
                       )
                    {

                        int diff = sw.GetWindowByte(cwIndex) - lookAheadBuffer.GetWindowByte(m);

                        if (diff != 0 && countMissMatch < 8 && !forbiddenmiss)
                        {
                            countMissMatch++;
                            offsetMissMatch = m;
                            sequenceLength++;

                            if (m < 2) break;
                        }
                        else if (diff == 0)
                        {
                            if (countMissMatch > 0)
                            {
                                forbiddenmiss = true;
                            }

                            sequenceLength++;
                            cwIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (countMissMatch >= 32 || offsetMissMatch < 0)
                {
                    return false;
                }
                // podminka rusi ssssssXXXXs nebo ssssssXXXX kdy je konec prilis kratky nebo jeho zakodovani neprinasi zisk
                else if (offsetMissMatch + countMissMatch >= sequenceLength -1)
                {
                    return false;
                }


               

                short encodedSize = Encode.GetEncodeSize_lbXRedundant_Match(swIndex, sequenceLength, offsetMissMatch, countMissMatch, swSize);
                resultMatch.Set((short)sequenceLength, (short)offsetMissMatch, (short)countMissMatch, encodedSize);
                return true;
            }

            return false;

        }

        /// <summary>
        ///  min match 3
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="swIndex"></param>
        /// <param name="lookAheadBuffer"></param>
        /// <param name="maxLenMatch"></param>
        /// <param name="resultMatch"></param>
        /// <returns></returns>
        private bool FindMatch_lbXMissing(SlidingWindow sw, int swIndex, SlidingWindow lookAheadBuffer, int maxLenMatch, ref SingleMatch_lbXMissing_Result resultMatch)
        {
            int swSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();
            int lenghtToEnd = swSize - swIndex;

            int lookAheadBufferMax = (lenghtToEnd < lookAheadBufferSize) ? lenghtToEnd : lookAheadBufferSize;

            int tmpDiff = (sw.GetUshort(swIndex) ^ lookAheadBuffer.GetUshort(0))
                      | (sw.GetWindowByte(swIndex + 2) ^ lookAheadBuffer.GetWindowByte(2));

            int sequenceLength = 0;
            if (tmpDiff == 0)
            {
                sequenceLength = 3;

                int countMissMatch = 0;
                int offsetMissMatch = -1;
                int cwIndex = swIndex+3;

                bool forbiddenmiss = false;
                // najde nejdelsi sekvenci
                for (int m = 3; m < lookAheadBufferMax; ++cwIndex)
                {
                    if (cwIndex < swSize //&& sequenceLength <= maxLenMatch
                       )
                    {

                        int diff = sw.GetWindowByte(cwIndex) - lookAheadBuffer.GetWindowByte(m);

                        if (diff != 0 && countMissMatch < 32 && !forbiddenmiss)
                        {
                            countMissMatch++;
                            offsetMissMatch = m;
                           

                            if (m < 2) break;
                        }
                        else if (diff == 0)
                        {

                            if (countMissMatch > 0)
                            {
                                forbiddenmiss = true;
                            }

                            sequenceLength++;
                            ++m;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (offsetMissMatch > 255 || offsetMissMatch < 0)
                {
                    return false;
                }
                // podminka rusi ssssssXXXXs nebo ssssssXXXX kdy je konec prilis kratky nebo jeho zakodovani neprinasi zisk
                else if (offsetMissMatch  >= sequenceLength-1 )
                {
                    return false;
                }




                short encodedSize = Encode.GetEncodeSize_lbXMissing_Match(swIndex, sequenceLength, offsetMissMatch, countMissMatch, swSize);
                resultMatch.Set((short)sequenceLength, (short)offsetMissMatch, (short)countMissMatch, encodedSize);
                return true;
            }

            return false;

        }


        


        public bool FindMatch_PureFaster(SlidingWindow sw, SlidingWindow lookAheadBuffer, HashSSlideWindow hashSW, int maxLenMatch, ref MatchResult resultMatch)
        {
            //return false;
            int maxShort = 0;
            int maxSequenceLenght = 0;
            int sequenceOffset = 0;

            int currWindowSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();
            // vypocet max bezpecne lookAheadBufferIterace
            lookAheadBufferSize = (lookAheadBufferSize > maxLenMatch) ? maxLenMatch : lookAheadBufferSize;

            int hash = hashSW.ComputeHash(lookAheadBuffer, 0);

            int hashMatchIndex = (hash < 0) ? -1 : hashSW.Get_StartMatchIndex(hash);

            while(hashMatchIndex >= 0)
            {
                int i = hashMatchIndex;

                int sequenceLength = 0;

                int lenghtToEnd = currWindowSize - i;

                int lookAheadBufferMax = (lenghtToEnd < lookAheadBufferSize) ? lenghtToEnd : lookAheadBufferSize;

                int tmpDiff = (sw.GetWindowByte(i) ^ lookAheadBuffer.GetWindowByte(0)) 
                    | (sw.GetWindowByte(i + 1) ^ lookAheadBuffer.GetWindowByte(1))
                    | (sw.GetWindowByte(i + 2) ^ lookAheadBuffer.GetWindowByte(2));

                if (tmpDiff == 0)
                {
                    sequenceLength += 3;
                    // najde nejdelsi sekvenci
                    for (int m = 3; m < lookAheadBufferMax; ++m)
                    {
                        int index = i + m;
                        if (sw.GetWindowByte(index) == lookAheadBuffer.GetWindowByte(m))
                        //if (sw[index] == lookAheadBuffer[m])
                        {
                            sequenceLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                int encodedPosition = Encode.GetEncodePosition_Size(currWindowSize- (i+sequenceLength) );
                int saveLen = sequenceLength - encodedPosition - 1;

                if (saveLen  >= maxShort && sequenceLength > 0)
                {
                    maxSequenceLenght = sequenceLength;
                    sequenceOffset = i;
                    maxShort = saveLen;

                }

                int distance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                hashMatchIndex = (distance > 0) ? hashMatchIndex + distance : -1;

            }

            if (maxSequenceLenght > 3)
            {

                resultMatch.Set(sequenceOffset, (short)maxSequenceLenght, -1, 0,0, MatchResult.MatchType.Pure);
                return true;
            }

            //resultMatch = new MatchResult();
            return false;
        }



      

       

        /// <summary>
        /// jeden znak je v obou slovech ruzny
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="lookAheadBuffer"></param>
        /// <param name="maxLenMatch"></param>
        /// <param name="resultMatch"></param>
        /// <returns></returns>
        public bool FindMatch_XTolerance_Faster(SlidingWindow sw, SlidingWindow lookAheadBuffer, HashSSlideWindow hashSW, int maxLenMatch, ref MatchResult resultMatch)
        {
            //return false;
            int maxSequenceLenght = 0;
            int maxShort = 0;
            int sequenceOffset = 0;
            int bestCountMisMatch = 0;
            int bestOffsetMissMatch = -1;

            int currWindowSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();

            // vypocet max bezpecne lookAheadBufferIterace
            lookAheadBufferSize = (lookAheadBufferSize > maxLenMatch) ? maxLenMatch : lookAheadBufferSize;

            int hash = hashSW.ComputeHash(lookAheadBuffer, 0);

            int hashMatchIndex = (hash < 0) ? -1 : hashSW.Get_StartMatchIndex(hash);

            while (hashMatchIndex >= 0)
            {
                int i = hashMatchIndex;

                int sequenceLength = 0;
                int countMissMatch = 0;
                int offsetMissMatch = -1;

                int lenghtToEnd = currWindowSize - i;

                int lookAheadBufferMax = (lenghtToEnd < lookAheadBufferSize) ? lenghtToEnd : lookAheadBufferSize;

                int tmpDiff = (sw.GetWindowByte(i) ^ lookAheadBuffer.GetWindowByte(0))
                  | (sw.GetWindowByte(i + 1) ^ lookAheadBuffer.GetWindowByte(1))
                  | (sw.GetWindowByte(i + 2) ^ lookAheadBuffer.GetWindowByte(2));

                if (tmpDiff == 0)
                {
                    sequenceLength += 3;
                    bool forbiddenmiss = false;
                    // najde nejdelsi sekvenci
                    for (int m = 3; m < lookAheadBufferMax; m++)
                    {
                        int index = i + m;
                        //if (index < currWindowSize   && sequenceLength <= maxLenMatch)
                        {
                            int diff = sw.GetWindowByte(index) - lookAheadBuffer.GetWindowByte(m);

                            if (diff != 0 && countMissMatch < 4 && !forbiddenmiss )
                            {
                                countMissMatch++;
                                offsetMissMatch = m;
                                sequenceLength++;

                                if (m < 2) break;
                            }
                            else if (diff == 0)
                            {
                                if(countMissMatch > 0)
                                {
                                    forbiddenmiss = true;
                                }

                                sequenceLength++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        //else
                        //{
                        //    break;
                        //}
                    }


                    //min match je 3 podminka neni nutna
                    //if (offsetMissMatch < 2 && offsetMissMatch >= 0) {

                    //    int tmpDistance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                    //    hashMatchIndex = (tmpDistance > 0) ? hashMatchIndex + tmpDistance : -1;
                    //    continue;
                    //}
                    if(offsetMissMatch > 64)
                    {
                            int tmpDistance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                            hashMatchIndex = (tmpDistance > 0) ? hashMatchIndex + tmpDistance : -1;
                            continue;
                    }

                    //if (offsetMissMatch == sequenceLength - 2)
                    //{
                    //    sequenceLength -= 2;
                    //    offsetMissMatch = -1;
                    //    countMissMatch = 0;
                    //}
                    else if (offsetMissMatch+countMissMatch-1 == sequenceLength - 1)
                    {
                        sequenceLength -= countMissMatch;
                        offsetMissMatch = -1;
                        countMissMatch = 0;
                    }


                    // to je proto ze dva znaky je delsi overhead kdyz je pouzite 
                    // slovo s jednim chybejicim znakem
                    int encodedPosition = Encode.GetEncodePosition_Size(currWindowSize - (i + sequenceLength));
                    int shorted = sequenceLength - encodedPosition;

                    
                    if (countMissMatch > 0) shorted -= 1+countMissMatch;


                    if (((sequenceLength > 4+countMissMatch && countMissMatch > 0) || (sequenceLength > 3 && countMissMatch == 0))
                        && (shorted > maxShort
                        || (shorted == maxShort
                          && countMissMatch < bestCountMisMatch
                        )
                        ))
                    {
                        maxSequenceLenght = sequenceLength;
                        bestOffsetMissMatch = offsetMissMatch;
                        maxShort = shorted;
                        sequenceOffset = i;
                        bestCountMisMatch = countMissMatch;
                    }
                }

                int distance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                hashMatchIndex = (distance > 0) ? hashMatchIndex + distance : -1;
            }

            if (maxSequenceLenght > 3)
            {

                resultMatch.Set(sequenceOffset, (short)maxSequenceLenght, (short)bestOffsetMissMatch, 0,(short)bestCountMisMatch, MatchResult.MatchType.XSeqDifferent);
                return true;
            }

            //resultMatch = new MatchResult();
            return false;
        }

        

        /// <summary>
        ///  v nalezenem slove je je jeden znak navic
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="lookAheadBuffer"></param>
        /// <param name="maxLenMatch"></param>
        /// <param name="resultMatch"></param>
        /// <returns></returns>
        public bool FindMatch_lbXRedundant_Faster(SlidingWindow sw, SlidingWindow lookAheadBuffer, HashSSlideWindow hashSW, int maxLenMatch, ref MatchResult resultMatch)
        {
            //return false;
            int maxSequenceLenght = 0;
            int maxShort = 0;
            int sequenceOffset = 0;
            int bestOffsetMissMatch = -1;

            int currWindowSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();

            // vypocet max bezpecne lookAheadBufferIterace
            lookAheadBufferSize = (lookAheadBufferSize > maxLenMatch) ? maxLenMatch : lookAheadBufferSize;

            int hash = hashSW.ComputeHash(lookAheadBuffer, 0);

            int hashMatchIndex = (hash < 0) ? -1 : hashSW.Get_StartMatchIndex(hash);

            while (hashMatchIndex >= 0)
            {
                int i = hashMatchIndex;
                int sequenceLength = 0;
                int countMissMatch = 0;
                int offsetMissMatch = -1;
                int cwIndex = i;

                //int lenghtToEnd = currWindowSize - i;

                //int lookAheadBufferMax = (lenghtToEnd < lookAheadBufferSize) ? lenghtToEnd : lookAheadBufferSize;

                // najde nejdelsi sekvenci

                int tmpDiff = (sw.GetWindowByte(i) ^ lookAheadBuffer.GetWindowByte(0))
               | (sw.GetWindowByte(i + 1) ^ lookAheadBuffer.GetWindowByte(1))
               | (sw.GetWindowByte(i + 2) ^ lookAheadBuffer.GetWindowByte(2));

                if (tmpDiff == 0)
                {
                    sequenceLength += 3;
                    cwIndex += 3;
                    for (int m = 3; m < lookAheadBufferSize; m++)
                    {

                        if (cwIndex < currWindowSize //&& sequenceLength <= maxLenMatch
                            )
                        {
                            int diff = sw.GetWindowByte(cwIndex) - lookAheadBuffer.GetWindowByte(m);

                            if (diff != 0 && countMissMatch < 1)
                            {
                                countMissMatch++;
                                offsetMissMatch = m;
                                sequenceLength++;

                                if (m < 2) break;
                            }
                            else if (diff == 0)
                            {
                                sequenceLength++;
                                cwIndex++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (offsetMissMatch < 2 && offsetMissMatch >= 0)
                {
                    int tmpDistance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                    hashMatchIndex = (tmpDistance > 0) ? hashMatchIndex + tmpDistance : -1;
                    continue;
                }


                if (offsetMissMatch == sequenceLength - 2)
                {
                    sequenceLength -= 2;
                    offsetMissMatch = -1;
                    countMissMatch = 0;
                }
                else if (offsetMissMatch == sequenceLength - 1)
                {
                    sequenceLength -= 1;
                    offsetMissMatch = -1;
                    countMissMatch = 0;
                }


                // to je proto ze dva znaky je delsi overhead kdyz je pouzite 
                // slovo s jednim chybejicim znakem
                int encodedPosition = Encode.GetEncodePosition_Size( currWindowSize - (i+sequenceLength));
                int shorted = sequenceLength - encodedPosition;
                if (countMissMatch > 0) shorted -= 2;


                if (((sequenceLength > 5 && countMissMatch > 0) || (sequenceLength > 3 && countMissMatch == 0))
                    && (shorted > maxShort
                    || (shorted == maxShort
                    //&& countMissMatch == 0
                    )
                    ))
                {
                    maxSequenceLenght = sequenceLength;
                    bestOffsetMissMatch = offsetMissMatch;
                    maxShort = shorted;
                    sequenceOffset = i;
                }

                int Distance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                hashMatchIndex = (Distance > 0) ? hashMatchIndex + Distance : -1;
            }

            if (maxSequenceLenght > 3)
            {

                resultMatch.Set(sequenceOffset, (short)maxSequenceLenght, (short)bestOffsetMissMatch, 0,0, MatchResult.MatchType.LABXSeqRedundant);
                return true;
            }

            //resultMatch = new MatchResult();
            return false;
        }

        
        /// <summary>
        ///  v nalezenem slove je je jeden znak navic
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="lookAheadBuffer"></param>
        /// <param name="maxLenMatch"></param>
        /// <param name="resultMatch"></param>
        /// <returns></returns>
        public bool FindMatch_lbXMissing_Faster(SlidingWindow sw, SlidingWindow lookAheadBuffer, HashSSlideWindow hashSW, int maxLenMatch, ref MatchResult resultMatch)
        {
            //return false;
            int maxSequenceLenght = 0;
            int maxShort = 0;
            int sequenceOffset = 0;
            int bestOffsetMissMatch = -1;

            int currWindowSize = sw.GetCurrWindowSize();
            int lookAheadBufferSize = lookAheadBuffer.GetCurrWindowSize();

            // vypocet max bezpecne lookAheadBufferIterace
            lookAheadBufferSize = (lookAheadBufferSize > maxLenMatch) ? maxLenMatch : lookAheadBufferSize;

            int hash = hashSW.ComputeHash(lookAheadBuffer, 0);

            int hashMatchIndex = (hash < 0) ? -1 : hashSW.Get_StartMatchIndex(hash);


            
                while (hashMatchIndex >= 0)
            {
                int i = hashMatchIndex;
                
                int sequenceLength = 0;
                int countMissMatch = 0;
                int offsetMissMatch = -1;

                int cwIndex = i;

                //int lenghtToEnd = currWindowSize - i;

                //int lookAheadBufferMax = (lenghtToEnd < lookAheadBufferSize) ? lenghtToEnd : lookAheadBufferSize;

                int tmpDiff = (sw.GetWindowByte(i) ^ lookAheadBuffer.GetWindowByte(0))
             | (sw.GetWindowByte(i + 1) ^ lookAheadBuffer.GetWindowByte(1))
             | (sw.GetWindowByte(i + 2) ^ lookAheadBuffer.GetWindowByte(2));

                if (tmpDiff == 0)
                {
                    sequenceLength += 3;
                    cwIndex += 3;
                    // najde nejdelsi sekvenci
                    for (int m = 3; m < lookAheadBufferSize; cwIndex++)
                    {

                        if (cwIndex < currWindowSize //&& sequenceLength <= maxLenMatch
                            )
                        {
                            int diff = sw.GetWindowByte(cwIndex) - lookAheadBuffer.GetWindowByte(m);

                            if (diff != 0 && countMissMatch < 1)
                            {
                                countMissMatch++;
                                offsetMissMatch = m;
                                //sequenceLength++;

                                if (m < 1) break;
                            }
                            else if (diff == 0)
                            {
                                sequenceLength++;

                                m++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }


                    }
                }

                if (offsetMissMatch == 0)
                {
                    int tmpDistance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                    hashMatchIndex = (tmpDistance > 0) ? hashMatchIndex + tmpDistance : -1;
                    continue;
                }


                else if (offsetMissMatch == sequenceLength)
                {
                    offsetMissMatch = -1;
                    countMissMatch = 0;
                }


                // to je proto ze dva znaky je delsi overhead kdyz je pouzite 
                // slovo s jednim chybejicim znakem
                int shorted = sequenceLength;
                if (countMissMatch > 0) shorted -= 1;


                if (((sequenceLength > 4 && countMissMatch > 0) || (sequenceLength > 3 && countMissMatch == 0))
                    && (shorted > maxShort
                    || (shorted == maxShort
                    //&& countMissMatch == 0
                    )
                    ))
                {
                    maxSequenceLenght = sequenceLength;
                    bestOffsetMissMatch = offsetMissMatch;
                    maxShort = shorted;
                    sequenceOffset = i;
                }

                int distance = hashSW.Get_NextMatchIndexDistance(hashMatchIndex);
                hashMatchIndex = (distance > 0) ? hashMatchIndex + distance : -1;
            }

            if (maxSequenceLenght > 3)
            {

                resultMatch.Set(sequenceOffset, (short)maxSequenceLenght, (short)bestOffsetMissMatch, 0,0, MatchResult.MatchType.LABXSeqMissing);
                return true;
            }

            //resultMatch = new MatchResult();
            return false;
        }


       

       
    }
}

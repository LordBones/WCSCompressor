using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using WCSCompress.Core.CPCompressor.Helpers;
using WCSCompress.Core.CPCompressor.VO;

namespace WCSCompress.Core.CPCompressor.Modules
{
    public class ContextPredictorModuleTrie : IPredictorModule
    {
        private Helpers.ContextForBorrow _contextForBorrow ;
        //private Trie<LastCharPredictorLightVO> _lookupContextPredictor = new Trie<LastCharPredictorLightVO>();
        private TrieStructFast<LastCharPredictorLightVO> _lookupContextPredictor = new TrieStructFast<LastCharPredictorLightVO>();

        private Helpers.ContextForBorrow<LastCharPredictorLightVO> _arrayDataForBorrow;
        private int _maxContextLength;
        private int _multiplier;
        private int _offset;
    
        public ContextPredictorModuleTrie(int maxContext, int multiplier, int offset)
        {
            _maxContextLength = maxContext;
            _multiplier = multiplier;
            _offset = offset;
            _contextForBorrow = new Helpers.ContextForBorrow(0, maxContext);
            _arrayDataForBorrow = new ContextForBorrow<LastCharPredictorLightVO>(0, maxContext);
        }

        public CharProbabilityVO PredictByte(SlidingWindow historyContext, int DataIndex)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            int maxContext = Helpers_Context.GetMaxPossibleContextSize(
                historyContextSize, _maxContextLength, _multiplier, _offset);



            CharProbabilityVO result = new CharProbabilityVO();
            bool posiblePredict = true;
            double tmpRatio = 0.0;

            if (maxContext >= 2)
            {
                byte[] context = _contextForBorrow[maxContext];
                Helpers_Context.GetLastXContexBytes(historyContext, context, this._multiplier, _offset);


                LastCharPredictorLightVO[] dataCArray = _arrayDataForBorrow[maxContext];
                _lookupContextPredictor.GetAllPossibleData(context, ref dataCArray);

                for (int i = maxContext; i >= 2; i--)
                    //for (int i = 2 ; i <= maxContext; i++)
                {
                    LastCharPredictorLightVO lcp = dataCArray[i-1];

                    if (lcp != null)
                    {
                        if (!lcp.IsPredictPosible) continue;
                        //if (lcp.predictSuccess.TotalCount < 5) continue;

                        BinaryCounterLight tmpSuccess = lcp.predictSuccess;

                        if (tmpSuccess.TotalCount > 2 )
                        {
                            double ratio = tmpSuccess.OneRatio;
                            //double ratio = 1 - Math.Pow((1 - tmpSuccess.OneRatio), i) ;
                            
                            //result = new CharProbabilityVO() { data = lcp.lastByte, probability = ratio };
                            //break;
                            if (ratio > 0.5)
                            {
                                if (tmpRatio < ratio )
                                {
                                    result.Set(lcp.LastByteIndex, ratio, lcp.lastByte);
                                    tmpRatio = ratio ;
                                    break;
                                }
                            }
                            //break;
                        }
                    }
                    else
                    {
                        // break;
                    }
                }

            }

            //var test = PredictByte2(historyContext);

            //if(!((!result.HasValue &&  !test.HasValue)|| (result.HasValue && test.HasValue && result.Value.data == test.Value.data && result.Value.probability == test.Value.probability)))
            //{
            //    Console.Write("fail");
            //}

            if (!posiblePredict) return result;

            return result;
        }

       


        public void UpdatePredictor(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            int maxContext = Helpers_Context.GetMaxPossibleContextSize(
                historyContextSize, _maxContextLength, _multiplier, _offset);


            if (maxContext >= 2)
            {
                byte[] context = _contextForBorrow[maxContext];
                Helpers_Context.GetLastXContexBytes(historyContext, context, this._multiplier, _offset);


                LastCharPredictorLightVO[] dataCArray = _arrayDataForBorrow[maxContext];
                _lookupContextPredictor.GetAllPossibleData(context, ref dataCArray);

                for (int i = 2 - 1; i < dataCArray.Length; i++)
                {
                    LastCharPredictorLightVO lcp = dataCArray[i];
                    if (lcp != null)
                    {
                        lcp.SetByte((byte)nextData);
                        
                    }
                    else
                    {
                        // implicitne se pouzije jiz vypujceny nejdelsi kontext
                        byte[] tmpContext = context;
                        // pripadne se pujci context mensi
                        if (i + 1 < maxContext)
                        {
                            tmpContext = _contextForBorrow[i + 1];
                            Array.Copy(context, tmpContext, i + 1);
                        }
                        LastCharPredictorLightVO newlcp = new LastCharPredictorLightVO((byte)nextData);
                        newlcp.LastByteIndex = DataIndex;

                        _lookupContextPredictor.Add(tmpContext, newlcp);

                        break;
                    }
                }

            }
        }

        public void UpdatePredictorSuccess(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            int maxContext = Helpers_Context.GetMaxPossibleContextSize(
                historyContextSize, _maxContextLength, _multiplier, _offset);


            if (maxContext >= 2)
            {
                byte[] context = _contextForBorrow[maxContext];
                Helpers_Context.GetLastXContexBytes(historyContext, context, this._multiplier, _offset);


                LastCharPredictorLightVO[] dataCArray = _arrayDataForBorrow[maxContext];
                _lookupContextPredictor.GetAllPossibleData(context, ref dataCArray);

                //for (int i = 2-1 ; i < dataCArray.Length; i++)
                //{
                //    byte[] ccontext = _contextForBorrow[i+1];
                //    Helpers_Context.GetLastXContexBytes(historyContext, ccontext, this._multiplier, _offset);

                //    LastCharPredictorLightVO lcp;
                //    bool res = _lookupContextPredictor.TryGetValue(ccontext, out lcp);
                //    if ((res && dataCArray[i] == null) ||
                //       (!res && dataCArray[i] != null))
                //    {
                //        Console.Write("Fail " + i.ToString());
                //    }
                //}

                for (int i = dataCArray.Length-1;  i > 0 ; i--)

                    //for (int i = 2 - 1; i < dataCArray.Length; i++)
                {
                    LastCharPredictorLightVO lcp = dataCArray[i];
                    if (lcp != null)
                    {
                        //if (lcp.predictSuccess.ZeroInRow >= 5) continue;
                        if (!lcp.IsPredictPosible) continue;

                        if (lcp.lastByte == (byte)nextData)
                        {
                            lcp.predictSuccess.AddOne(1);
                            lcp.predictSuccess.AddOne(1);
                           // break;
                            //lcp.predictSuccess.AddOne(1);

                        }
                        else
                        {
                            lcp.predictSuccess.AddZero(1);
                           
                        }

                        lcp.LastByteIndex = DataIndex;
                    }
                }
            }
        }
    }
}

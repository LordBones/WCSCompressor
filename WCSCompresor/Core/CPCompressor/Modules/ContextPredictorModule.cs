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
    public class ContextPredictorModule : IPredictorModule
    {

        private Helpers.ContextForBorrow _contextForBorrow;
        private Dictionary<byte[], LastCharPredictorLightVO> _lookupContextPredictor = new Dictionary<byte[], LastCharPredictorLightVO>(new ByteArraryComparer());

        private int _maxContextLength;
        private int _multiplier;
        private int _offset;

        public ContextPredictorModule(int maxContext, int multiplier, int offset)
        {
            _maxContextLength = maxContext;
            _multiplier = multiplier;
            _offset = offset;
            _contextForBorrow = new Helpers.ContextForBorrow(0,maxContext);
        }

        public CharProbabilityVO PredictByte(SlidingWindow historyContext,int DataIndex)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            int maxContext = this._maxContextLength;
            if (historyContextSize < this._maxContextLength * this._multiplier)
            {
                maxContext = historyContextSize / this._multiplier;
            }


            CharProbabilityVO result = new CharProbabilityVO();

            bool posiblePredict = true;
            double tmpRatio = 0.0;

            // for (int i = 2; i <= LastCharPreditor_contextLength; i++)
            for (int i = maxContext; i >= 2; i--)
            {
                byte[] context = _contextForBorrow[i];
                Helpers_Context.GetLastXContexBytes(historyContext, context, this._multiplier, _offset);
                //byte[] context = GetLastXContexBytes(historyContext, i,1,0);
                LastCharPredictorLightVO lcp = null;

                if (_lookupContextPredictor.TryGetValue(context, out lcp))
                {

                    if (!lcp.IsPredictPosible) continue;
                    //if (lcp.predictSuccess.TotalCount < 5) continue;

                    BinaryCounterLight tmpSuccess = lcp.predictSuccess;

                    if (tmpSuccess.TotalCount > 2)
                    {
                        double ratio = tmpSuccess.OneRatio;
                        //double ratio = 1 - Math.Pow((1 - tmpSuccess.OneRatio), i) ;
                        //if (result.HasValue)
                        //{
                        //    if(ratio > result.Value.probability)
                        //    {
                        //        CharProbabilityVO cp = result.Value;
                        //        cp.Set(ratio, lcp.lastByte);
                        //        result = cp;
                        //        posiblePredict = lcp.IsPredictPosible;

                        //    }
                        //}
                        //else
                        //{
                        //    result = new CharProbabilityVO() { data = lcp.lastByte, probability = ratio };
                        //    posiblePredict = lcp.IsPredictPosible;
                        //}

                        //result = new CharProbabilityVO() { data = lcp.lastByte, probability = ratio };
                        //break;

                        if (ratio > 0.5)
                        {
                            if (tmpRatio < ratio * i)
                            {
                                result.Set(DataIndex, ratio, lcp.lastByte);
                                tmpRatio = ratio * i;
                            }
                        }
                    }
                }
                else
                {
                    // break;
                }

            }

            if (!posiblePredict) return result;

            return result;
        }


        public void UpdatePredictor(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            int maxContext = this._maxContextLength;
            if (historyContextSize < this._maxContextLength * this._multiplier)
            {
                maxContext = historyContextSize / this._multiplier;
            }

            for (int i = 2; i <= maxContext; i++)
            {

                byte[] context = _contextForBorrow[i];
                Helpers_Context.GetLastXContexBytes(historyContext, context, this._multiplier, _offset);
                //byte[] context = GetLastXContexBytes(historyContext, i,1,0);

                LastCharPredictorLightVO lcp = null;

                if (_lookupContextPredictor.TryGetValue(context, out lcp))
                {
                    lcp.SetByte((byte)nextData);
                    //if (_lookupContextPredictor[context].predictSuccess.TotalCount == 0) break;
                }
                else
                {
                    // if (_lookupContextPredictor.Count > 16000) _lookupContextPredictor.Clear();

                    byte[] tmp = new byte[context.Length];
                    Buffer.BlockCopy(context, 0, tmp, 0, context.Length);
                    _lookupContextPredictor.Add(tmp, new LastCharPredictorLightVO((byte)nextData));
                    
                    break;
                }


            }
        }

        public void UpdatePredictorSuccess(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            int maxContext = this._maxContextLength;
            if (historyContextSize < this._maxContextLength * this._multiplier)
            {
                maxContext = historyContextSize / this._multiplier;
            }

            for (int i = 2; i <= maxContext; i++)
            {

                byte[] context = _contextForBorrow[i];
                Helpers_Context.GetLastXContexBytes(historyContext, context, this._multiplier, _offset);
                //byte[] context = GetLastXContexBytes(historyContext, i,1,0);

                LastCharPredictorLightVO lcp = null;

                if (_lookupContextPredictor.TryGetValue(context, out lcp))
                {
                    if (!lcp.IsPredictPosible) continue;

                    if (lcp.lastByte == (byte)nextData)
                    {
                        lcp.predictSuccess.AddOne(1);
                        lcp.predictSuccess.AddOne(1);
                        lcp.predictSuccess.AddOne(1);
                    }
                    else
                    {
                        lcp.predictSuccess.AddZero(1);
                    }
                }

            }
        }
    }
}

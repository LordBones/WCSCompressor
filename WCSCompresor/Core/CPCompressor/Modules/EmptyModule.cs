using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using WCSCompress.Core.CPCompressor.VO;

namespace WCSCompress.Core.CPCompressor.Modules
{
    internal class EmptyModule : IPredictorModule
    {
        private byte _lastChar;
        private BinaryCounterLight _predictorSuccess;
        private bool _CanPredict;

        public EmptyModule()
        {
            _predictorSuccess = new BinaryCounterLight();
        }

        public CharProbabilityVO PredictByte(SlidingWindow historyContext, int DataIndex)
        {
           

            CharProbabilityVO result = new CharProbabilityVO();

            if (!_CanPredict) return result;
            //int historyContextSize = historyContext.GetCurrWindowSize();

            //if (historyContextSize <= 0) return result;

            // CharProbabilityVO? result = null;


            result.Set(DataIndex, _predictorSuccess.OneRatio, _lastChar);
            
            return result;
           
        }

        public void UpdatePredictor(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            _CanPredict = this._lastChar == nextData;

            this._lastChar = nextData;
        }

        public void UpdatePredictorSuccess(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            if (_CanPredict)
            {
                if ( this._lastChar == nextData)
                {
                    this._predictorSuccess.AddOne(1);
                }
                else
                {
                    this._predictorSuccess.AddZero(1);
                }
            }
           
        }
    }
}

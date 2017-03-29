using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using WCSCompress.Core.CPCompressor.VO;

namespace WCSCompress.Core.CPCompressor.Modules
{
    class Order1_ByteCounterModule : IPredictorModule
    {
        private CharCountsOrder[] _dataCounter;
        public BinaryCounterLight[] _predictSuccess;

        public Order1_ByteCounterModule()
        {
            this._dataCounter = new CharCountsOrder[256];
            this._predictSuccess = new BinaryCounterLight[256];
            for(int i = 0;i<this._dataCounter.Length;i++)
            {
                this._dataCounter[i] = new CharCountsOrder();
                this._predictSuccess[i] = new BinaryCounterLight();
            }
        }

        public CharProbabilityVO PredictByte(SlidingWindow historyContext, int DataIndex)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            CharProbabilityVO result = new CharProbabilityVO();

            if (historyContextSize < 1)
            {
                return result;
            }


            byte context = historyContext.GetWindowLastByte();

            result.Set(DataIndex, this._predictSuccess[context].OneRatio, this._dataCounter[context].Stack[0]);
            
            return result;
        }

        public void UpdatePredictor(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            if (historyContextSize < 1)
            {
                return ;
            }

            byte context = historyContext.GetWindowLastByte();
            this._dataCounter[context].Add_Char(nextData);
        }

        public void UpdatePredictorSuccess(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            if (historyContextSize < 1)
            {
                return;
            }

            byte context = historyContext.GetWindowLastByte();

            if(this._dataCounter[context].Stack[0] == nextData)
            {
                this._predictSuccess[context].AddOne(1);
                
            }
            else
            {
                this._predictSuccess[context].AddZero(1);
            }

        }
    }
}

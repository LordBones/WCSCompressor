using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using WCSCompress.Core.CPCompressor.VO;

namespace WCSCompress.Core.CPCompressor.Modules
{
    class CircleContextPredictModule : IPredictorModule
    {
        private SlidingWindow _circleContext = null;
        private BinaryCounterLight _successPredict = new BinaryCounterLight();
        private int _contextSize = 0;

        public CircleContextPredictModule(int contextCircleSize)
        {
            _circleContext = new SlidingWindow(contextCircleSize*2);
            _circleContext.AddByte(0);
            _contextSize = contextCircleSize;
        }

        public CharProbabilityVO PredictByte(SlidingWindow historyContext, int DataIndex)
        {
            CharProbabilityVO result = new CharProbabilityVO();

            if (_contextSize*2 > this._circleContext.GetCurrWindowSize())
            {
                return result;
            }

            if (!IsPredictPossible()) return result;

            if (_successPredict.TotalCount > 2)
            {
                double ratio = _successPredict.OneRatio;
                result.Set(DataIndex, ratio, _circleContext.GetWindowFirstByte());
                return result;
            }

            return result;
        }

        public void UpdatePredictor(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            _circleContext.AddByte(nextData);

            
        }

        public void UpdatePredictorSuccess(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            if (_contextSize * 2 > this._circleContext.GetCurrWindowSize())
            {
                return ;
            }

            if (!IsPredictPossible())
            {
                //this._successPredict.AddOne(1);
                return;
            }

            

            if (nextData == _circleContext.GetWindowFirstByte())
            {
                this._successPredict.AddOne(1);
            }
            else
            {
                this._successPredict.AddZero(1);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsPredictPossible()
        {
            if (this._circleContext.GetWindowByte(0) != this._circleContext.GetWindowByte(_contextSize)
                || this._circleContext.GetWindowByte(1) != this._circleContext.GetWindowByte(_contextSize+1)
               ) return false;

            return true;
        }
    }
}

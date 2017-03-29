using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using WCSCompress.Core.CPCompressor.VO;

namespace WCSCompress.Core.CPCompressor
{
    interface IPredictorModule
    {
        CharProbabilityVO PredictByte(SlidingWindow historyContext, int DataIndex);

        void UpdatePredictorSuccess(SlidingWindow historyContext, int DataIndex, byte nextData);

        void UpdatePredictor(SlidingWindow historyContext, int DataIndex, byte nextData);

    }
}

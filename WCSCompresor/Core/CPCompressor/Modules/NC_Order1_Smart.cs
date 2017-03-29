using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using WCSCompress.Core.CPCompressor.VO;

namespace WCSCompress.Core.CPCompressor.Modules
{
    class NC_Order1_Smart : IPredictorModule
    {
        BinaryCounterLight[] _predictors;

        private int _maxContextLenght;

        public NC_Order1_Smart(int contextLength)
        {
            this._maxContextLenght = contextLength;
            _predictors = new BinaryCounterLight[contextLength];
            for(int i  = 0;i < _predictors.Length;i++)
            {
                _predictors[i] = new BinaryCounterLight();
            }
        }

        public CharProbabilityVO PredictByte(SlidingWindow historyContext, int DataIndex)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            CharProbabilityVO result = new CharProbabilityVO();

            if (historyContextSize < this._maxContextLenght)
            {
                return result;
            }

            int hcIndexStart = FindMinWindowContextIndex_WhenStartPredict(historyContext, DataIndex);
            if(hcIndexStart < 0)
            {
                return result;
            }

            byte predictByte = historyContext.GetWindowByte(hcIndexStart + 1);

            int hcIndexEnd = FindMaxWindowContextIndex_WhenEndPredict(historyContext, DataIndex, hcIndexStart);

            double bestRatio = 0.0;
            int bestIndex = -1;
            for (int i = 0; i < _predictors.Length; i++)
            {
                if (bestRatio < _predictors[i].OneRatio)
                {
                    bestRatio = _predictors[i].OneRatio;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0) return result;

            bestIndex = historyContextSize - bestIndex - 1;
            if (!(bestIndex <= hcIndexStart && bestIndex >= hcIndexEnd)) return result;

            /*
            // find predictor with best probability
            for(int i = hcIndexEnd; i <= hcIndexStart;i++)
            {
                int index = historyContextSize - i - 1;

                if(bestRatio < _predictors[index].OneRatio)
                {
                    bestRatio = _predictors[index].OneRatio;
                }
            }
            */

            result.Set(DataIndex,bestRatio, predictByte);
            return result;
        }

        private int FindMinWindowContextIndex_WhenStartPredict(SlidingWindow historyContext, int DataIndex)
        {
            int hcSize = historyContext.GetCurrWindowSize();
            int startIndex = hcSize - 1;

            byte byteForMatch = historyContext.GetWindowByte(startIndex);

            startIndex--;
            /*
            // najdi prvni vyskytujicise kontextovy byte
            while (startIndex >= 0 && (hcSize-startIndex) <= this._maxContextLenght)
            {
                if (historyContext.GetWindowByte(startIndex) == byteForMatch)
                {
                    return startIndex;
                }

                startIndex--;
            }*/

            // najdi prvni vyskytujicise kontextovy byte
            while (startIndex >= 0 && (hcSize - startIndex) <= this._maxContextLenght)
            {
                if (historyContext.GetWindowByte(startIndex) == byteForMatch)
                {
                    startIndex--;
                    break;
                }

                startIndex--;
            }

            //// najdi prvni vyskytujicise kontextovy byte
            //while (startIndex >= 0 && (hcSize - startIndex) <= this._maxContextLenght)
            //{
            //    if (historyContext.GetWindowByte(startIndex) == byteForMatch)
            //    {
            //        return startIndex;
            //    }

            //    startIndex--;
            //}

            // -1 zadny predictor se neucastni predikce

            return -1;
        }

        private int FindMaxWindowContextIndex_WhenEndPredict(SlidingWindow historyContext, int DataIndex, int startIndex)
        {
            int hcSize = historyContext.GetCurrWindowSize();
            byte byteContextForMatch = historyContext.GetWindowByte(startIndex);
            byte bytePredict = historyContext.GetWindowByte(startIndex+1);

            startIndex--;
            // -1 predictor s danou velikosti kontextu se neucastni predikce nasledujiciho znaku
            
            // najdi prvni vyskytujicise kontextovy byte
            while(startIndex >= 0 && (hcSize - startIndex) <= this._maxContextLenght)
            {
                if(historyContext.GetWindowByte(startIndex) == byteContextForMatch)
                {
                    if(historyContext.GetWindowByte(startIndex+1) != bytePredict )
                    {
                        return startIndex + 1;
                    }
                }

                startIndex--;
            }

            return hcSize- this._maxContextLenght;
        }

        public void UpdatePredictor(SlidingWindow historyContext, int DataInde, byte nextData)
        {
            // tato metoda vubec neni potreba pro algoritmus
        }

        public void UpdatePredictorSuccess(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            int historyContextSize = historyContext.GetCurrWindowSize();

            if (historyContextSize < this._maxContextLenght)
            {
                return ;
            }

            int hcIndexStart = FindMinWindowContextIndex_WhenStartPredict(historyContext, DataIndex);
            if (hcIndexStart < 0)
            {
                return ;
            }

            byte predictByte = historyContext.GetWindowByte(hcIndexStart + 1);

            int hcIndexEnd = FindMaxWindowContextIndex_WhenEndPredict(historyContext, DataIndex, hcIndexStart);

            if (predictByte == nextData)
            {
                for (int i = hcIndexEnd; i <= hcIndexStart; i++)
                {
                    int index = historyContextSize - i - 1;
                    _predictors[index].AddOne(1);
                }
            }
            else
            {
                for (int i = hcIndexEnd; i <= hcIndexStart; i++)
                {
                    int index = historyContextSize - i - 1;
                    _predictors[index].AddZero(1);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core.CPCompressor.VO
{
    public struct CharProbabilityVO
    {
        public bool IsPrediction;
        public double probability;
        public byte data;
        public int LastDataIndex;

        public void Set(double probability)
        {
            this.probability = probability;
        }

        public void Set(int lastDataIndex, double probability, byte data)
        {
            this.probability = probability;
            this.data = data;
            this.IsPrediction = true;
            this.LastDataIndex = lastDataIndex;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core.CPCompressor.VO
{
    class LastCharPredictorLightVO
    {
        public BinaryCounterLight predictSuccess = new BinaryCounterLight();
        public int LastByteIndex;
        public byte lastByte;
        public byte lastByteCount = 0;
        public byte miss = 0;
        public bool miss2;
        
        const byte CONST_MaxLastByteCount = 1;

        public LastCharPredictorLightVO()
        {

        }

        //public bool IsPredictPosible => lastByteCount >= 1;
        public bool IsPredictPosible => true;// miss2;

        public LastCharPredictorLightVO(byte lastByte)
        {
            this.lastByte = lastByte;
            this.lastByteCount = 1;
        }

        public void SetByte(byte newByte)
        {
            if(!miss2)
            {
                if(lastByte != newByte)
                {
                    miss2 = true;
                }

                return;
            }

            miss2 = lastByte != newByte; 
            lastByte = newByte;
        }

        /*public void SetByte(byte newByte)
        {
            lastByte = newByte;
        }*/

        public void SetByte2(byte newByte)
        {
            if (newByte == lastByte)
            {
                if (lastByteCount < CONST_MaxLastByteCount)
                    lastByteCount++;

                miss = 0;
            }
            else
            {
                if(miss == 0)
                {
                    lastByte = newByte;
                    lastByteCount = 1;
                }
                else
                {
                    miss = 1;
                }

                /*if (lastByteCount > CONST_MaxLastByteCount-1)
                {
                    lastByteCount--;
                }
                else
                {
                    lastByte = newByte;
                    lastByteCount = 1;// CONST_MaxLastByteCount;
                }*/

                //
            }
        }
    }
}

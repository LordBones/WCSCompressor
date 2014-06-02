using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core
{
    internal class WCSCompressor
    {
        public int StatCharRemove = 0;
        public int StatCharAdd = 0;

        public void Coding(Stream input, Stream output, int sizeSlidingWindow)
        {
            BufferedStream bfInput = new BufferedStream(input);

            SlidingWindow sw = new SlidingWindow(sizeSlidingWindow);
            LookupPredictor lp = new LookupPredictor();
            CharCountsOrder ccoGlobal = new CharCountsOrder();
            CharCountsOrder [] cco = new CharCountsOrder[256];
            for (int i = 0; i < cco.Length; i++)
                cco[i] = new CharCountsOrder();

            int data = bfInput.ReadByte();

            bool wasLastCharRemove = false;

            while (data >= 0)
            {
                byte dataB = (byte)data;

                if (sw.GetCurrWindowSize() > 1)
                {
                    if (wasLastCharRemove)
                    {
                        output.WriteByte(dataB);

                        wasLastCharRemove = false;
                    }
                     //has one char most count of others, but next char is not it
                    else if (lp.DataFollowAncestorTotalCount(sw.GetWindowLastByte()) > 8 &&
                        lp.IsDataMostCountContraOthers(sw.GetWindowLastByte()) &&
                        !lp.IsDataMostCountContraOthers(sw.GetWindowLastByte(), dataB))
                    {
                        CharCountsOrder tmpCCO = cco[sw.GetWindowLastByte()];
                        if (tmpCCO.Celk_Cetnost == 0)
                            tmpCCO = ccoGlobal;

                        byte tmp = GetCharNotInWindow(lp, tmpCCO);
                        output.WriteByte(tmp);
                        output.WriteByte(dataB);

                        StatCharAdd++;
                    }
                    else if (!lp.IsDataFollowAncestorOnly(sw.GetWindowLastByte(), dataB)
                          && !(lp.DataFollowAncestorTotalCount(sw.GetWindowLastByte()) > 8 &&
                        lp.IsDataMostCountContraOthers(sw.GetWindowLastByte()))
                         )
                    {
                        if ((lp.IsDataFollowAncestorOnlyFirstBreak(sw.GetWindowLastByte(), dataB) 
                             
                            )&&
                            lp.DataFollowAncestorTotalCount(sw.GetWindowLastByte()) > 1 
                            )
                        {
                            // write synchron byte which not exist in last context
                            //ms.WriteByte((byte)'#');

                            //byte tmp = lp.GetNotUseInWindowByte();

                            CharCountsOrder tmpCCO = cco[sw.GetWindowLastByte()];
                            if (tmpCCO.Celk_Cetnost == 0)
                                tmpCCO = ccoGlobal;

                            byte tmp = GetCharNotInWindow(lp, tmpCCO);
                            output.WriteByte(tmp);

                            StatCharAdd++;
                        }

                        output.WriteByte(dataB);

                    }
                    else
                    {
                        wasLastCharRemove = true;
                        StatCharRemove++;
                    }
                }
                else 
                {
                    output.WriteByte(dataB);
                }


                ccoGlobal.Add_Char(dataB);

                if (sw.GetCurrWindowSize() > 1)
                    cco[sw.GetWindowLastByte()].Add_Char(dataB);


                UpdateLookupTable(lp, sw, dataB);
                sw.AddByte(dataB);



                data = bfInput.ReadByte();
            }
        }

        private byte GetCharNotInWindow(LookupPredictor lp, CharCountsOrder cco)
        {
            for(int i = 0;i < 256;i++)
            {
                if(!lp.IsByteUsed( cco.Stack[i]))
                {
                    return (byte)cco.Stack[i];
                }
            }

            throw new Exception("Every time must exist char not in use");
        }

        private void UpdateLookupTable(LookupPredictor lp, SlidingWindow sw, byte dataByte)
        {
            // update lookup
            if (sw.GetCurrWindowSize() > 0)
                lp.AddByte(sw.GetWindowLastByte(), dataByte);

            if (sw.GetCurrWindowSize() > 1)
            {
                if (sw.IsMaxSizeWindow())
                    lp.RemoveByte(sw.GetWindowFirstByte(), sw.GetWindowByte(1));
            }
        }

       

        
    }
}

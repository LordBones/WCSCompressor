
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.Basic;

namespace WCSCompress.MCR
{
    /// <summary>
    /// most count remover
    /// </summary>
    public class MCR
    {
        public void Compress(Stream input, Stream output, int windowSize)
        {
            const int CONST_CountTest = 32;
            byte[] buffer = new byte[windowSize+ CONST_CountTest-1];
            int bufferCount = 0;

            int countReaded = 0;

            MCRCore mcr = new MCRCore();

            do
            {
                countReaded = input.Read(buffer, bufferCount, buffer.Length-bufferCount);
                bufferCount += countReaded;

                if (bufferCount < buffer.Length)
                {
                    output.Write(buffer, 0, countReaded);
                    bufferCount = -1;
                    
                }
                else
                {
                    int blockIndex = CheckBestBlockPosition(new ArraySegmentEx_Byte(buffer, 0, bufferCount), windowSize, CONST_CountTest);

                    if(blockIndex < 0)
                    {
                        output.WriteByte(CONST_CountTest);
                        output.Write(buffer,0, CONST_CountTest);

                        // skopiruje a posune data
                        int countForMove = bufferCount - (CONST_CountTest);

                        for (int i = 0; i < countForMove; i++)
                        {
                            buffer[i] = buffer[CONST_CountTest + i];
                        }

                        bufferCount = countForMove;
                    }
                    else
                    {
                        // zapiseme pocet nezabalitelnych znaku
                        output.WriteByte((byte)(blockIndex));
                        output.Write(buffer, 0, blockIndex);

                        // ziskame zabalenou cast
                        byte[] tmpResult = mcr.Compress(new ArraySegmentEx_Byte(buffer, blockIndex, windowSize));
                        output.Write(tmpResult, 0, tmpResult.Length);

                        // skopirujeme koncove znaky z bufferu jsou li nejake na pocatek
                        int countForMove =  bufferCount - (blockIndex + windowSize);
                        for(int i = 0;i< countForMove;i++)
                        {
                            buffer[i] = buffer[blockIndex + windowSize + i];
                        }

                        bufferCount = countForMove;
                    }
                }

            } while (bufferCount >= 0);
        }

        private int CheckBestBlockPosition(ArraySegmentEx_Byte data, int blockSize, int maxAttemtps)
        {
            MCRCore mcr = new MCRCore();
            int bestIndex = -1;
            int bestCompress = 0;

            for (int i = 0; i < (data.Count - blockSize+1) && i < maxAttemtps; i++)
            {
                byte[] result = mcr.Compress(data.CreateSegment(i, blockSize));

                int sizeDiff = blockSize - result.Length;

                if (sizeDiff > 0 && sizeDiff > bestCompress)
                {
                    bestIndex = i;
                    bestCompress = sizeDiff;
                }
            }

            return bestIndex;
        }
    }

   

    public class MCRCore
    {
        public byte [] Compress(ArraySegmentEx_Byte  data)
        {
            //
            int countMCByte; 
            byte mostCountByte = GetMostCount(data,out countMCByte);

            int lengthHeader = (data.Count >> 3) + (((data.Count & 7) > 0) ? 1 : 0);

            byte[] header = new byte[lengthHeader];
            byte[] body = new byte[data.Count - countMCByte];

            Encode(mostCountByte, data, header, body);


            /// vypis vysledku
            using (MemoryStream ms = new MemoryStream())
            {
                if (countMCByte <= lengthHeader+1)
                {
                    ms.Write(data.Array, data.Offset, data.Count);
                }
                else
                {

                    ms.WriteByte(mostCountByte);
                    ms.Write(header, 0, header.Length);
                    ms.Write(body, 0, body.Length);
                }

                return ms.ToArray();
            }
        }


        private void Encode(byte encByte, ArraySegmentEx_Byte data, byte [] header, byte [] body)
        {
            int indexBody = 0;

            for(int i = 0;i<data.Count;i++)
            {
                if(data[i] == encByte)
                {
                    int index = i >> 3;
                    byte byteForOr = (byte)(1 << (i & 7));

                    header[index] |= byteForOr;
                }
                else
                {
                    body[indexBody] = data[i];
                    indexBody++;
                }
            }
        }

        private byte GetMostCount(ArraySegmentEx_Byte data, out int countMCByte)
        {
            int[] lookup = new int[256];

            for(int i = 0; i < data.Count;i++)
            {
                lookup[data[i]]++;
            }

            byte bestByte = 0;
            int countBestByte = 0;
            for(int i = 0;i< lookup.Length;i++)
            {
                if(countBestByte < lookup[i])
                {
                    bestByte = (byte)i;
                    countBestByte = lookup[i];
                }
            }

            countMCByte = countBestByte;
            return bestByte;
        }
    }
}

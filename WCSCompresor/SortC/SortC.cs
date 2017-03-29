
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.Basic;

namespace WCSCompress.SortC
{
    public class SortC
    {
        public void Compress(Stream input, Stream output, int windowSize)
        {

            byte[] buffer = new byte[windowSize];
            int bufferCount = 0;

            int countReaded = 0;

            SortC_Core sortCc = new SortC_Core();
            int[] shiftMatrix = null;
            int countMatrixApply = 0;

            do
            {
                countReaded = input.Read(buffer, bufferCount, buffer.Length - bufferCount);
                bufferCount += countReaded;

                if (bufferCount < buffer.Length)
                {
                    output.Write(buffer, 0, bufferCount);
                    bufferCount = -1;
                }
                else
                {
                    if(shiftMatrix == null  || countMatrixApply > 10)
                    {
                        shiftMatrix = sortCc.ComputeShiftMatrix(new ArraySegmentEx_Byte(buffer, 0, bufferCount));
                        output.Write(buffer, 0, bufferCount);
                        bufferCount = 0;
                        countMatrixApply = 0;
                    }
                    else
                    {
                        byte[] result = new byte[bufferCount];
                        sortCc.ApplyShiftMatrix(new ArraySegmentEx_Byte(buffer, 0, bufferCount),shiftMatrix,ref result);
                        output.Write(result, 0, result.Length);
                        bufferCount = 0;
                        countMatrixApply++;
                    }

                    //byte[] result = sortCc.Compress(new ArraySegmentEx_Byte(buffer, 0, bufferCount));
                    //output.Write(result, 0, result.Length);

                    //bufferCount = 0;
                }

            } while (bufferCount >= 0);
        }
    }
    public class SortC_Core
    {
        public byte[] Compress(ArraySegmentEx_Byte data)
        {

            byte[] tmpData = new byte[data.Count];

            Buffer.BlockCopy(data.Array, data.Offset, tmpData, 0, data.Count);


            Array.Sort(tmpData);

            return tmpData;

        }

        public int [] ComputeShiftMatrix(ArraySegmentEx_Byte data)
        {
            tmpSortMatrixItem[] tmpTransorm = new tmpSortMatrixItem[data.Count];

            for(int i = 0;i<data.Count;i++)
            {
                tmpTransorm[i] = new tmpSortMatrixItem() { data = data[i], index = i };
            }

            Array.Sort(tmpTransorm);

            int [] shiftMatrix = new int[data.Count];

            for(int i = 0;i< tmpTransorm.Length;i++)
            {
                shiftMatrix[i] = tmpTransorm[i].index;
            }

            return shiftMatrix;
        }

        public void  ApplyShiftMatrix(ArraySegmentEx_Byte data, int [] shiftMatrix, ref byte [] result)
        {
            if (!(data.Count == shiftMatrix.Length && shiftMatrix.Length == result.Length)) throw new Exception("fail");
            
            for (int i = 0; i < shiftMatrix.Length; i++)
            {
                result[i] = data[shiftMatrix[i]];
            }
        }

        struct tmpSortMatrixItem : IComparable<tmpSortMatrixItem>
        {
            public byte data;
            public int index;

            public int CompareTo(tmpSortMatrixItem other)
            {
                return this.data - other.data;
            }
        }
    }
}

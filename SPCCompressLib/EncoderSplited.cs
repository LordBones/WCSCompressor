
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.Basic;

namespace SPCCompressLib
{
    internal class EncoderSplited
    {
        private const int CONST_CodeSplitByte = 0;
        private const int CONST_LengthCodeSplitByte = 1;


        private byte _splitBy;
        private int _maxNotMatchLenght;

        private int _splitByteCountInRow;

        private List<byte> _outCodesPart;
        private List<byte> _outNotCodedPart;

        private List<byte> _output;

        public byte[] Output => _output.ToArray();

        public int CountIndexEncodedAsOneByte => Helper_FirstByte_MaxIndexEncodeMatch() + 1;

        public EncoderSplited(byte splitBy, int maxNotMatchLenght, int reserveLenght)
        {
            if (maxNotMatchLenght > 32) maxNotMatchLenght = 32;

            this._output = new List<byte>(reserveLenght);
            this._outCodesPart = new List<byte>(reserveLenght);
            this._outNotCodedPart = new List<byte>(reserveLenght);
            this._splitBy = splitBy;
            this._output.Add(splitBy);
            this._output.Add((byte)maxNotMatchLenght);
            this._maxNotMatchLenght = maxNotMatchLenght;
            this._splitByteCountInRow = 0;
        }

        public void EncodeMatchToken(int matchIndex, ArraySegmentEx_Byte word)
        {
            if (this._splitByteCountInRow > 0)
            {
                EncodeTokenZeroInRow();
                this._splitByteCountInRow = 0;
            }

            int maxIndexEncode = Helper_FirstByte_MaxIndexEncodeMatch();
            int startFirstByte = Helper_FirstByte_StartEncodeMatch();

            if (matchIndex > maxIndexEncode + 255)
            {
                _outCodesPart.Add((byte)(startFirstByte + maxIndexEncode + 2));
                int encodeIndex = matchIndex - maxIndexEncode-255;
                _outCodesPart.Add((byte)(encodeIndex >> 8));
                _outCodesPart.Add((byte)(encodeIndex & 0xff));
            }
            else if (matchIndex > maxIndexEncode)
            {
                _outCodesPart.Add((byte)(startFirstByte + maxIndexEncode + 1));
                _outCodesPart.Add((byte)((matchIndex - maxIndexEncode) & 0xff));
            }

            else
            {
                _outCodesPart.Add((byte)(startFirstByte + matchIndex ));
            }
        }

        public void EncodeNotMatchToken(ArraySegmentEx_Byte word)
        {
            if (this._splitByteCountInRow > 0)
            {
                EncodeTokenZeroInRow();
                this._splitByteCountInRow = 0;
            }

            int lenght = word.Count;
            if (lenght > _maxNotMatchLenght)
            {
                _outCodesPart.Add(255);

                for(int i =0;i< word.Count;i++)
                _outNotCodedPart.Add(word[i]);

                //_outNotCodedPart.AddRange(word);

                _outNotCodedPart.Add(_splitBy);

                //result.Add((byte)(lenght >> 8));
                //result.Add((byte)(lenght & 0xff));
            }
            else
            {
                _outCodesPart.Add((byte)(Helper_StartByte_EncodeNotMatch() + word.Count));

                for (int i = 0; i < word.Count; i++)
                    _outNotCodedPart.Add(word[ i]);

                //_outNotCodedPart.AddRange(word);
            }
        }

        public void EncodeTokenZeroLenght()
        {
            if(this._splitByteCountInRow == 257)
            {
                EncodeTokenZeroInRow();
                this._splitByteCountInRow = 0;
            }

            this._splitByteCountInRow++;
            //_outCodesPart.Add(0);
        }

        private void EncodeTokenZeroInRow()
        {
            if(this._splitByteCountInRow == 0)
            {
                throw new NotSupportedException();
            }
            else if(this._splitByteCountInRow == 1)
            {
                _outCodesPart.Add(0);
            }
            else if (this._splitByteCountInRow == 2)
            {
                _outCodesPart.Add(1);
            }
            else
            {
                _outCodesPart.Add(2);
                _outCodesPart.Add((byte)(this._splitByteCountInRow - 3));
            }
        }

        public void EncodeFinish( int recursiveLevel)
        {
            if (this._splitByteCountInRow > 0)
            {
                EncodeTokenZeroInRow();
            }

            if (this._outCodesPart.Count > 65535)
            {
                throw new Exception("Block code is longer then 65535");
            }


            if(recursiveLevel <8)
            {
                SPCCompress spc = new SPCCompress();
                byte [] output = spc.EncodeBlock(this._outNotCodedPart.ToArray(),recursiveLevel+1);

                this._output.Add((byte)'X');
                this._output.Add((byte)'X');
                this._output.Add((byte)'X');
                this._output.Add((byte)((this._outCodesPart.Count >> 8) & 0xff));
                this._output.Add((byte)((this._outCodesPart.Count) & 0xff));

                this._output.AddRange(this._outCodesPart);
                this._output.Add((byte)'#');
                this._output.Add((byte)'#');
                this._output.Add((byte)'#');

                this._output.AddRange(output);

            }
            else
            {
                this._output.Add((byte)((this._outCodesPart.Count >> 8) & 0xff));
                this._output.Add((byte)((this._outCodesPart.Count) & 0xff));

                this._output.AddRange(this._outCodesPart);
                this._output.AddRange(this._outNotCodedPart);

            }



            //for (int i = 0; i < this._outCodesPart.Count; i++)
            //{
            //    this._output.Add(this._outCodesPart[i]);
            //}
            //for (int i = 0; i < this._outNotCodedPart.Count; i++)
            //{
            //    this._output.Add(this._outNotCodedPart[i]);
            //}

        }

        public void Clear()
        {
            
        }

        public int GetMatchEncodedLenght(int matchIndex)
        {
            int maxIndexEncode = Helper_FirstByte_MaxIndexEncodeMatch();

            if (matchIndex > maxIndexEncode + 255) return 3;
            else if (matchIndex > maxIndexEncode) return 2;
            else return 1;
        }

        private int Helper_FirstByte_StartEncodeMatch()
        {
            return CONST_LengthCodeSplitByte+2;
        }

        private int Helper_FirstByte_MaxIndexEncodeMatch()
        {
            return Helper_StartByte_EncodeNotMatch() - Helper_FirstByte_StartEncodeMatch() - 2;
        }

       

        // koduje i delku 0 ikdyz je to zbytecne
        private int Helper_StartByte_EncodeNotMatch()
        {
            /// jeden byte je rezerva, pro zakodovani vetsi velikosti indexu
            int result = 255 - this._maxNotMatchLenght - 1;
            return result;
        }
    }
}

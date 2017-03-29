using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.Basic;

namespace SPCCompressLib
{
    internal class Encoder
    {
        private byte _splitBy;
        private List<byte> _output = new List<byte>();

        public byte[] Output => _output.ToArray();

        public Encoder(byte splitBy)
        {
            this._splitBy = splitBy;
            this._output.Add(splitBy);
        }

        public void EncodeMatchToken(int matchIndex, ArraySegmentEx_Byte word)
        {
            if (matchIndex > 124 + 255)
            {
                _output.Add(127);
                _output.Add((byte)(matchIndex >> 8));
                _output.Add((byte)(matchIndex & 0xff));
            }
            else if (matchIndex > 124)
            {
                _output.Add(126);
                _output.Add((byte)((matchIndex - 124) & 0xff));
            }

            else
            {
                _output.Add((byte)(matchIndex + 1));
            }
        }

        public void EncodeNotMatchToken(ArraySegmentEx_Byte word)
        {
            int lenght = word.Count;
            if (lenght > 127)
            {
                _output.Add(255);
                for (int i = 0; i < word.Count; i++)
                {
                    _output.Add(word[i]);
                }
                _output.Add(_splitBy);
                //result.Add((byte)(lenght >> 8));
                //result.Add((byte)(lenght & 0xff));
            }
            else
            {
                _output.Add((byte)(lenght + 128));
                for (int i = 0; i < word.Count; i++)
                {
                    _output.Add(word[i]);
                }
            }
        }

        public void EncodeTokenZeroLenght()
        {
            _output.Add(0);
        }

        public void EncodeFinish()
        {
           
        }

        public int GetMatchEncodedLenght(int matchIndex)
        {
            if (matchIndex > 124 + 255) return 3;
            else if (matchIndex > 124) return 2;
            else return 1;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.Basic;

namespace ToolsLib.AdvanceStructure
{
    public class SlidingWindow 
    {
        private int _maxWindowSize;
        
        private byte[] _tmpWindowBuffer;
      

        private int _WIndexStart;
        private int _WIndexEnd;

        public SlidingWindow(int windowSize)
        {
            _maxWindowSize = windowSize;

            int bufferSize = windowSize * 2;
            //if (windowSize < 1024) bufferSize = 1024 * 2;

            _tmpWindowBuffer = new byte[ bufferSize];
      
            _WIndexStart = 0;
            _WIndexEnd = -1;

        }

       
        public byte this[int index]   // long is a 64-bit integer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // Read one byte at offset index and return it.
            get
            {
                return _tmpWindowBuffer[_WIndexStart + index];
            }
           
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCurrWindowSize() { return _WIndexEnd - _WIndexStart + 1; }
        public byte GetWindowFirstByte() 
        {
          
            return _tmpWindowBuffer[_WIndexStart]; 
        }

        public byte GetWindowLastByte() 
        {
            return _tmpWindowBuffer[_WIndexEnd]; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetWindowByte(int index)
        {
            return _tmpWindowBuffer[_WIndexStart + index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetUshort(int index)
        {
            return (ushort) (_tmpWindowBuffer[_WIndexStart + index] << 8 | _tmpWindowBuffer[_WIndexStart + index + 1]);
            //return BitConverter.ToUInt16(_tmpWindowBuffer, _WIndexStart + index);
        }

        public void Clear()
        {
            _WIndexStart = 0;
            _WIndexEnd = -1;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLeft(int count )
        {
            if (count < GetCurrWindowSize()) _WIndexStart += count;
            else
            {
                Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddByte(byte data)
        {
            if (GetCurrWindowSize() < _maxWindowSize)
                _tmpWindowBuffer[++_WIndexEnd] = data;
            else
            {
                _tmpWindowBuffer[++_WIndexEnd] = data;
                _WIndexStart++;
            }

            if (_WIndexEnd == _tmpWindowBuffer.Length - 1)
            {
                Buffer.BlockCopy(_tmpWindowBuffer, _WIndexStart, _tmpWindowBuffer, 0, GetCurrWindowSize());
                // move index window at end shift data and read next bytes
                /*for (int i = _WIndexStart, s = 0; i <= _WIndexEnd; i++, s++)
                {
                    _tmpWindowBuffer[s] = _tmpWindowBuffer[i];
                }*/

                int tmpLen = _WIndexStart - 0;
                _WIndexStart -= tmpLen;
                _WIndexEnd -= tmpLen;

                
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddByte(byte [] data,int offset, int count)
        {
            // buffer ma dostatecnou velikost 
            if(_WIndexEnd + count < _tmpWindowBuffer.Length - 1)
            {
                AddByte_AppendOnly(data, offset, count);
            }
            else
            {
                // ted zacina sranda 
                // okno je vetsi jak zbyvajici velikost
                // skopiruje pocet znaku zbyvajici do konce bloku

                int countToEnd = _tmpWindowBuffer.Length - 1 - _WIndexEnd;
                AddByte_AppendOnly(data, offset, countToEnd);

                // presune okno s daty na pocatek
                Buffer.BlockCopy(_tmpWindowBuffer, _WIndexStart, _tmpWindowBuffer, 0, GetCurrWindowSize());
                int tmpLen = _WIndexStart - 0;
                _WIndexStart -= tmpLen;
                _WIndexEnd -= tmpLen;

                // nyni do presunuteho bloku dat nakopirujeme zbyvajici
                AddByte_AppendOnly(data, offset+countToEnd, count - countToEnd);
                
            }
            
        }

        /// <summary>
        /// nakopiruje data z pole na konec pole a pripadne posune okno,
        /// pocet kopirovanych dat nesmi presahnout max velikost bufferu
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        private void AddByte_AppendOnly(byte [] data, int index, int count)
        {
            if (count == 0) return;

            // zkopirujeme data
            Buffer.BlockCopy(data, index,_tmpWindowBuffer, _WIndexEnd+1,  count);
            
            if (GetCurrWindowSize() + count < _maxWindowSize)
            {
                // okno  je po nacteni dat mensi nez je maximalni velikost
                _WIndexEnd += count;
            }
            else
            {
                // okno je vetsi nez maximalni velikost, pouze posuneme pocatek a konec
               
                _WIndexEnd += count;
                _WIndexStart = _WIndexEnd - (_maxWindowSize - 1);
            }
        }

        public bool IsMaxSizeWindow()
        {
            return GetCurrWindowSize() == _maxWindowSize;
        }

        public ArraySegmentEx_Byte GetAsSegmentArray()
        {
            return new ArraySegmentEx_Byte(this._tmpWindowBuffer, this._WIndexStart, this.GetCurrWindowSize());
        }
    }

    public class SlidingWindow_Int
    {
        private int _maxWindowSize;

        private int[] _tmpWindowBuffer;


        private int _WIndexStart;
        private int _WIndexEnd;

        public SlidingWindow_Int(int windowSize)
        {
            _maxWindowSize = windowSize;

            int bufferSize = windowSize * 2;
            //if (windowSize < 1024) bufferSize = 1024 * 2;

            _tmpWindowBuffer = new int[ bufferSize];

            _WIndexStart = 0;
            _WIndexEnd = -1;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCurrWindowSize() { return _WIndexEnd - _WIndexStart + 1; }
        public int GetWindowFirstByte()
        {

            return _tmpWindowBuffer[_WIndexStart];
        }

        public int GetWindowLastByte()
        {
            return _tmpWindowBuffer[_WIndexEnd];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWindowByte(int index)
        {
            return _tmpWindowBuffer[_WIndexStart + index];
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public int GetWindowShort(int index)
        //{
        //    return  _tmpWindowBuffer[_WIndexStart + index];
        //}


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWindowByte(int index, int data)
        {
            _tmpWindowBuffer[_WIndexStart + index] = data;
        }

        public void Clear()
        {
            _WIndexStart = 0;
            _WIndexEnd = -1;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLeft(int count)
        {
            if (count < GetCurrWindowSize()) _WIndexStart += count;
            else
            {
                Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInt(int data)
        {
           

            if (GetCurrWindowSize() < _maxWindowSize)
                _tmpWindowBuffer[++_WIndexEnd] = data;
            else
            {
                _tmpWindowBuffer[++_WIndexEnd] = data;
                _WIndexStart++;
            }

            if (_WIndexEnd == _tmpWindowBuffer.Length - 1)
            {
                Buffer.BlockCopy(_tmpWindowBuffer, _WIndexStart*sizeof(int), _tmpWindowBuffer, 0, GetCurrWindowSize() * sizeof(int));
                // move index window at end shift data and read next bytes
                /*for (int i = _WIndexStart, s = 0; i <= _WIndexEnd; i++, s++)
                {
                    _tmpWindowBuffer[s] = _tmpWindowBuffer[i];
                }*/

                int tmpLen = _WIndexStart - 0;
                _WIndexStart -= tmpLen;
                _WIndexEnd -= tmpLen;

            }


        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInt(byte[] data, int offset, int count)
        {
            // buffer ma dostatecnou velikost 
            if (_WIndexEnd + count < _tmpWindowBuffer.Length - 1)
            {
                AddByte_AppendOnly(data, offset, count);
            }
            else
            {
                // ted zacina sranda 
                // okno je vetsi jak zbyvajici velikost
                // skopiruje pocet znaku zbyvajici do konce bloku

                int countToEnd = _tmpWindowBuffer.Length - 1 - _WIndexEnd;
                AddByte_AppendOnly(data, offset, countToEnd);

                // presune okno s daty na pocatek
                Buffer.BlockCopy(_tmpWindowBuffer, _WIndexStart * sizeof(int), _tmpWindowBuffer, 0, GetCurrWindowSize() * sizeof(int));
                int tmpLen = _WIndexStart - 0;
                _WIndexStart -= tmpLen;
                _WIndexEnd -= tmpLen;

                // nyni do presunuteho bloku dat nakopirujeme zbyvajici
                AddByte_AppendOnly(data, offset + countToEnd, count - countToEnd);

            }

        }

        /// <summary>
        /// nakopiruje data z pole na konec pole a pripadne posune okno,
        /// pocet kopirovanych dat nesmi presahnout max velikost bufferu
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        private void AddByte_AppendOnly(byte[] data, int index, int count)
        {
            if (count == 0) return;

            // zkopirujeme data
            Buffer.BlockCopy(data, index * sizeof(int), _tmpWindowBuffer, (_WIndexEnd + 1)* sizeof(int), count * sizeof(int));

            if (GetCurrWindowSize() + count < _maxWindowSize)
            {
                // okno  je po nacteni dat mensi nez je maximalni velikost
                _WIndexEnd += count;
            }
            else
            {
                // okno je vetsi nez maximalni velikost, pouze posuneme pocatek a konec

                _WIndexEnd += count;
                _WIndexStart = _WIndexEnd - (_maxWindowSize - 1);
            }
        }

        public bool IsMaxSizeWindow()
        {
            return GetCurrWindowSize() == _maxWindowSize;
        }

        
    }
}

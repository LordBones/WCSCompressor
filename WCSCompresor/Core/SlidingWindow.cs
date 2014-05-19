using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core
{
    class SlidingWindow 
    {
        private Stream _source = null;

        private int _maxWindowSize;
        
        private byte[] _tmpWindowBuffer;
        private int _tmpWindowBufferCount;

        private int _WIndexStart;
        private int _WIndexEnd;

        public SlidingWindow(Stream f, int windowSize)
        {
            _source = f;
            _maxWindowSize = windowSize;
            
            _tmpWindowBuffer = new byte[10 * windowSize];
            _tmpWindowBufferCount = f.Read(_tmpWindowBuffer,0,_tmpWindowBuffer.Length);

            _WIndexStart = 0;
            _WIndexEnd = -1;

        }

        public int GetCurrWindowSize() { return _WIndexEnd - _WIndexStart + 1; }
        public byte GetWindowFirstByte() 
        {
            if (_WIndexEnd < _WIndexStart)
                throw new IndexOutOfRangeException();

            return _tmpWindowBuffer[_WIndexStart]; 
        }

        public byte GetWindowLastByte() 
        {
            if (_WIndexEnd < _WIndexStart)
                throw new IndexOutOfRangeException();
            return _tmpWindowBuffer[_WIndexEnd]; 
        }

        public byte GetWindowByte(int index)
        {
            if (_WIndexEnd < _WIndexStart+index)
                throw new IndexOutOfRangeException();
            return _tmpWindowBuffer[_WIndexStart+index];
        }

        public byte GetByte()
        {
            return _tmpWindowBuffer[_WIndexEnd+1];
        }

        
        public void MoveWindow()
        {
            if (_WIndexEnd >= (_tmpWindowBufferCount))
                throw new IndexOutOfRangeException();

            if (GetCurrWindowSize() < _maxWindowSize)
                _WIndexEnd++;
            else
            {
                _WIndexEnd++;
                _WIndexStart++;
            }

            if(_WIndexEnd == _tmpWindowBuffer.Length-1)
            {
                // move index window at end shift data and read next bytes
                for(int i = _WIndexStart, s = 0;i <= _WIndexEnd;i++,s++)
                {
                    _tmpWindowBuffer[s] = _tmpWindowBuffer[i];
                }

                int tmpLen = _WIndexStart-0;
                _WIndexStart -= tmpLen;
                _WIndexEnd -= tmpLen;

                _tmpWindowBufferCount = _WIndexEnd + 1;

                // read next bytes
                _tmpWindowBufferCount += _source.Read(_tmpWindowBuffer, _tmpWindowBufferCount, _tmpWindowBuffer.Length - _tmpWindowBufferCount);
            }
        }

        public bool HasByteToRead()
        {
            return _WIndexEnd + 1 < _tmpWindowBufferCount;
        }

        public bool WillWindowMove()
        {
            return GetCurrWindowSize() < _maxWindowSize;
        }
    }
}

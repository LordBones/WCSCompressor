using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core
{
    class LookupPredictor
    {
        int [][] _lookup;
        int[] _totalCount;
        int [] _DataNotZeroCount;

        public LookupPredictor()
        {
            _lookup = new int[256][];
            _totalCount = new int[256];
            _DataNotZeroCount = new int[256];

            for(int i = 0;i<256;i++)
            {
                _lookup[i] = new int[256];
                _totalCount[i] = 0;

                for (int c = 0; c < 256; c++)
                    _lookup[i][c] = 0;
            }
        }

        public void AddByte(byte prescedentor, byte data)
        {
            if (_lookup[prescedentor][data] == 0)
                _DataNotZeroCount[prescedentor]++;

            _lookup[prescedentor][data]++;
            _totalCount[prescedentor]++;
        }

        public void RemoveByte(byte prescedentor, byte data)
        {
            if (_lookup[prescedentor][data] == 1)
                _DataNotZeroCount[prescedentor]--;


            _lookup[prescedentor][data]--;
            _totalCount[prescedentor]--;
            if (_lookup[prescedentor][data] < 0)
                throw new IndexOutOfRangeException();
        }

        public bool IsDataFollowAncestorOnly(byte prescedentor, byte data)
        {
            if ( _lookup[prescedentor][data] == _totalCount[prescedentor] && _lookup[prescedentor][data] != 0)
                return true;
            else
                return false;
        }

        public bool IsDataFollowAncestorOnlyFirstBreak(byte prescedentor, byte data)
        {
            return _lookup[prescedentor][data] == 0 && _DataNotZeroCount[prescedentor] == 1;
        }
    }
}

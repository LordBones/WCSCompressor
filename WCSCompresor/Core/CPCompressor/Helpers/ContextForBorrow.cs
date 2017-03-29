using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core.CPCompressor.Helpers
{
    class ContextForBorrowDic
    {
        private Dictionary<short, byte[]> _contextForBorrow = new Dictionary<short, byte[]>();

        public ContextForBorrowDic()
        {
            
        }

        
        public byte [] this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                byte[] context;
                if(!_contextForBorrow.TryGetValue((short)i,out context))
                {
                    context = new byte[i];
                    _contextForBorrow.Add((short)i, context);
                }

                return context;
            }
            
        }
    }

    class ContextForBorrow
    {
        private byte[][] _contextForBorrow ;
        private int _minContextLenght;
        private int _maxContextLenght;

        public ContextForBorrow(int minContextLength, int maxContextLenght)
        {
            _minContextLenght = minContextLength;
            _maxContextLenght = maxContextLenght;

            int len = maxContextLenght - _minContextLenght + 1;
            _contextForBorrow = new byte[len][];

            for(int i  = 0;i<len;i++)
            {
                _contextForBorrow[i] = new byte[this._minContextLenght + i];
            }
        }


        public byte[] this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this._contextForBorrow[i - this._minContextLenght];
            }
        }
    }

    class ContextForBorrow<T> where T: new()
    {
        private T[][] _contextForBorrow;
        private int _minContextLenght;
        private int _maxContextLenght;

        public ContextForBorrow(int minContextLength, int maxContextLenght)
        {
            _minContextLenght = minContextLength;
            _maxContextLenght = maxContextLenght;

            int len = maxContextLenght - _minContextLenght + 1;
            _contextForBorrow = new T[len][];

            for (int i = 0; i < len; i++)
            {
                _contextForBorrow[i] = new T[this._minContextLenght + i];
            }
        }


        public T[] this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this._contextForBorrow[i - this._minContextLenght];
            }
        }
    }
}

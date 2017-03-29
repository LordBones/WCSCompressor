using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core
{
    public class ContextDictionary<T>
        where T : class
    {
        const int CONST_hashSize = (1<<12)-1;


        struct HashItem
        {
            public byte[] Context;
            public long HashCode;
            public T Data;
        }

        private List<HashItem>[] _lookup = new List<HashItem>[CONST_hashSize];

        public ContextDictionary()
        {
            for(int i  = 0; i < _lookup.Length;i++)
            {
                _lookup[i] = new List<HashItem>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(byte [] context,T data)
        {
            long hashCode = GetHashCode(context);
            long lookupIndex = hashCode % CONST_hashSize;

            _lookup[lookupIndex].Add(new HashItem() { Context = context, HashCode = hashCode, Data = data });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(byte [] context )
        {
            long hashCode = GetHashCode(context);
            long lookupIndex = hashCode % CONST_hashSize;

            List<HashItem> items = _lookup[lookupIndex];

            for (int i = 0;i < items.Count;i++)
            {
                HashItem item = items[i];
                if (item.HashCode == hashCode)
                {
                    if(IsEqual(item.Context,context))
                    {
                        return item.Data;
                    }
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long  GetHashCode(byte []  context)
        {
            ulong tmp = 0;
            for (int i = 0; i < context.Length; i++)
            {
                tmp = tmp * 63333333 + context[i];
            }

            //tmp = tmp * 63333333 + context[context.Length-1];

            return (long)(tmp % long.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEqual(byte [] x, byte [] y)
        {
            if (x.Length != y.Length) return false;

            //int tmp = 0;
            for (int i = 0; i < x.Length; i++)
            {
              //  tmp |= (x[i] ^ y[i]);

                if (x[i] != y[i]) return false;
            }

//            if (tmp != 0) return false;

            return true;
        }
    }
}

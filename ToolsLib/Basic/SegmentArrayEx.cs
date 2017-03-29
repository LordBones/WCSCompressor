using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsLib.Basic
{
    public class ArraySegmentEx<T>
    {
        public ArraySegmentEx(T[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (array.Length - offset < count)
            {
                throw new ArgumentException();
            }
            this._array = array;
            this._offset = offset;
            this._count = count;
        }

        private T[] _array;

        private int _offset;

        private int _count;


        public ArraySegmentEx<T> CreateSegment(int index, int count)
        {
            return CreateSegmentFromSegment(this, index, count);
        }

        public static ArraySegmentEx<T>  CreateSegmentFromSegment(ArraySegmentEx<T> segment, int index, int count)
        {
            return new ArraySegmentEx<T>(segment.Array, segment.Offset + index, count);
        }

        public T[] Array
        {
            get
            {
                return this._array;
            }
        }

        public int Offset
        {
            get
            {
                return this._offset;
            }
        }

        public int Count
        {
            get
            {
                return this._count;
            }
        }

        public T this[int index]
        {
            get
            {
                return this._array[this._offset + index];
            }
        }
    }

    public class ArraySegmentEx_Byte : ArraySegmentEx<byte>, IEquatable<ArraySegmentEx_Byte>
    {
        private int _hash = -1;

        // hash bude -1;
        // chceteli ho mit zavolejte funkci co ho vypocte a priradi to teto promene
        public int Hash => _hash;
        
        public ArraySegmentEx_Byte(byte[] array, int offset, int count)
            : base(array, offset, count)
        {
           // GenerateHash();
        }

        public new ArraySegmentEx_Byte CreateSegment(int index, int count)
        {
            return new ArraySegmentEx_Byte(this.Array,this.Offset+index,count);
        }

        public void GenerateHash()
        {
            uint hash = (uint)Count;

            for (int i = 0; i < this.Count; i++)
                hash = (hash << 4) ^ (hash >> 28) ^  this[ i];

            this._hash = (int)((hash ^ (hash >> 10) ^ (hash >> 20)) & 0x7fffffff);
        }

        public bool Equals(ArraySegmentEx_Byte other)
        {
            if (other == null)
                return false;

            return other.Array == this.Array && other.Count == this.Count && other.Offset == this.Offset;
        }
    }
}

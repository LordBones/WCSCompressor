using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsLib.Basic
{
    public class ArraySegmentComparer : IEqualityComparer<ArraySegmentEx_Byte>
    {
        public bool Equals(ArraySegmentEx_Byte x, ArraySegmentEx_Byte y)
        {
            if (x.Count != y.Count) return false;

            if (x.Array == y.Array && x.Offset == y.Offset) return true;

            byte[] arrayx = x.Array;
            byte[] arrayy = y.Array;
            for (int i = 0; i < x.Count; i++)
            {
                if (arrayx[x.Offset + i] != arrayy[y.Offset + i]) return false;
            }
            
            return true;
        }

        public bool Equals2(ArraySegmentEx_Byte x, ArraySegmentEx_Byte y)
        {
            if (x.Count != y.Count) return false;

            int offsetx = x.Offset;
            int offsety = y.Offset;

            for (int i = 0; i < x.Count-1; i+=2)
            {
                if (((x.Array[offsetx + i] != y.Array[offsety + i]) ||
                    (x.Array[offsetx + i+1] != y.Array[offsety + i+1])) 
                    ) return false;
            }

            if((x.Count & 1) == 1)
            {
                return (x.Array[offsetx + x.Count - 1] == y.Array[offsety + y.Count - 1]);
            }

                return true;
        }

        public int GetHashCode1(ArraySegmentEx_Byte obj)
        {
            int sum = 0;
            int offset = obj.Offset;
            for (int i = 0; i < obj.Count; i++)
            {
                sum = sum * 13 + obj.Array[offset + i];
            }


            return sum;
        }

        public int GetHashCode(ArraySegmentEx_Byte obj)
        {
            if (obj.Hash < 0) obj.GenerateHash();

            return obj.Hash;

            uint hash = (uint)obj.Count;

            for (int i = 0; i < obj.Count; i++)
                hash = (hash << 4) ^ (hash >> 28) ^ obj.Array[obj.Offset+ i];

            return (int)((hash ^ (hash >> 10) ^ (hash >> 20)) & 0x7fffffff);

        }


        public int GetHashCode2(ArraySegmentEx_Byte obj)
        {
            int sum = 0;
            int offset = obj.Offset;
            for (int i = 0; i + 1 < obj.Count; i += 2)
            {
                sum = sum * 13 + ((obj.Array[offset + i]<<8)| (obj.Array[offset + i+1]));
            }

            if((obj.Count & 1) == 1 )
            {
                sum = sum * 13 + (obj.Array[offset + obj.Count - 1]);
            }

            return sum;
        }
    }
}

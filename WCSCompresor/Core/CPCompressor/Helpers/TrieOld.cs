using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core.CPCompressor.Helpers
{
    class TrieOld<T>
    {
        private TrieNode[] _root = new TrieNode[256];
        private T[] _data;
        private int _dataCurrentLenght;

        public TrieOld()
        {
            for (int i = 0; i < 256; i++)
            {
                _root[i] = null;
            }

            _data = new T[16];
            _dataCurrentLenght = 0;
        }

        public bool TryGetValue(byte[] key, out T data)
        {
            TrieNode node = FindNode(key);

            if (node != null && node.Depth == key.Length - 1 && node.DataIndex >= 0)
            {
                data = this._data[node.DataIndex];
                return true;
            }


            data = default(T);
            return false;
        }

        public void Add(byte[] key, T data)
        {
            TrieNode node = FindNode(key);

            if (node != null && node.Depth == key.Length - 1)
            {
                InsertNodeData(node, data);
            }
            else
            {
                node = InsertRestNodes(node, key);
                InsertNodeData(node, data);
            }
        }

        private void InsertNodeData(TrieNode node, T data)
        {
            if (this._data.Length == this._dataCurrentLenght)
            {
                int size = (this._dataCurrentLenght < 16) ? 16 : this._dataCurrentLenght + (this._dataCurrentLenght >> 1);

                T[] tmp = new T[size];
                Array.Copy(this._data, tmp, this._data.Length);
                this._data = tmp;
            }

            node.DataIndex = this._dataCurrentLenght;
            this._data[this._dataCurrentLenght] = data;
            this._dataCurrentLenght++;
        }

        private TrieNode InsertRestNodes(TrieNode startNode, byte[] key)
        {
            TrieNode node = startNode;

            short currentDepth = 0;
            if (node == null)
            {
                node = new TrieNode(key[currentDepth], currentDepth);
                this._root[node.keyPart] = node;
            }
            else
            {
                currentDepth = node.Depth;
            }

            //if(currentDepth == key.Length-1)
            //{
            //    return node;
            //}

            while (currentDepth < key.Length - 1)
            {
                if (node.Children == null)
                {
                    node.Children = new TrieNode[1];
                    node.lookup = new byte[1];
                    currentDepth++;
                    node.Children[0] = new TrieNode(key[currentDepth], currentDepth);
                    node.lookup[0] = key[currentDepth];

                    node = node.Children[0];
                    continue;
                }

                currentDepth++;

                int i = FindIndexChildren(node.lookup, key[currentDepth]);
                i = ~i;
                TrieNode newNode = new TrieNode(key[currentDepth], currentDepth);

                //node.Children.Add(newNode);
                //node.lookup.Add(newNode.keyPart);
                node.Children = InsertIntoChildren(node.Children, i, newNode);
                //    node.Children.Insert(i,newNode);
                //node.lookup.Insert(i, newNode.keyPart);
                node.lookup = InsertIntoLookup(node.lookup, i, newNode.keyPart);
                // TestArrayInOrder(node.lookup);
                node = newNode;

            }

            return node;
        }

        /// <summary>
        /// vraci index nalezeneho potomka v uzlu
        /// pokud neni nalezen vraci zapornou hodnotu, ktera je negaci mista kam vlozit noveho potomka
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static int FindIndexChildren(byte[] lookup, byte data)
        {
            int i = 0;

            if (lookup.Length < 11)
            {
                for (; i < lookup.Length; i++)
                {
                    byte tmp = lookup[i];
                    if (tmp >= data)
                    {
                        if (tmp == data) return i;
                        break;
                    }
                }

                i = ~i;
            }
            else //if (node.lookup.Length > 14)
            {

                i = BinarySearchIterative(lookup, data);
            }

            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TrieNode[] InsertIntoChildren(TrieNode[] child, int i, TrieNode data)
        {

            TrieNode[] tmp = new TrieNode[child.Length + 1];

            for (int k = 0; k < i; k++)
            {
                tmp[k] = child[k];
            }

            //Buffer.BlockCopy(lookup, 0, tmp, 0,i);

            if (i < child.Length)
            {
                for (int k = 0; k < child.Length - i; k++)
                {
                    tmp[i + k + 1] = child[i + k];
                }

                //Buffer.BlockCopy(lookup, i , tmp, i + 1, lookup.Length - i);
            }

            tmp[i] = data;

            return tmp;
        }

        private bool TestArrayInOrder(byte[] data)
        {
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i - 1] > data[i])
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] InsertIntoLookup(byte[] lookup, int i, byte data)
        {
            byte[] tmp = new byte[lookup.Length + 1];

            for (int k = 0; k < i; k++)
            {
                tmp[k] = lookup[k];
            }

            //Buffer.BlockCopy(lookup, 0, tmp, 0,i);

            if (i < lookup.Length)
            {
                //for (int k = 0; k < lookup.Length-i; k++)
                //{
                //    tmp[i+k+1] = lookup[i+k];
                //}

                Buffer.BlockCopy(lookup, i, tmp, i + 1, lookup.Length - i);
            }

            tmp[i] = data;

            return tmp;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearchIterative(byte[] inputArray, byte key)
        {
            int min = 0;
            int max = inputArray.Length - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                byte tmp = inputArray[mid];
                if (key <= tmp)
                {
                    if (key == tmp)
                        return mid;

                    max = mid - 1;
                }

                else
                {
                    min = mid + 1;
                }
            }
            return ~min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearchIterative2(byte[] inputArray, byte key)
        {
            int min = 0;
            int max = inputArray.Length - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                byte tmp = inputArray[mid];
                if (key == tmp)
                {
                    return mid;
                }
                else if (key < tmp)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }
            return ~min;
        }

        internal struct ChildrenItem
        {
            public byte Data;
            public TrieNode Node;

            public ChildrenItem(byte data, TrieNode node)
            {
                this.Data = data;
                this.Node = node;
            }
        }

        private TrieNode FindNode(byte[] key)
        {
            short depth = 0;
            TrieNode node = this._root[key[depth]];
            depth++;

            while (node != null && depth < key.Length)
            {
                if (node.Children == null) break;


                int i = FindIndexChildren(node.lookup, key[depth]);


                if (i >= 0)
                {
                    node = node.Children[i];
                    depth++;
                    continue;
                }

                break;
            }

            return node;
        }


        internal class TrieNode
        {
            public TrieNode[] Children;
            public int DataIndex;
            public short Depth;
            public byte keyPart;

            public byte[] lookup;



            public TrieNode(byte key, short depth)
            {
                this.keyPart = key;
                this.Depth = depth;
                this.Children = null;
                this.lookup = null;
                this.DataIndex = -1;
            }
        }
    
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core.CPCompressor.Helpers
{
    public class TrieStructFast<T>
    {
        private int[] _root = new int[256];

        private TrieNode[] _nodePool = new TrieNode[16];
        private int _nodePoolCurrentLength = 0;

        private T[] _data;
        private int _dataCurrentLenght;

        public TrieStructFast()
        {
            for (int i = 0; i < 256; i++)
            {
                _root[i] = -1;
            }

            _data = new T[16];
            _dataCurrentLenght = 0;
        }

        public bool TryGetValue(byte[] key, out T data)
        {
            long nodeIndex = FindNode(key);

            if (nodeIndex >= 0)
            {
                TrieNode node = _nodePool[nodeIndex];

                if (node.Depth == key.Length - 1 && node.DataIndex >= 0)
                {
                    data = this._data[node.DataIndex];
                    return true;
                }
            }


            data = default(T);
            return false;
        }

        /// <summary>
        /// vrati vsechny nalezene data zadaneho kontextu
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void GetAllPossibleData(byte[] key, ref T [] data)
        {
            Array.Clear(data, 0, data.Length);

            int depth = 0;
            int nodeIndex = this._root[key[depth]];
            
            if (nodeIndex >= 0)
            {
                while (depth < key.Length)
                {
                    TrieNode node = this._nodePool[nodeIndex];

                    if (node.DataIndex >= 0)
                    {
                        data[depth] = this._data[node.DataIndex];
                    }

                    if (node.Children == null) break;

                    depth++;
                    int i = FindIndexChildren(node.Children, key[depth]);


                    if (i >= 0)
                    {
                        nodeIndex = node.Children[i].Node;
                       // depth++;
                        continue;
                    }

                    break;
                }
            }
        }


        public void Add(byte[] key, T data)
        {
            int nodeIndex = FindNode(key);

            if (nodeIndex >= 0)
            {
                TrieNode node = _nodePool[nodeIndex];

                if (node.Depth == key.Length - 1)
                {
                    InsertNodeData(nodeIndex, data);
                }
                else
                {
                    int tmpNodeIndex = InsertRestNodes(nodeIndex, key);
                    InsertNodeData(tmpNodeIndex, data);
                }
            }
            else
            {
                int tmpNodeIndex = InsertRestNodes(nodeIndex, key);
                InsertNodeData(tmpNodeIndex, data);
            }
        }

        private void InsertNodeData(int nodeIndex, T data)
        {
            if (this._data.Length == this._dataCurrentLenght)
            {
                int size = (this._dataCurrentLenght < 16) ? 16 : this._dataCurrentLenght + (this._dataCurrentLenght >> 1);

                T[] tmp = new T[size];
                Array.Copy(this._data, tmp, this._data.Length);
                this._data = tmp;
            }

            TrieNode node = _nodePool[nodeIndex];
            node.DataIndex = this._dataCurrentLenght;
            _nodePool[nodeIndex] = node;

            this._data[this._dataCurrentLenght] = data;
            this._dataCurrentLenght++;
        }

        private int InsertIntoNodePool(TrieNode node)
        {
            if (this._nodePool.Length == this._nodePoolCurrentLength)
            {
                int size = (this._nodePoolCurrentLength < 16) ? 16 : this._nodePoolCurrentLength + (this._nodePoolCurrentLength >> 1);

                TrieNode[] tmp = new TrieNode[size];
                Array.Copy(this._nodePool, tmp, this._nodePool.Length);
                this._nodePool = tmp;
            }

            this._nodePool[this._nodePoolCurrentLength] = node;
            this._nodePoolCurrentLength++;

            return this._nodePoolCurrentLength - 1;
        }

        private int InsertRestNodes(int startNodeIndex, byte[] key)
        {
            TrieNode node;


            short currentDepth = 0;
            if (startNodeIndex < 0)
            {
                TrieNode tmpNode = new TrieNode(key[currentDepth], currentDepth);

                int nodeIndex = InsertIntoNodePool(tmpNode);
                node = tmpNode;
                this._root[node.keyPart] = nodeIndex;
                startNodeIndex = nodeIndex;
            }
            else
            {
                node = this._nodePool[startNodeIndex];
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
                    node.Children = new ChildrenItem[1];
                    currentDepth++;

                    TrieNode tmpNode = new TrieNode(key[currentDepth], currentDepth);
                    int tmpNodeIndex = InsertIntoNodePool(tmpNode);
                    node.Children[0] = new ChildrenItem(key[currentDepth], tmpNodeIndex);
                   
                    this._nodePool[startNodeIndex] = node;
                    startNodeIndex = tmpNodeIndex;
                    node = tmpNode;
                    continue;
                }

                currentDepth++;

                int i = FindIndexChildren(node.Children, key[currentDepth]);
                i = ~i;

                TrieNode newNode = new TrieNode(key[currentDepth], currentDepth);
                int newNodeIndex = InsertIntoNodePool(newNode);

               

                node.Children = InsertIntoChildren(node.Children,  i,new ChildrenItem(key[currentDepth],newNodeIndex));
                
                //TestArrayInOrder(node.lookup);
                this._nodePool[startNodeIndex] = node;
                startNodeIndex = newNodeIndex;
                node = newNode;
            }

            return startNodeIndex;
        }

        /// <summary>
        /// vraci index nalezeneho potomka v uzlu
        /// pokud neni nalezen vraci zapornou hodnotu, ktera je negaci mista kam vlozit noveho potomka
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindIndexChildren(ChildrenItem[] lookup, byte data)
        {
            int i = 0;

            if (lookup.Length < 11)
            {
                for (; i < lookup.Length; i++)
                {
                    byte tmp = lookup[i].Data;
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
        private ChildrenItem[] InsertIntoChildren(ChildrenItem[] child, int i, ChildrenItem data)
        {


            {

                ChildrenItem[] tmp = new ChildrenItem[child.Length + 1];

                for (int k = 0; k < i; k++)
                {
                    tmp[k] = child[k];
                }

                if (i < child.Length)
                {
                    for (int k = 0; k < child.Length - i; k++)
                    {
                        tmp[i + k + 1] = child[i + k];
                    }
                }

                tmp[i] = data;

                return tmp;
            }
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

            //{
            //    int k = 0;
            //    while (k + 3 < i)
            //    {
            //        tmp[k] = lookup[k];
            //        tmp[k + 1] = lookup[k + 1];
            //        tmp[k + 2] = lookup[k + 2];
            //        tmp[k + 3] = lookup[k + 3];
            //        k += 4;
            //    }

            //    for (; k < i; k++)
            //    {
            //        tmp[k] = lookup[k];
            //    }
            //}
            for (int k = 0; k < i; k++)
            {
                tmp[k] = lookup[k];
            }

            //Buffer.BlockCopy(lookup, 0, tmp, 0,i);

            if (i < lookup.Length)
            {
                for (int k = 0; k < lookup.Length - i; k++)
                {
                    tmp[i + k + 1] = lookup[i + k];
                }

                //Buffer.BlockCopy(lookup, i, tmp, i + 1, lookup.Length - i);
            }

            tmp[i] = data;

            return tmp;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearchIterative(ChildrenItem[] inputArray, byte key)
        {
            int min = 0;
            int max = inputArray.Length - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                byte tmp = inputArray[mid].Data;
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

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            public int Node;

            public ChildrenItem(byte data, int node)
            {
                this.Data = data;
                this.Node = node;
            }
        }

        private int FindNode(byte[] key)
        {
            int depth = 0;
            int nodeIndex = this._root[key[depth]];
            depth++;
            if (nodeIndex >= 0)
            {
                while (depth < key.Length)
                {
                    TrieNode node = this._nodePool[nodeIndex];

                    if (node.Children == null) break;
                    
                    int i = FindIndexChildren(node.Children, key[depth]);

                    if (i >= 0)
                    {
                        nodeIndex = node.Children[i].Node;
                        depth++;
                        continue;
                    }

                    break;
                }
            }

            return nodeIndex;
        }

       
        internal struct TrieNode
        {

            public ChildrenItem[] Children;
            public int DataIndex;
          
            public short Depth;
            public byte keyPart;

            public TrieNode(byte key, short depth)
            {
                this.keyPart = key;
                this.Depth = depth;
                this.Children = null;
                this.DataIndex = -1;
               
            }
        }
    
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using ToolsLib.Basic;

namespace WCSCompress.LzC
{
    public class HashSSlideWindow
    {
        private const int CONST_HashSize = 3;
        /// <summary>
        /// pocatecni index, jeho zvysovanim dochazi k posouvani pozice rozdilem indexu 
        /// a tohoto cisla
        /// </summary>
        private int _offsetIndexStart;

        struct HashItem
        {
            public int IndexFirstWord;
            public int IndexLastWord;

            public HashItem(int indexFirstWord,  int indexLastWord)
            {
                this.IndexFirstWord = indexFirstWord;
                this.IndexLastWord = indexLastWord;
            }
        }

        private HashItem[] _hashTable;
        private int hashMask = 0;
        private SlidingWindow_Int _wordArrayPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get_StartMatchIndex(int hash) => _hashTable[hash].IndexFirstWord - this._offsetIndexStart;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get_NextMatchIndexDistance(int indexLastMatch) => _wordArrayPosition.GetWindowByte(indexLastMatch);

        public HashSSlideWindow(int sizeHashTable, int sizeSlideWindow)
        {
            if (sizeHashTable > 16 || sizeHashTable <= 0) sizeHashTable = 7;

            sizeHashTable = (1 << sizeHashTable);

            hashMask = sizeHashTable - 1;

            this._offsetIndexStart = 0;
            this._hashTable = new HashItem[sizeHashTable];
            this._wordArrayPosition = new SlidingWindow_Int(sizeSlideWindow);

            for(int i = 0;i < this._hashTable.Length;i++)
            {
                this._hashTable[i] = new HashItem(-1, -1);
            }
        }



        public void AchjoAddByte(SlidingWindow swData, byte data)
        {
            MoveAddEmpty(1, swData);
            swData.AddByte(data);
            UpdateAllEmpty(swData);
           
        }

        /// <summary>
        /// smaze polozky z hash pred posunem sliding window
        /// </summary>
        /// <param name="countMove"></param>
        /// <param name="swData"></param>
        public void MoveAddEmpty(int countMove, SlidingWindow swData)
        {
           // CheckHashSearchIsValid(swData);
            for (int k = 0;k<countMove;++k)
            {
                

                if(this._wordArrayPosition.IsMaxSizeWindow())
                {
                    

                    RemoveFirstMatchFromHash(swData,k);

                    ++this._offsetIndexStart;

                    
                }

                // pridame novou pozici a posuneme pole s pozicemi
                this._wordArrayPosition.AddInt(-1);
               // CheckHashSearchIsValid(swData);

            }
            
        }

        /// <summary>
        /// prida polozky do hash nad jiz aktualnim sliding window
        /// </summary>
        /// <param name="countMove"></param>
        /// <param name="swData"></param>
        public void UpdateAllEmpty( SlidingWindow swData)
        {
           

            if (swData.GetCurrWindowSize() < CONST_HashSize) return;

            int startIndexForUpdate = swData.GetCurrWindowSize() - CONST_HashSize;

            // najdeme prvni vyskyt prazdneho hash pokracovani
            while (startIndexForUpdate >= 0 && this._wordArrayPosition.GetWindowByte(startIndexForUpdate) < 0) startIndexForUpdate--;

            startIndexForUpdate++;

            int endIndex = swData.GetCurrWindowSize() - CONST_HashSize + 1;

            while(startIndexForUpdate < endIndex)
            {
                int hash = ComputeHash(swData, startIndexForUpdate);
                if (hash < 0) break;

                var hashItem = this._hashTable[hash];
                if(hashItem.IndexLastWord < 0)
                {
                    int position = this._offsetIndexStart + startIndexForUpdate;
                    // pridani noveho hash
                    hashItem.IndexFirstWord = position;
                    hashItem.IndexLastWord = position;
                    this._hashTable[hash] = hashItem;

                    this._wordArrayPosition.SetWindowByte(startIndexForUpdate, 0);

                }
                else
                {
                    

                    // krok od posledniho vyskytu
                    int lastWordIndex = (hashItem.IndexLastWord - this._offsetIndexStart);

                    this._wordArrayPosition.SetWindowByte(lastWordIndex, startIndexForUpdate - lastWordIndex);
                    
                    hashItem.IndexLastWord = this._offsetIndexStart + startIndexForUpdate;
                    this._hashTable[hash] = hashItem;

                    this._wordArrayPosition.SetWindowByte(startIndexForUpdate, 0);
                    
                }

                startIndexForUpdate++;

             
            }
        }

        /// <summary>
        /// smaze jeden hash a premapuje pripadne na dalsi polozku,
        /// start index urcuje ktery prvni index bereme v uvahu pri volani pro vice najednou
        /// </summary>
        /// <param name="swData"></param>
        /// <param name="startIndex"></param>
        private void RemoveFirstMatchFromHash(SlidingWindow swData, int startIndex)
        {
            // kontrola hash pred posunem, zda ho nema vyradit
            //if (this._wordArrayPosition.IsMaxSizeWindow())
            {
                int hash = ComputeHash(swData, startIndex);
                if (hash >= 0)
                {
                    HashItem hashData = this._hashTable[hash];

                    if (this._wordArrayPosition.GetWindowByte(0) == 0)
                    {
                        hashData.IndexFirstWord = -1;
                        hashData.IndexLastWord = -1;
                    }
                    else
                    {
                        hashData.IndexFirstWord += this._wordArrayPosition.GetWindowByte(0);
                    }

                    this._hashTable[hash] = hashData;

                }
            }

        }

        public int CheckHashSearchIsValid(SlidingWindow data)
        {
            for (int i = 0; i < _hashTable.Length; i++)
            {
                HashItem hashValue = _hashTable[i];
                if(hashValue.IndexFirstWord >= 0)
                {
                    int testHash = ComputeHash(data, hashValue.IndexFirstWord - this._offsetIndexStart);
                    if(testHash != i)
                    {
                        return i;
                    }
                }

                if (hashValue.IndexLastWord >= 0)
                {
                    int testHash = ComputeHash(data, hashValue.IndexLastWord - this._offsetIndexStart);
                    if (testHash != i)
                    {
                        return i;
                    }
                }
                
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeHash(SlidingWindow data, int index)
        {
            //return  data.GetWindowByte(index) & 1;

            if (data.GetCurrWindowSize() - index < 3) return -1;


            //            int tmp = data.GetWindowByte(index) << 16 | data.GetWindowByte(index + 1) << 8 | data.GetWindowByte(index + 2);

            //int tmp = 0;
            //tmp = tmp * 633 + data.GetWindowByte(index);
            //tmp = tmp * 633 + data.GetWindowByte(index+1);
            //tmp = tmp * 633 + data.GetWindowByte(index+2);

            
            ulong  h = 2166136261UL;


            h = (h * 16777619Ul) ^ data.GetWindowByte(index);
            h = (h * 16777619Ul) ^ data.GetWindowByte(index + 1);
            h = (h * 16777619Ul) ^ data.GetWindowByte(index + 2);





            // int tmp = data.GetUshort(index) << 8 | data.GetWindowByte(index + 2);

            return (int)(h & (ulong)(this.hashMask));
        }

    }
}

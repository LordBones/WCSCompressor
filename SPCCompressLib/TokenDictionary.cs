
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.Basic;

namespace SPCCompressLib
{
    public class TokenDictionary
    {
        private static ArraySegmentComparer _asComparer = new ArraySegmentComparer();

        private int _nextTokenIndex = 0;
        private int _maxCountMostFrequentWords;
        private List<TokenItem> _wordsMostFrequent = new List<TokenItem>();
        private Dictionary<ArraySegmentEx_Byte, TokenItem> _wordsMostFrequent_Lookup;// = new Dictionary<ArraySegmentEx_Byte, TokenItem>(_asComparer);

        private Dictionary<ArraySegmentEx_Byte, TokenItem> _wordsDict;

        class TokenItem
        {
            public ArraySegmentEx_Byte Token;
            public int Index = 0;
            public int TokenCount = 0;

            //public int Spare => (TokenCount +1) * Token.Count;
            //public int SpareLong => (TokenCount+1) * (Token.Count-2);

            public TokenItem()
            {

            }

            public TokenItem(ArraySegmentEx_Byte token, int index)
            {
                this.Index = index;
                this.Token = token;
            }
        }

        public TokenDictionary(int maxCountMostFrequentWords, int hintDictSize)
        {
            this._maxCountMostFrequentWords = maxCountMostFrequentWords;
            this._wordsMostFrequent_Lookup = new Dictionary<ArraySegmentEx_Byte, TokenItem>(this._maxCountMostFrequentWords, _asComparer);
            this._wordsMostFrequent = new List<TokenItem>(_maxCountMostFrequentWords);
            this._wordsDict = new Dictionary<ArraySegmentEx_Byte, TokenItem>(hintDictSize, _asComparer);
        }

        /// <summary>
        /// zaporny index znamena nenalezeno
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public int MatchToken(ArraySegmentEx_Byte token)
        {
            TokenItem ti = FindMFT(token);
            if(ti != null)
            {
                return ti.Index;
            }
            
            if (_wordsDict.TryGetValue(token,out ti))
            {
                return ti.Index;
            }

            return -1;
        }

        public void AddOrUpdateCountToken(ArraySegmentEx_Byte token)
        {
            TokenItem ti = FindMFT(token);
            if(ti != null)
            {
                this._wordsMostFrequent[ti.Index].TokenCount++;

                MFTUpdatePositonMove(ti.Index);
            }
            else
            {
                if(_wordsMostFrequent.Count < this._maxCountMostFrequentWords)
                {
                    TokenItem tmpTi = new TokenItem(token, _nextTokenIndex);
                    _wordsMostFrequent.Add(tmpTi);
                    _wordsMostFrequent_Lookup.Add(token, tmpTi);
                    _nextTokenIndex++;
                }
                else
                {
                    TokenItem wdTi;
                    if(_wordsDict.TryGetValue(token,out wdTi))
                    {
                        wdTi.TokenCount++;

                        WDUpdatePositionIfNeeded(wdTi);
                    }
                    else
                    {
                        wdTi = new TokenItem(token, _nextTokenIndex);
                        _wordsDict.Add(token, wdTi);
                       _nextTokenIndex++;
                    }
                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TokenItem FindMFT(ArraySegmentEx_Byte token)
        {
            TokenItem ti;
            _wordsMostFrequent_Lookup.TryGetValue(token, out ti);
            
            return ti;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MFTUpdatePositonMove(int index)
        {
            TokenItem tmpTi = this._wordsMostFrequent[index];
            while(index>0)
            {
                var tmpWMFToken = this._wordsMostFrequent[index - 1];

                if (!((tmpTi.TokenCount > tmpWMFToken.TokenCount) ||
                    (tmpTi.TokenCount == tmpWMFToken.TokenCount &&
                     tmpTi.Token.Count >= tmpWMFToken.Token.Count
                  )))
                {
                    break;
                }
                
                tmpWMFToken.Index++;
                this._wordsMostFrequent[index] = tmpWMFToken ;
                
                index--;
            }

            tmpTi.Index = index;

            this._wordsMostFrequent[index] = tmpTi;
        }

        /// <summary>
        /// pokud je token dostatecne cetny zameni a zaradi ho do seznamu
        /// nejcetnejsich tokenu
        /// a nejhorsiho da zpatky do slovniku
        /// </summary>
        /// <param name="token"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WDUpdatePositionIfNeeded(TokenItem token)
        {
            TokenItem mftToken = this._wordsMostFrequent[this._wordsMostFrequent.Count-1];
            if(token.TokenCount >= mftToken.TokenCount)
            {
                int mftTokenIndex = mftToken.Index;

                mftToken.Index = token.Index;
                this._wordsDict.Remove(token.Token);
                this._wordsDict.Add(mftToken.Token, mftToken);

                this._wordsMostFrequent_Lookup.Remove(mftToken.Token);
                this._wordsMostFrequent_Lookup.Add(token.Token, token);

                token.Index = mftTokenIndex;
                this._wordsMostFrequent[mftTokenIndex] = token;

                MFTUpdatePositonMove(mftTokenIndex);
            }
        }

    }
}

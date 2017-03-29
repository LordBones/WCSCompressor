using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using WCSCompress.Core.CPCompressor.Modules;
using WCSCompress.Core.CPCompressor.VO;

namespace WCSCompress.Core.CPCompressor
{
    class ByteArraryComparer : IEqualityComparer<byte[]>
    {
        
        //System.Data.HashFunction.xxHash xxHash = new System.Data.HashFunction.xxHash();
        bool IEqualityComparer<byte[]>.Equals(byte[] x, byte[] y)
        {
            if (x.Length != y.Length) return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) return false;
            }
            return true;
        }

        System.Data.HashFunction.xxHash hash = new System.Data.HashFunction.xxHash();

       
        int IEqualityComparer<byte[]>.GetHashCode(byte[] obj)
        {
            //int result = 0;
            //obj = hash.ComputeHash(obj);

            //result += ((obj[3] & 0x7f) << 24);
            //result += ((obj[2]) << 16);
            //result += ((obj[1]) << 8);
            //result += ((obj[0]));

            //return result;



            //if(i < obj.Length-1)
            //{
            //    tmp = (tmp * 633333 + obj[i+1]);
            //}

            //return (int)tmp % int.MaxValue;

            //uint hash = (uint)obj.Length; 

            //for (int i = 0; i < obj.Length; ++i)
            //    hash = (hash << 4) ^ (hash >> 28) ^ obj[i];

            //return (int)((hash ^ (hash >> 10) ^ (hash >> 20)) & 0x7fffffff);

            //ulong h = 0xcbf29ce484222325;
            //int i;

            //for (i = 0; i < obj.Length; i++)
            //    h = (h ^ obj[i]) * 0x100000001b3;

            //return(int)h % int.MaxValue;

            uint tmp = 0x84222325;
            for (int i = 0; i < obj.Length; i++)
            {
                tmp = tmp * 63333333 + obj[i];
                //tmp = (tmp  + obj[i])*63333333;

            }

            return (int)(tmp & 0x7fffffff);
            //tmp % int.MaxValue;
        }
    }

    struct CharProbabiliity
    {
        
        public double probability;
        public int count;
        public int lastDataIndex;
        public byte data;

        public void Set(double probability)
        {
            this.probability = probability;
        }
    }

    public class Predictor
    {
        class LastCharPredictor
        {
            public byte lastByte;
            public byte lastByteCount = 0;
            public BinaryCounter predictSuccess = new BinaryCounter();
            const byte CONST_MaxLastByteCount = 1;

            public LastCharPredictor()
            {

            }

            public LastCharPredictor(byte lastByte)
            {
                this.lastByte = lastByte;
                this.lastByteCount = 1;
            }

            public void SetByte(byte newByte)
            {
                if (newByte == lastByte)
                {
                    if (lastByteCount < CONST_MaxLastByteCount)
                        lastByteCount++;
                }
                else
                {
                    if (lastByteCount > 1)
                    {
                        lastByteCount--;
                    }
                    else
                    {
                        lastByte = newByte;
                        lastByteCount = CONST_MaxLastByteCount;
                    }
                }
            }
        }

        class LastCharPredictorLight
        {
            public byte lastByte;
            public byte lastByteCount = 0;
            public BinaryCounterLight predictSuccess = new BinaryCounterLight();
            const byte CONST_MaxLastByteCount = 3;

            public LastCharPredictorLight()
            {

            }

            public bool IsPredictPosible => lastByteCount >= 2;

            public LastCharPredictorLight(byte lastByte)
            {
                this.lastByte = lastByte;
                this.lastByteCount = 1;
            }

            public void SetByte(byte newByte)
            {
                if (newByte == lastByte)
                {
                    if (lastByteCount < CONST_MaxLastByteCount)
                        lastByteCount++;
                }
                else
                {
                    if (lastByteCount > 1)
                    {
                        lastByteCount--;
                    }
                    else
                    {
                        lastByte = newByte;
                        lastByteCount = 1;// CONST_MaxLastByteCount;
                    }
                }
            }
        }

        const int CONST_NC_Order1_Smart_maxcontext = 128;

        const int LastCharPreditor_contextLength = 7;

        const int PredictCircle_ContextLength = 0;

        IPredictorModule[] _modulePredictors = new IPredictorModule[0];

        private CharProbabiliity [] _predChar;
        private int _predCharCount;

        class OnePredictor
        {
            public CharCountsOrder[] _lookup = new CharCountsOrder[256];
            public BinaryCounter[] _lookup_PredictSuccess = new BinaryCounter[256];
        }

        private void AddNewModulePredictor(IPredictorModule module)
        {
            IPredictorModule[] tmp = new IPredictorModule[_modulePredictors.Length + 1];
            Array.Copy(_modulePredictors, tmp, _modulePredictors.Length);

            tmp[tmp.Length - 1] = module;

            _modulePredictors = tmp;
        }

        public void Init()
        {
            _modulePredictors = new IPredictorModule[0];

            AddNewModulePredictor(new EmptyModule());
            
            //AddNewModulePredictor(new NC_Order1_Smart(CONST_NC_Order1_Smart_maxcontext));
            //AddNewModulePredictor(new Order1_ByteCounterModule());

            AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 1, 0));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 1, 1));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 1, 2));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 1, 3));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 1, 4));
           // AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 1, 5));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 1, 6));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 2, 0));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 2, 1));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 2, 1));
            // AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 2, 0));
            // AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 2, 1));
             AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 3, 0));
            AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 3, 1));
            AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 3, 2));
            //  AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 4, 0));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 4, 1));
            //AddNewModulePredictor(new ContextPredictorModuleTrie(LastCharPreditor_contextLength, 5, 0));

            //AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength,1,0));
            //AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 2, 0));

            //AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 3,0));

            //AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 5,0));
            //AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 4,0));
            //AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 6,0));
            // AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 7,0));
            // AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 8,0));

            // AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 9, 0));
            // AddNewModulePredictor(new ContextPredictorModule(LastCharPreditor_contextLength, 10, 0));

            for (int i = 2; i <= PredictCircle_ContextLength; i++)
            {
                AddNewModulePredictor(new CircleContextPredictModule(i));
            }
            //AddNewModulePredictor(new CircleContextPredictModule(3));
            //AddNewModulePredictor(new CircleContextPredictModule(4));
            //AddNewModulePredictor(new CircleContextPredictModule(2));

            this._predChar = new CharProbabiliity[_modulePredictors.Length];
            this._predCharCount = 0;
        }


        private void AddPredChar( byte data, int lastDataIndex, int currentDataIndex, double probability)
        {
            //CharProbabiliity tmp = null;

            int tmpIndex = -1;

            for (int i = 0; i < this._predCharCount; i++)
            {
                if (this._predChar[i].data == data)
                {
                    //tmp = predChar[i];
                    tmpIndex = i;
                    break;
                }
            }

            //if (probability > 0.5)
            //    probability = Math.Sqrt(probability);
            //else
            //{
            //    probability = 0.0;// probability * probability;
            //}

            if (tmpIndex < 0)
            {
                //if (lastDataIndex + 1024 >= currentDataIndex)
                {
                    this._predChar[this._predCharCount++] = new CharProbabiliity() { lastDataIndex = lastDataIndex, data = data, probability = probability, count = 1 };
                }
                //predChar.Add(new CharProbabiliity() { data = data, probability = 1.0 });
            }
            else
            {
                var kk = this._predChar[tmpIndex];

               /* if (kk.lastDataIndex+1024 < currentDataIndex )
                //if (kk.probability < probability)
                {
                    if (lastDataIndex + 1024 < currentDataIndex)
                    {
                        if (kk.probability < probability)
                        {
                            //kk.Set(1.0 - (1.0 - kk.probability) * (1.0 - probability));
                            //kk.probability += probability;
                            kk.probability = probability;
                            kk.count += 1;
                            kk.lastDataIndex = lastDataIndex;
                            //kk.Set(probability);
                            this._predChar[tmpIndex] = kk;
                        }
                    }
                    else
                    {
                        kk.probability = probability;
                        kk.count += 1;
                        kk.lastDataIndex = lastDataIndex;
                        //kk.Set(probability);
                        this._predChar[tmpIndex] = kk;
                    }
                }
                else if(lastDataIndex+1024< currentDataIndex)
                {
                    kk.Set(1.0 - (1.0 - kk.probability) * (1.0 - probability));

                    //if (kk.probability < probability)
                    {
                        //kk.probability += probability;
                      //  kk.probability = probability;
                        kk.count += 1;
                        kk.lastDataIndex = lastDataIndex;
                        //kk.Set(probability);
                        this._predChar[tmpIndex] = kk;
                    }
                }*/

               
                //if (kk.lastDataIndex < lastDataIndex)
                if (kk.probability < probability)
                {
                    kk.Set(1.0 - (1.0 - kk.probability) * (1.0 - probability));
                    //kk.probability += probability;
                    //kk.probability = probability;
                    kk.count += 1;
                    kk.lastDataIndex = lastDataIndex;
                    //kk.Set(probability);
                    this._predChar[tmpIndex] = kk;

                }
                
                
            }
        }

        private int PredChar_GetBest()
        {
            double tmp = double.MinValue;
            int result = -1;
            int count = 0;

            for (int i = 0; i < this._predCharCount; i++)
            {
                CharProbabiliity cp = this._predChar[i];
                if (cp.probability > tmp)
                {
                    tmp = cp.probability;
                    result = cp.data;
                    count = cp.count;

                }
            }

            //if (predChar.Count > 1)
            //{
            //    double tmp2 = double.MinValue;
            //    byte result2 = 0;
            //    int count2 = 0;

            //    for (int i = 0; i < predChar.Count; i++)
            //    {
            //        CharProbabiliity cp = predChar[i];
            //        if (cp.probability > tmp2 && cp.data != result)
            //        {
            //            tmp2 = cp.probability;
            //            result2 = cp.data;
            //            count2 = cp.count;

            //        }
            //    }

            //    if (tmp2 + 0.05 > tmp && count2 > count)
            //        return result2;


            //    return result;
            //}

            return result;
        }

        private double PredChar_GetBestProbability(List<CharProbabiliity> predChar)
        {
            double tmp = double.MinValue;

            for (int i = 0; i < predChar.Count; i++)
            {
                if (predChar[i].probability > tmp)
                {
                    tmp = predChar[i].probability;
                }
            }

            return tmp;
        }

       
        public int PredictByte(SlidingWindow historyContext, int DataIndex)
        {
            this._predCharCount = 0;

            for(int i = 0;i < _modulePredictors.Length;i++)
            {
                CharProbabilityVO predictResult = _modulePredictors[i].PredictByte(historyContext,DataIndex);
                if(predictResult.IsPrediction)
                {
                    AddPredChar( predictResult.data, predictResult.LastDataIndex,DataIndex, predictResult.probability);
                }
            }
           
            return PredChar_GetBest();
        }


        public void UpdatePredictorSuccess(SlidingWindow historyContext,int DataIndex, byte nextData)
        {
            for (int i = 0; i < _modulePredictors.Length; i++)
            {
                _modulePredictors[i].UpdatePredictorSuccess(historyContext,DataIndex,nextData);
            }
           
        }

        public void UpdatePredictor(SlidingWindow historyContext, int DataIndex, byte nextData)
        {
            for (int i = 0; i < _modulePredictors.Length; i++)
            {
                _modulePredictors[i].UpdatePredictor(historyContext,DataIndex, nextData);
            }

        }



       
    }
}

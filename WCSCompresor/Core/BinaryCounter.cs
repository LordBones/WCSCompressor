using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core
{
    public class BinaryCounter
    {
        private byte _lastValue;
        private int _oneInRowCount;
        private int _oneInRowCountLast;
        private int _oneRowLength_Sum = 0;
        private int _oneRowCount = 0;
        private double _oneRowLength_SumStd = 0;
        private double _oneRowLength_SumStdNegative = 0;
        private int _oneRowLength_SumStdNegative_Count = 0;

        private double _oneRowLength_SumStdPositive = 0;
        private int _oneRowLength_SumStdPositive_Count = 0;


        List<byte> _history = new List<byte>();


        public int TotalCount;
        public int OneCount;
        public int ZeroCount
        {
            get
            {
                return this.TotalCount - this.OneCount;

            }
        }

        public double ZeroRatio
        {
            get
            {
                return (TotalCount == 0) ? 0.0 : (1 - (OneCount / (double)TotalCount));
            }
        }

        public double OneRatio
        {
            get
            {
                return (TotalCount == 0)? 0.0 : (OneCount / (double)TotalCount);
            }
        }

        public bool LastWasOne => _lastValue == 1;
        public bool LastWasZero => _lastValue == 0;

        public double AvgOneInRow_Lenght => (this._oneRowCount == 0)? 0.0 : (this._oneRowLength_Sum + this._oneInRowCount) / (double)(this._oneRowCount+1);
        public double StdOneInRow_Lenght => (this._oneRowCount == 0) ? 0.0 : this._oneRowLength_SumStd / (double)this._oneRowCount;
        //public double StdOneInRowPositive_Lenght => (this._oneRowLength_SumStdPositive_Count == 0)?0.0:this._oneRowLength_SumStdPositive / (double)this._oneRowLength_SumStdPositive_Count;
        //public double StdOneInRowNegative_Lenght => (this._oneRowLength_SumStdNegative_Count == 0)?0.0 : this._oneRowLength_SumStdNegative / (double)this._oneRowLength_SumStdNegative_Count;
        public double StdOneInRowPositive_Lenght => (this._oneRowCount == 0) ? 0.0 : this._oneRowLength_SumStdPositive / (double)this._oneRowCount;
        public double StdOneInRowNegative_Lenght => (this._oneRowCount == 0) ? 0.0 : this._oneRowLength_SumStdNegative / (double)this._oneRowCount;

        public int OneRowCount => _oneRowCount;
        public int OneInRowCount => _oneInRowCount;
        public int OneInRowCountLast => _oneInRowCountLast;

        public void Init()
        {
            this.TotalCount = 0;
            this.OneCount = 0;
        }

        public void AddZero(int increment)
        {
            this.TotalCount += increment;

            //_history.Add(0);

            if (this.LastWasOne)
            {
                double stdDiff = this._oneInRowCount - this.AvgOneInRow_Lenght;
                
                double stdDiffAbs = Math.Abs(stdDiff);

                //if (stdDiff<0.0)
                //{
                //    _oneRowLength_SumStdNegative += stdDiffAbs;
                //    _oneRowLength_SumStdNegative_Count++;
                //}
                //else
                //{
                //    _oneRowLength_SumStdPositive += stdDiffAbs;
                //    _oneRowLength_SumStdPositive_Count++;
                //}

                //_oneRowLength_SumStdNegative += stdDiff;
                //_oneRowLength_SumStdNegative_Count += 1;
                _oneRowLength_SumStd += stdDiffAbs;

                this._oneRowCount++;
                this._oneRowLength_Sum += this._oneInRowCount;

                this._oneInRowCountLast = this._oneInRowCount;

                this._oneInRowCount = 0;
                this._lastValue = 0;
            }

            //ReduceHistory();
            //this.ZeroCount += increment;
        }

        public void AddOne(int increment)
        {
            this.TotalCount += increment;
            this.OneCount += increment;

            this._oneInRowCount += 1;
            this._lastValue = 1;

            //_history.Add(1);

            //ReduceHistory();

        }

        private void ReduceHistory()
        {
            //return;
            if (this.TotalCount <0)
            {
                int len = this.TotalCount >> 1;

                List<byte> newHistory = new List<byte>(len);
                for (int i = this.TotalCount - len; i < this.TotalCount; i++)
                {
                    newHistory.Add(_history[i]);
                }

                _history = newHistory;

                _lastValue = 0;
                _oneInRowCount = 0;
                _oneRowLength_Sum = 1;
                _oneRowCount = 1;
                _oneRowLength_SumStd = 0;

                TotalCount = 0;
                OneCount = 0;

                for (int i = 0; i < _history.Count; i++)
                {
                    if (_history[i] == 0)
                    {
                        this.TotalCount++;

                        if (this.LastWasOne)
                        {
                            this._oneRowCount++;
                            this._oneRowLength_Sum += this._oneInRowCount;

                            _oneRowLength_SumStd += Math.Abs(this.AvgOneInRow_Lenght - this._oneInRowCount);

                            this._oneInRowCount = 0;
                            this._lastValue = 0;
                        }

                    }
                    else
                    {
                        this.TotalCount++;
                        this.OneCount++;

                        this._oneInRowCount += 1;
                        this._lastValue = 1;
                    }
                }


            }
        }
    }

   

    public class BinaryCounterLight
    {
        private byte _lastValue;
        //private short _oneInRowCount;
        //private short _oneInRowCountLast;

        public short TotalCount;
        public short OneCount;
        public short ZeroInRow;
        public int ZeroCount
        {
            get
            {
                return this.TotalCount - this.OneCount;

            }
        }

        public double ZeroRatio
        {
            get
            {
                return (TotalCount == 0) ? 0.0 : (1 - (OneCount / (double)TotalCount));
            }
        }

        public double OneRatio
        {
            get
            {
                return (TotalCount == 0) ? 0.0 : (OneCount / (double)TotalCount);
            }
        }

        public bool LastWasOne => _lastValue == 1;
        public bool LastWasZero => _lastValue == 0;

        // public int OneInRowCount => _oneInRowCount;
        // public int OneInRowCountLast => _oneInRowCountLast;

        public BinaryCounterLight()
        {
            this.TotalCount = 2;
            this.OneCount = 1;
        }


        public void Init()
        {
            this.TotalCount = 0;
            this.OneCount = 0;
        }

        public void AddZero(int increment)
        {
            if (TotalCount == short.MaxValue)
            {
                this.TotalCount >>= 1;
                this.OneCount >>= 1;
                this.ZeroInRow >>= 1;
            }

            this.TotalCount++;//= increment;
            this.ZeroInRow++;

            this._lastValue = 0;
            //if (this.LastWasOne)
            {
                //  this._oneInRowCountLast = this._oneInRowCount;

                //this._oneInRowCount = 0;

            }

            //this.ZeroCount += increment;
        }

        public void AddOne(int increment)
        {
            if (TotalCount == short.MaxValue)
            {
                this.TotalCount >>= 1;
                this.OneCount >>= 1;
                this.ZeroInRow >>= 1;
            }

            this.TotalCount++;//= increment;
            this.OneCount++;//= increment;
            this.ZeroInRow = 0;
            //  this._oneInRowCount += 1;
            this._lastValue = 1;

            //_history.Add(1);


        }

    }

}

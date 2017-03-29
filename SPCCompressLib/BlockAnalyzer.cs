using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.Core.CSPCompressor
{
    internal class BlockAnalyzer
    {
        //private ArraySegmentComparer _acpComparer = new ArraySegmentComparer();
        private int[] _byteDistances = new int[256];
        HashSet<int>[] _matchSameDistances = new HashSet<int>[256];

        StatByte[] _fastLookup = new StatByte[256];

        public AnalyzeResult Analyze(byte [] data)
        {
            AnalyzeResult result = new AnalyzeResult();

            StatByte[] statChars = CoputeStatChar(data);
            result.StatBytes = statChars;


            return result;
        }

        private StatByte [] CoputeStatChar(byte [] data)
        {
            List<StatByte> result = new List<StatByte>(16);

            
            Array.Clear(_byteDistances, 0, _byteDistances.Length);
            Array.Clear(_fastLookup, 0, _fastLookup.Length);

            //for (int i = 0;i<_matchSameDistances.Length;i++)
            //{
            //    if(_matchSameDistances[i] != null)
            //    {
            //        _matchSameDistances[i].Clear();
            //    }
            //}



            //Array.Clear(_matchSameDistances, 0, _matchSameDistances.Length);
            // int[] byteDistances = new int[256];


            

            for (int i = 0;i< data.Length;i++)
            {
                //StatByte sc = result.FirstOrDefault(x => x.Byte == data[i]);
                StatByte sc = null;
                
                sc = _fastLookup[data[i]];

                if (sc != null)
                {
                    

                    int distance = i - _byteDistances[sc.Byte] - 1;
                    _byteDistances[sc.Byte] = i;



                    sc.CountByte++;
                    sc.DistancesSum += distance;
                    //sc.StdDevSum += (int)Math.Abs(sc.AvgDistance - distance);
                   
                  


                    if (sc.DistanceMax < distance) sc.DistanceMax = distance;


                    if(distance == 0)
                    {
                        if(sc.lastDistance == distance)
                        {
                            sc.DistancesMatchSame++;
                        }
                    }
                    //else if (MatchSameDistance(_matchSameDistances, sc.Byte, distance))
                    //{
                    //        //if (distance > 0)
                    //        {
                    //            sc.DistancesMatchSameNotZero++;
                    //            sc.DistancesMatchSame++;
                    //        }
                    //}

                    sc.lastDistance = distance;

                }
                else
                {
                    byte oneByte = data[i];
                    int distance = i - _byteDistances[oneByte];
              
                    _byteDistances[oneByte] = i;

                    sc = new StatByte();
                    sc.Byte = oneByte;
                    sc.CountByte = 1;
                    sc.DistancesSum = distance;
                    sc.DistanceMax = distance;
                    sc.lastDistance = -1;


                   // MatchSameDistance(_matchSameDistances, oneByte, distance);

                    _fastLookup[sc.Byte] = sc;
                    result.Add(sc);
                    
                }
            }

            for (int i = 0; i < result.Count; i++)
            {

                StatByte sc = result[i];

                int distance = data.Length - _byteDistances[sc.Byte] - 1;
                sc.DistancesSum += distance;

                if (sc.DistanceMax < distance) sc.DistanceMax = distance;

                if (distance == 0)
                {
                    if (sc.lastDistance == distance)
                    {
                        sc.DistancesMatchSame++;
                    }
                }
                //else if (MatchSameDistance(_matchSameDistances, sc.Byte, distance))
                //{
                //    //if (distance > 0)
                //    {
                //        sc.DistancesMatchSameNotZero++;
                //        sc.DistancesMatchSame++;
                //    }
                //}

                sc.lastDistance = distance;

            }

            return result.ToArray();
        }

      

        private bool MatchSameDistance(HashSet<int>[] matchSameDistances, byte data, int distance)
        {
            HashSet<int> listDistances = matchSameDistances[data];
            if (listDistances != null)
            {
                bool result = !listDistances.Add(distance);
                return result;

                /*if (listDistances.Contains(distance))
                {
                    return true;
                }
                else
                {
                    ;
                }*/
            }
            else
            {
                listDistances = new HashSet<int>();
                listDistances.Add(distance);
                matchSameDistances[data] = listDistances;
            }

            return false;
        }
    }

    internal class AnalyzeResult
    {
        public StatByte[] StatBytes;

        public StatByte GetMostCount()
        {
            StatByte result = null;

            for(int i = 0;i<StatBytes.Length;i++)
            {
                if (result == null || (StatBytes[i].CountByte > result.CountByte ))
                    result = StatBytes[i];
            }

            return result;
        }

        public StatByte GetMinAvgDistance()
        {
            StatByte result = null;

            for (int i = 0; i < StatBytes.Length; i++)
            {
                if (result == null || StatBytes[i].AvgDistance < result.AvgDistance)
                    result = StatBytes[i];
            }

            return result;
        }

        public StatByte GetMinMaxDistance()
        {
            StatByte result = null;

            for (int i = 0; i < StatBytes.Length; i++)
            {
                if (result == null || StatBytes[i].DistanceMax < result.DistanceMax)
                    result = StatBytes[i];
            }

            return result;
        }

      

        public StatByte GetMaxMatchNZDistance()
        {
            StatByte result = null;

            for (int i = 0; i < StatBytes.Length; i++)
            {
                if (
                                      
                    (

                    result == null || 
                    (StatBytes[i].DistancesMatchSameNotZero > result.DistancesMatchSameNotZero 
                    
                    )))
                    result = StatBytes[i];
            }

            return result;
        }

        public StatByte GetMaxMatchDistance()
        {
            StatByte result = null;

            for (int i = 0; i < StatBytes.Length; i++)
            {
                if (

                    (

                    result == null ||
                    (StatBytes[i].DistancesMatchSame > result.DistancesMatchSame

                    )))
                    result = StatBytes[i];
            }

            return result;
        }

        public StatByte GetMinStdDevDistance()
        {
            StatByte result = null;

            for (int i = 0; i < StatBytes.Length; i++)
            {
                

                if (

                    (

                    result == null ||
                    (StatBytes[i].AvgStdDiffDistance < result.AvgStdDiffDistance && StatBytes[i].CountByte >1

                    )))
                    result = StatBytes[i];
            }

            return result;
        }
    }

    internal class StatByte
    {
        public byte Byte;
        public int CountByte;
        public int CountDistances => CountByte + 1;
        public int DistancesMatchSame;
        public int DistancesMatchSameNotZero;
        public int StdDevSum;
        internal  int lastDistance ;
        public int DistanceMax;
        public int DistancesSum;
        public float AvgStdDiffDistance => (CountDistances == 0) ? 0 : StdDevSum / (float)CountDistances;
        public float AvgDistance => (CountDistances == 0)? 0 : DistancesSum /(float) CountDistances;
    }
    
}

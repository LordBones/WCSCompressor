using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;
using WCSCompress.Core.CPCompressor;

namespace WCSCompress.Core
{
    /// <summary>
    /// compress predictor
    /// </summary>
    class CPCompress
    {

        public struct TestStatistic
        {
            public double success;
            public double SuccessCount;
            public double AvgCountSuccInRow;
            public int CountSuccInRow;
            public double StdCountSuccInRow;
            public int RealByteSave;
        }

        PredictorGroup[] _predictors = new PredictorGroup[2];

        public class PredictorGroup
        {
            public Predictor predictor = new Predictor();
            public SlidingWindow slWindow = new SlidingWindow(1024);
        }

        public double PredictorTest2(Stream input)
        {
            _predictors = new PredictorGroup[16];
            for(int i = 0;i< this._predictors.Length;i++)
            {
                this._predictors[i] = new PredictorGroup();
                this._predictors[i].predictor.Init();
                this._predictors[i].slWindow.AddByte(0);
            }
            
            BufferedStream bfInput = new BufferedStream(input);
            
            int data;
            int success = 0;
            int total = 0;
            int index = 0;
            while (true)
            {
                int predIndex = (index/1) % 4;

                PredictorGroup predGroup = this._predictors[predIndex];

                data = ReadByte(bfInput);
                if (data < 0) break;

                int predictData = predGroup.predictor.PredictByte(predGroup.slWindow,total);
                if ((predictData >= 0 && predictData == data))
                {
                    success++;
                }
                else
                {
                }

                total++;

                predGroup.predictor.UpdatePredictorSuccess(predGroup.slWindow,total, (byte)data);
                predGroup.predictor.UpdatePredictor(predGroup.slWindow,total, (byte)data);
                predGroup.slWindow.AddByte((byte)data);

                index++;

            }

            double successRate =  success / (double)total;

            return successRate;
        }

        public void PredictorTest_AnalyzeFile(Stream input, Stream output)
        {
            Predictor predictor = new Predictor();

            BufferedStream bfInput = new BufferedStream(input);
            SlidingWindow slWindow = new SlidingWindow(1024);

            // necessary init
            slWindow.AddByte(0);
            predictor.Init();

            int data;
            int success = 0;
            int successInRow = 0;
            int count = 0;
            while (true)
            {
                data = ReadByte(bfInput);
                count++;
                if (data < 0) break;

                int predictData = predictor.PredictByte(slWindow,count);
                if ((predictData >= 0 && predictData == data))
                {
                  //  success++;
                  //  successInRow++;
                    output.WriteByte((byte)'.');
                }
                else
                {
                    //output.WriteByte((byte)'.');
                    output.WriteByte((byte)data);
                }

                predictor.UpdatePredictorSuccess(slWindow,count, (byte)data);
                predictor.UpdatePredictor(slWindow,count, (byte)data);
                slWindow.AddByte((byte)data);

            }
        }

        public TestStatistic PredictorTest(Stream input )
        {
            Predictor predictor = new Predictor();

            BufferedStream bfInput = new BufferedStream(input);
            SlidingWindow slWindow = new SlidingWindow(1024);

            // necessary init
            slWindow.AddByte(0);
            predictor.Init();

            int data;
            int success = 0;
            int successInRow = 0;
            int notSuccessInRow = 0;
            int CountMatchedRows = 0;
            int SumMatchedRows = 0;
            double SumMatchesStd = 0;
            int realByteSafe = 0;
            int total = 0;
            while (true)
            {
                data = ReadByte(bfInput);
                if (data < 0) break;

                int predictData = predictor.PredictByte(slWindow,total);
                if ((predictData >= 0 && predictData == data))
                {
                    success++;
                    successInRow++;
                }
                else
                {
                    //if(successInRow > 0)
                    {
                        if (successInRow > 2)
                        {
                            realByteSafe += successInRow - 2;
                            SumMatchedRows += successInRow;
                            CountMatchedRows++;

                            double avg = (SumMatchedRows / (double)CountMatchedRows);
                            SumMatchesStd += Math.Abs(avg - successInRow);
                        }

                        successInRow = 0;

                       
                    }
                }

                total++;

                predictor.UpdatePredictorSuccess(slWindow,total, (byte)data);
                predictor.UpdatePredictor(slWindow,total, (byte)data);
                slWindow.AddByte((byte)data);

            }

            TestStatistic result = new TestStatistic();
            result.success =  success / (double)total;
            result.SuccessCount = success;
            result.CountSuccInRow = CountMatchedRows;
            result.AvgCountSuccInRow = SumMatchedRows / (double)CountMatchedRows;
            result.StdCountSuccInRow = SumMatchesStd / (double)CountMatchedRows;
            result.RealByteSave = realByteSafe;

            return result;
        }

        public void Coding(Stream input, Stream output, int sizeSlidingWindow)
        {
            List<byte> outputBuffer = new List<byte>();

            Predictor predictor = new Predictor();
            BufferedStream bfInput = new BufferedStream(input);
            SlidingWindow slWindow = new SlidingWindow(1024);

            // necessary init
            slWindow.AddByte(0);
            predictor.Init();
            
            List<byte> buffer = new List<byte>();

            int data;
            int successPredictCounter = 0;
            int unsuccessPredictCounter = 0;
            int count = 0;
            while (true)
            {
               

                // process unpredicted chars
                while (true)
                {
                    data = ReadByte(bfInput);
                    count++;
                    if (data < 0) break;

                    int predictData = predictor.PredictByte(slWindow,count);
                    if ((predictData >= 0 && predictData == data) || unsuccessPredictCounter > 254)
                    {
                        PutBackByte(data);
                        break;
                    }
                    else
                    {

                        predictor.UpdatePredictorSuccess(slWindow,count, (byte)data);
                        predictor.UpdatePredictor(slWindow,count, (byte)data);
                        
                        slWindow.AddByte((byte)data);
                        outputBuffer.Add((byte)data);

                        unsuccessPredictCounter++;
                    }
                }

                unsuccessPredictCounter = 0;

                ///predict as long as you can
                while (true)
                {
                    data = ReadByte(bfInput);
                    count++;
                    if (data < 0) break;

                    int predictData = predictor.PredictByte(slWindow,count);
                    if (predictData >= 0 && predictData == data && successPredictCounter < 255)
                    {
                        

                        predictor.UpdatePredictor(slWindow,count, (byte)data);
                        predictor.UpdatePredictorSuccess(slWindow,count, (byte)data);
                        slWindow.AddByte((byte)data);
                        outputBuffer.Add((byte)data);
                        successPredictCounter++;
                    }
                    else
                    {
                        PutBackByte(data);
                        break;
                    }
                }

                WriteOutputData(outputBuffer, output, successPredictCounter, false);

                successPredictCounter = 0;

                // test konce souboru
                data = ReadByte(bfInput);
                if (data < 0) break;
                else PutBackByte(data);
            }

            WriteOutputData(outputBuffer, output, 0, true);




        }

        /*public void Coding(Stream input, Stream output, int sizeSlidingWindow)
        {
            List<byte> outputBuffer = new List<byte>();

            Predictor predictor = new Predictor();
            BufferedStream bfInput = new BufferedStream(input);
            SlidingWindow slWindow = new SlidingWindow(1024);

            // necessary init
            slWindow.AddByte(0);
            predictor.Init();

            List<byte> buffer = new List<byte>();

            int data;
            int successPredictCounter = 0;
            int unsuccessPredictCounter = 0;

            while (true)
            {


                // process unpredicted chars
                while (true)
                {
                    data = ReadByte(bfInput);
                    if (data < 0) break;

                    int predictData = predictor.PredictByte(slWindow);
                    if ((predictData >= 0 && predictData == data) || unsuccessPredictCounter > 254)
                    {
                        PutBackByte(data);
                        break;
                    }
                    else
                    {

                        predictor.UpdatePredictorSuccess(slWindow, (byte)data);
                        predictor.UpdatePredictor(slWindow, (byte)data);

                        slWindow.AddByte((byte)data);
                        outputBuffer.Add((byte)data);

                        unsuccessPredictCounter++;
                    }
                }

                unsuccessPredictCounter = 0;

                ///predict as long as you can
                while (true)
                {
                    data = ReadByte(bfInput);
                    if (data < 0) break;

                    int predictData = predictor.PredictByte(slWindow);
                    if (predictData >= 0 && predictData == data && successPredictCounter < 255)
                    {


                        //predictor.UpdatePredictor(slWindow, (byte)data);
                        predictor.UpdatePredictorSuccess(slWindow, (byte)data);
                        slWindow.AddByte((byte)data);
                        outputBuffer.Add((byte)data);
                        successPredictCounter++;
                    }
                    else
                    {
                        PutBackByte(data);
                        break;
                    }
                }

                WriteOutputData(outputBuffer, output, successPredictCounter, false);

                successPredictCounter = 0;

                // test konce souboru
                data = ReadByte(bfInput);
                if (data < 0) break;
                else PutBackByte(data);
            }

            WriteOutputData(outputBuffer, output, 0, true);

        }*/

        private void WriteOutputData(List<byte> outputBuffer, Stream output, int successPredictCounter, bool writeAll)
        {
            if(successPredictCounter > 2)
            {
                // ulozi se nejdrive obsah bufferu, potom zakodovany pocet uspesneho hadani

                int index = 0;
                int countToWrite = outputBuffer.Count()-successPredictCounter;

                while (countToWrite > 255)
                {
                    output.WriteByte(255);
                    for (int i = 0; i < 255; i++)
                    {
                        output.WriteByte(outputBuffer[index]);
                        index++;
                    }

                    countToWrite -= 255;
                    // zapise pocet neodhadnutelnych
                    output.WriteByte(0);
                }

                
                    output.WriteByte((byte)countToWrite);
                    for (int i = 0; i < countToWrite; i++)
                    {
                        output.WriteByte(outputBuffer[index]);
                        index++;
                    }

                    countToWrite = 0;
                    // zapise pocet odhadnutelnych

                    
                

                while (successPredictCounter > 255)
                {
                    output.WriteByte(255);
                    // pocet neodhadnutelnych
                    output.WriteByte(0);
                    successPredictCounter -= 255;
                }

                output.WriteByte((byte)successPredictCounter);

                outputBuffer.Clear();
            }
            else
            {
                int index = 0;
                int countToWrite = outputBuffer.Count();

                while(countToWrite > 255)
                {
                    output.WriteByte(255);
                    for (int i = 0; i < 255; i++)
                    {
                        output.WriteByte(outputBuffer[index]);
                        index++;
                    }

                    countToWrite -= 255;
                    // zapise pocet neodhadnutelnych
                    output.WriteByte(0);
                }

                if(countToWrite > 0 && writeAll)
                {
                    output.WriteByte((byte)countToWrite);
                    for (int i = 0; i < countToWrite; i++)
                    {
                        output.WriteByte(outputBuffer[index]);
                        index++;
                    }

                    countToWrite = 0;
                    // zapise pocet neodhadnutelnych
                    output.WriteByte(0);

                    outputBuffer.Clear();
                }
                else
                {
                    if (index > 0)
                    {
                        List<byte> tmp = new List<byte>();
                        for (int i = 0; i < countToWrite; i++)
                        {
                            tmp.Add(outputBuffer[index]);
                            index++;
                        }

                        outputBuffer.Clear();
                        outputBuffer.AddRange(tmp);
                    }
                }
            }

        }


        private int putBackByte;
        private bool isPutBackByte;


        private void PutBackByte(int data)
        {
            if (isPutBackByte) throw new NotImplementedException();
            else
            {
                isPutBackByte = true;
                putBackByte = data;
            }
        }

        private int ReadByte( BufferedStream bfInput)
        {
            if(isPutBackByte)
            {
                isPutBackByte = false;
                return putBackByte;
            }
            else
            {
                return bfInput.ReadByte();
            }
        }

        private int Predict(CharCountsOrder [] lookup, int data)
        {
            //byte bestChar = lookup[data].Get_Char_From_Poz_Stack(0);
            //if ((lookup[data].Celk_Cetnost) <= lookup[data].Get_Cetnost_Char(bestChar)*2)
            //{
            //    return -1;
            //}

            return lookup[data].Get_Char_From_Poz_Stack(0);
        }
    }
}

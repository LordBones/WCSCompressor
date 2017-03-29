using SPCCompressLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCSCompress;
using WCSCompress.Core;
using WCSCompress.Core.CPCompressor;
using WCSCompress.Core.CPCompressor.Helpers;
using WCSCompress.LzC;
using WCSCompress.MCR;
using WCSCompress.SortC;

namespace WCSCompresor
{
    static class  Program
    {
        public static void Main(string[] args)
        {
            //TestSlidingWindow();

            if (args[0] == "-sortCEncode")
            {
                SortC sortC = new SortC();

                int blockSize = 18;
                if (args.Length == 4)
                    blockSize = int.Parse(args[3]);

                string inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[1]);
                string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[2]);


                long ticks = 0;
                using (FileStream fsIn = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
                using (FileStream fsOut = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {

                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
                    sortC.Compress(fsIn, fsOut, blockSize);
                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
                }

                FileInfo fi = new FileInfo(inputPath);
                WriteTimeRun(fi.Length, ticks);
                return;
            }


            if (args[0] == "-lzcEncode")
            {
                LzC lzc = new LzC();

                int blockSize = 128;
                if (args.Length == 4)
                    blockSize = int.Parse(args[3]);

                string inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[1]);
                string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[2]);


                long ticks = 0;
                using (FileStream fsIn = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
                using (FileStream fsOut = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                using (BufferedStream bfIn = new BufferedStream(fsIn))
                using (BufferedStream bfOut = new BufferedStream(fsOut))
                {

                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
                    lzc.Compress(bfIn, bfOut, blockSize);
                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
                }

                FileInfo fi = new FileInfo(inputPath);
                WriteTimeRun(fi.Length, ticks);
                return;
            }



            if (args[0] == "-mcrEncode")
            {
                MCR mcr = new MCR();

                int blockSize = 128;
                if (args.Length == 4)
                    blockSize = int.Parse(args[3]);

                string inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[1]);
                string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[2]);


                long ticks = 0;
                using (FileStream fsIn = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
                using (FileStream fsOut = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {

                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
                    mcr.Compress(fsIn, fsOut,blockSize);
                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
                }

                FileInfo fi = new FileInfo(inputPath);
                WriteTimeRun(fi.Length, ticks);
                return;
            }


            SPCCompress spc = new SPCCompress();
         
            if (args[0] == "-spcEncode")
            {
                int slidingSize = 128;
                if (args.Length == 4)
                    slidingSize = int.Parse(args[3]);

                string inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[1]);
                string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[2]);


                long ticks = 0;
                using (FileStream fsIn = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
                using (FileStream fsOut = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    
                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
                    spc.EncodeStream(fsIn,fsOut);
                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
                } 

                FileInfo fi = new FileInfo(inputPath);
                WriteTimeRun(fi.Length, ticks);
                return;
            }
            

            CPCompress cpc = new CPCompress();

            if (args.Length == 1)
            {
                if (args[0] == "-testDic")
                {
                    TestTrie();
                    return;
                }
            }

            if (args.Length == 2)
            {
                if (args[0] == "-tp")
                {
                    long ticks = 0;
                    string input = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[1]);
                    FileInfo fi = new FileInfo(input);
                    using (FileStream fsIn = new FileStream(input, FileMode.Open, FileAccess.Read))
                    {
                        ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
                        CPCompress.TestStatistic stat = cpc.PredictorTest(fsIn);
                        ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
                        double bpc = ((fi.Length - stat.RealByteSave) / ((double)fi.Length)) * 8.0;

                        Console.WriteLine(string.Format("bpc:{0} save {1}  match {2} P. err. : {3}%  ", bpc, stat.RealByteSave, stat.SuccessCount, (1 - stat.success) * 100));
                        Console.WriteLine(string.Format("SucLines : {0}  avgLen: {1}  std:{2}", stat.CountSuccInRow, stat.AvgCountSuccInRow, stat.StdCountSuccInRow));
                    }


                    WriteTimeRun(fi.Length, ticks);
                }

                return;
            }

            if (args.Length != 3 && args.Length != 4)
                return;



            WCSCompressor vcs = new WCSCompressor();

            if (args[0] == "-tpa")
            {
                string input = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, args[1]);

                using (FileStream fsIn = new FileStream(input, FileMode.Open, FileAccess.Read))
                using (FileStream fsOut = new FileStream(args[2], FileMode.Create, FileAccess.Write))
                {
                    cpc.PredictorTest_AnalyzeFile(fsIn, fsOut);
                }
            }
            else
            if (args[0] == "-c")
            {
                int slidingSize = 128;
                if (args.Length == 4)
                    slidingSize = int.Parse(args[3]);

                using (FileStream fsIn = new FileStream(args[1], FileMode.Open, FileAccess.Read))
                //using (FileStream fsOut = new FileStream("tmp", FileMode.Create, FileAccess.Write))
                //{
                //    vcs.Coding(fsIn, fsOut, slidingSize);
                //    fsOut.Flush();
                //}
                //Console.WriteLine(string.Format("C. Removed: {0}   Add: {1}", vcs.StatCharRemove, vcs.StatCharAdd));

                //vcs = new WCSCompressor();
                //using (FileStream fsIn = new FileStream("tmp", FileMode.Open, FileAccess.Read))
                using (FileStream fsOut = new FileStream(args[2], FileMode.Create, FileAccess.Write))
                {
                    vcs.Coding(fsIn, fsOut, slidingSize);
                    fsOut.Flush();
                }


                Console.WriteLine(string.Format("C. Removed: {0}   Add: {1}", vcs.StatCharRemove, vcs.StatCharAdd));
            }



            if (args[0] == "-c2")
            {
                int slidingSize = 128;
                if (args.Length == 4)
                    slidingSize = int.Parse(args[3]);

                long ticks = 0;


                using (FileStream fsIn = new FileStream(args[1], FileMode.Open, FileAccess.Read))

                //using (FileStream fsIn = new FileStream("tmp", FileMode.Open, FileAccess.Read))
                using (FileStream fsOut = new FileStream(args[2], FileMode.Create, FileAccess.Write))
                {
                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
                    cpc.Coding(fsIn, fsOut, slidingSize);
                    fsOut.Flush();
                    ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
                }

                FileInfo fi = new FileInfo(args[1]);
                WriteTimeRun(fi.Length, ticks);


            }





        }

        private static void WriteTimeRun(long fileSize, long ticks)
        {
            DateTime time = new DateTime(ticks);
            double kbperSec = fileSize / new TimeSpan(ticks).TotalSeconds;
            /* vypis vyslednych hodnot */
            Console.WriteLine("C Time : {0:D2} : {1:D2}.{2:D3}s  Speed: {3} Kb/s", time.Minute, time.Second, time.Millisecond, kbperSec / 1000);
        }


        static void TestSlidingWindow()
        {
            byte[] input = Encoding.UTF8.GetBytes("Ahoj ja jsem testovaci string a doufam ze se spravne prectu.");
            //byte[] input = Encoding.UTF8.GetBytes("jaoifuodjfahdfoodfuludflahjfhadfhjaoidfhoiajdfoaihdfoahdfojhdfahdfoahdfljahfohafalfja");


            WCSCompressor vcs = new WCSCompressor();

            string output = null;

            using (MemoryStream msOutput = new MemoryStream())
            using (MemoryStream msInput = new MemoryStream(input))
            {
                vcs.Coding(msInput, msOutput, 128);

                output = Encoding.UTF8.GetString(msOutput.ToArray());
            }

            Console.ReadKey();
        }



        public static void TestTrie()
        {
            const int countContext = 1000000;
            const int minContext = 2;
            const int maxContext = 10;
            const int alhpaSize = 64;

            byte[][] data = TrieTester.GenerateTestStrings(countContext, minContext, maxContext, alhpaSize);

            Dictionary<byte[], BinaryCounterLight> dic = new Dictionary<byte[], BinaryCounterLight>(new ByteArraryComparer());

            //  Console.WriteLine("str: {0:d} c: {1:d} - {2:d}   maxAlpha:{3}", countContext, minContext, maxContext, alhpaSize);
            GC.Collect();

            long ticks = 0;
            int found = 0;
            Console.WriteLine("Test Dic:");
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
            TrieTester.AddToDictionary(dic, data);
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
            Console.WriteLine("Add");
            WriteTimeRun(countContext, ticks);

            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
            found = TrieTester.SearchInDictionary(dic, data);
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
            Console.WriteLine("Hit: {0}", found);
            WriteTimeRun(countContext, ticks);

            dic = null;
            GC.Collect();

            TestTrie_Trie(data, countContext);

            TestTrie_Struct(data, countContext);
            TestTrie_StructFast(data, countContext);

        }

        private static void TestTrie_Trie(byte[][] data, int countContext)
        {
            long ticks = 0;
            int found = 0;
            // ------------------------------------------------------------------------------
            Trie<BinaryCounterLight> trie = new Trie<BinaryCounterLight>();
            Console.WriteLine("Test Trie:");
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
            TrieTester.AddTo_Trie(trie, data);
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
            Console.WriteLine("Add");
            WriteTimeRun(countContext, ticks);

            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
            found = TrieTester.SearchInTrie(trie, data);
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
            Console.WriteLine("Hit: {0}", found);
            WriteTimeRun(countContext, ticks);
        }

        private static void TestTrie_Struct(byte[][] data, int countContext)
        {
            long ticks = 0;
            int found = 0;
            GC.Collect(); 
            // ------------------------------------------------------------------------------
            TrieStruct<BinaryCounterLight> trieStruct = new TrieStruct<BinaryCounterLight>();
            Console.WriteLine("Test Trie:");
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
            TrieTester.AddTo_TrieStruct(trieStruct, data);
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
            Console.WriteLine("Add");
            WriteTimeRun(countContext, ticks);

            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
            found = TrieTester.SearchInTrieStruct(trieStruct, data);
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
            Console.WriteLine("Hit: {0}", found);
            WriteTimeRun(countContext, ticks);
        }

        private static void TestTrie_StructFast(byte[][] data, int countContext)
        {
            long ticks = 0;
            int found = 0;
            GC.Collect();
            // ------------------------------------------------------------------------------
            TrieStructFast<BinaryCounterLight> trieStruct = new TrieStructFast<BinaryCounterLight>();
            Console.WriteLine("Test Trie:");
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
            TrieTester.AddTo_TrieStructFast(trieStruct, data);
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
            Console.WriteLine("Add");
            WriteTimeRun(countContext, ticks);

            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks;
            found = TrieTester.SearchInTrieStructFast(trieStruct, data);
            ticks = Process.GetCurrentProcess().UserProcessorTime.Ticks - ticks;
            Console.WriteLine("Hit: {0}", found);
            WriteTimeRun(countContext, ticks);
        }

    }
}

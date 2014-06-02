using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCSCompress.Core;

namespace WCSCompresor
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestSlidingWindow();

            if (args.Length != 3 && args.Length != 4)
                return;

            WCSCompressor vcs = new WCSCompressor(); 
            

            if(args[0] == "-c")
            {
                int slidingSize = 128;
                if(args.Length == 4)
                    slidingSize = int.Parse(args[3]);

                using(FileStream fsIn = new FileStream(args[1],FileMode.Open, FileAccess.Read))
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
        }

        static void TestSlidingWindow()
        {
            byte[] input = Encoding.UTF8.GetBytes("Ahoj ja jsem testovaci string a doufam ze se spravne prectu.");
            //byte[] input = Encoding.UTF8.GetBytes("jaoifuodjfahdfoodfuludflahjfhadfhjaoidfhoiajdfoaihdfoahdfojhdfahdfoahdfljahfohafalfja");


            WCSCompressor vcs = new WCSCompressor(); 
            
            string output = null;

            using(MemoryStream msOutput = new MemoryStream())
            using (MemoryStream msInput = new MemoryStream(input))
            {
                vcs.Coding(msInput,msOutput, 128);
            
                output = Encoding.UTF8.GetString(msOutput.ToArray());
            }

            Console.ReadKey();
        }
    }
}

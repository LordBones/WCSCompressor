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
            TestSlidingWindow();
        }

        static void TestSlidingWindow()
        {
            byte[] input = Encoding.UTF8.GetBytes("Ahoj ja jsem testovaci string a doufam ze se spravne prectu.");



            SlidingWindow sw = new SlidingWindow(new MemoryStream(input), 8);
            LookupPredictor lp = new LookupPredictor();

            string output = null;

            using (MemoryStream ms = new MemoryStream())
            {
                while (sw.HasByteToRead())
                {
                    if (sw.GetCurrWindowSize() > 0)
                    {
                        if(!lp.IsSuccAlone(sw.GetWindowLastByte(),sw.GetByte()))
                            ms.WriteByte(sw.GetByte());
                    }

                    // update lookup
                    if(sw.GetCurrWindowSize() > 0)
                        lp.AddByte(sw.GetWindowLastByte(),sw.GetByte());

                    if (sw.GetCurrWindowSize() > 1)
                    {
                        if (!sw.WillWindowMove())
                            lp.RemoveByte(sw.GetWindowFirstByte(), sw.GetWindowByte(1));
                    }

                    sw.MoveWindow();
                }

                output = Encoding.UTF8.GetString(ms.ToArray());
            }

            Console.ReadKey();
        }
    }
}

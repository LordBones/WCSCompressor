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



            
            SlidingWindow sw = new SlidingWindow( 8);
            LookupPredictor lp = new LookupPredictor();

            string output = null;

            using (MemoryStream ms = new MemoryStream())
            using (MemoryStream msInput = new MemoryStream(input))
            {
                int data = msInput.ReadByte(); 

                while (data > 0)
                {
                    byte dataB = (byte)data;

                    if (sw.GetCurrWindowSize() > 1)
                    {
                        if(!lp.IsSuccAlone(sw.GetWindowLastByte(),dataB))
                            ms.WriteByte(dataB);
                    }

                    // update lookup
                    if(sw.GetCurrWindowSize() > 0)
                        lp.AddByte(sw.GetWindowLastByte(),dataB);

                    if (sw.GetCurrWindowSize() > 1)
                    {
                        if (sw.IsMaxSizeWindow())
                            lp.RemoveByte(sw.GetWindowFirstByte(), sw.GetWindowByte(1));
                    }

                    sw.AddByte(dataB);

                    data = msInput.ReadByte(); 
                }

                output = Encoding.UTF8.GetString(ms.ToArray());
            }

            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Script_Create
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "ServiceToken.exe";
            Console.WriteLine("Activating Create script");

            string[] files = Directory.GetFiles(@"D:\Program Files\Lumenis\ConnectWise_POC\ServiceToken", "*.exe");//D:\Program Files\Lumenis\ConnectWise_POC\ServiceToken
            if (files?.Length > 0)
            {

                try
                {
                    foreach (string file in files)
                    {
                       // Console.WriteLine("file {0} was found", file);
                       // Console.ReadKey();
                        if (file.Contains(fileName))
                        {
                            ProcessStartInfo pInfo = new ProcessStartInfo(file);
                            pInfo.Arguments = "r 10001";
                            Process.Start(pInfo);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                Console.WriteLine("no file was found");
            }

            Console.ReadKey();
        }
    }
}

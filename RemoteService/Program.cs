using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace LumenisRemoteService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            try
            {
                if (!System.IO.Directory.Exists(@"D:\Program Files\Lumenis\Remote Service\x86"))
                {

                    System.IO.Directory.CreateDirectory(@"D:\Program Files\Lumenis\Remote Service\x86");
                }

                if (!System.IO.File.Exists(@"D:\Program Files\Lumenis\Remote Service\x86\KernelTraceControl.dll"))
                {
                    //copy
                    System.IO.File.Copy(@"D:\Program Files\Lumenis\Remote Service\KernelTraceControl.dll", @"D:\Program Files\Lumenis\Remote Service\x86\KernelTraceControl.dll");
                    System.IO.File.Copy(@"D:\Program Files\Lumenis\Remote Service\KernelTraceControl.Win61.dll", @"D:\Program Files\Lumenis\Remote Service\x86\KernelTraceControl.Win61.dll");
                    System.IO.File.Copy(@"D:\Program Files\Lumenis\Remote Service\msdia140.dll", @"D:\Program Files\Lumenis\Remote Service\x86\msdia140.dll");
                }

            }
            catch
            {

            }

           
#if TestApp

            LumenisRemoteServiceApp.Start();
            Console.ReadKey();
#else



            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new LumenisService()
            };
            ServiceBase.Run(ServicesToRun);


#endif
        }

        private static void RegisterToGAC()
        {
            try
            {
                //System.EnterpriseServices.Internal.Publish gg = new Publish();

                //gg.GacRemove("NLog.dll");
                //gg.GacRemove("COM.dll");
                //gg.GacRemove("Interfaces.dll");
                //gg.GacRemove("Logging.dll");
                //gg.GacRemove("Remote ServiceRemoteServiceApi.dll");
                //gg.GacInstall("NLog.dll");
                //gg.GacInstall("COM.dll");
                //gg.GacInstall("Interfaces.dll");
                //gg.GacInstall("Logging.dll");
                //gg.GacInstall("RemoteServiceApi.dll");
                //return;

                Process p = new Process();
                ProcessStartInfo pStart = new ProcessStartInfo("gacutil.exe");
                string filePath = @"""D:\Program Files\Lumenis\Remote Service\NLog.dll""";
                pStart.Arguments = @"/i " + filePath;
               // pStart.WindowStyle = ProcessWindowStyle.Hidden;
                pStart.UseShellExecute = false;

                p.StartInfo = pStart;
                p.Start();

                p = new Process();

                pStart = new ProcessStartInfo("gacutil.exe");
                filePath = @"""D:\Program Files\Lumenis\Remote Service\COM.dll""";
                pStart.Arguments = @"/i " + filePath;
               // pStart.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo = pStart;
                p.Start();

                p = new Process();

                pStart = new ProcessStartInfo("gacutil.exe");
                filePath = @"""D:\Program Files\Lumenis\Remote Service\Interfaces.dll""";
                pStart.Arguments = @"/i " + filePath;
               // pStart.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo = pStart;
                p.Start();

                p = new Process();

                pStart = new ProcessStartInfo("gacutil.exe");
                filePath = @"""D:\Program Files\Lumenis\Remote Service\Logging.dll""";
                pStart.Arguments = @"/i " + filePath;
               // pStart.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo = pStart;
                p.Start();


                p = new Process();

                pStart = new ProcessStartInfo("gacutil.exe");
                filePath = @"""D:\Program Files\Lumenis\Remote Service\RemoteServiceApi.dll""";
                pStart.Arguments = @"/i " + filePath;
                //pStart.WindowStyle = ProcessWindowStyle.Hidden;



                p.StartInfo = pStart;
                p.Start();
            }
            catch (Exception ex)
            {


            }
        }
    }
}

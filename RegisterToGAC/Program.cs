using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.EnterpriseServices.Internal;

namespace RegisterToGAC
{
    class Program
    {
        static void Main(string[] args)
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
              //  pStart.WindowStyle = ProcessWindowStyle.Hidden;
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
                //pStart.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo = pStart;
                p.Start();


                p = new Process();

                pStart = new ProcessStartInfo("gacutil.exe");
                filePath = @"""D:\Program Files\Lumenis\Remote Service\RemoteServiceApi.dll""";
                pStart.Arguments = @"/i " + filePath;
               // pStart.WindowStyle = ProcessWindowStyle.Hidden;


                p.StartInfo = pStart;
                p.Start();
            }
            catch (Exception ex)
            {

              
            }




          
            Console.ReadLine();
        }
    }
}

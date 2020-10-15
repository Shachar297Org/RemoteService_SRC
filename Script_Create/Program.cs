using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Diagnostics.Tracing.Parsers;
using Logging;

namespace Script_Create
{
    class Program
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            
            //string fileName = "ServiceToken.exe";
            //Console.WriteLine("Activating Create script");

            //string[] files = Directory.GetFiles(@"D:\Program Files\Lumenis\ConnectWise_POC\ServiceToken", "*.exe");//D:\Program Files\Lumenis\ConnectWise_POC\ServiceToken
            //if (files?.Length > 0)
            //{

            //    try
            //    {
            //        foreach (string file in files)
            //        {
            //           // Console.WriteLine("file {0} was found", file);
            //           // Console.ReadKey();
            //            if (file.Contains(fileName))
            //            {
            //                ProcessStartInfo pInfo = new ProcessStartInfo(file);
            //                pInfo.Arguments = "r 10001";
            //                Process.Start(pInfo);
            //                return;
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex);
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("no file was found");
            //}

            try
            {
                NetworkPerformanceReporter.Create();
            }
            catch(Exception ex)
            {
                Logger.Error(ex);
            }

            Console.ReadKey();
        }
    }

    public sealed class NetworkPerformanceReporter : IDisposable
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        private DateTime m_EtwStartTime;
        private TraceEventSession m_EtwSession;

        private readonly Counters m_Counters = new Counters();

        private class Counters
        {
            public long Received;
            public long Sent;
        }

        private NetworkPerformanceReporter() { }

        public static NetworkPerformanceReporter Create()
        {
            var networkPerformancePresenter = new NetworkPerformanceReporter();
            networkPerformancePresenter.Initialise();
            return networkPerformancePresenter;
        }

        private void Initialise()
        {
            // Note that the ETW class blocks processing messages, so should be run on a different thread if you want the application to remain responsive.
            Task.Run(() => StartEtwSession());
        }

        private void StartEtwSession()
        {
            try
            {
                Process[] process = Process.GetProcessesByName("ScreenConnect.ClientService");
                if(process.Length <1)
                {
                    Logger.Error("process not found");
                    return;
                }
                var processId = process[0].Id;
                ResetCounters();

                using (m_EtwSession = new TraceEventSession("MyKernelAndClrEventsSession"))
                {
                    m_EtwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

                    m_EtwSession.Source.Kernel.TcpIpRecv += data =>
                    {
                        if (data.ProcessID == processId)
                        {
                            lock (m_Counters)
                            {
                                m_Counters.Received += data.size;
                                Logger.Information("received size is : {0}",m_Counters.Received.ToString());
                                
                            }
                        }
                    };

                    m_EtwSession.Source.Kernel.TcpIpSend += data =>
                    {
                        if (data.ProcessID == processId)
                        {
                            lock (m_Counters)
                            {
                                m_Counters.Sent += data.size;
                                Logger.Information("sent size is : {0}", m_Counters.Received.ToString());
                            }
                        }
                    };

                    m_EtwSession.Source.Process();
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex);
                ResetCounters(); // Stop reporting figures
                // Probably should log the exception
            }
        }

        public NetworkPerformanceData GetNetworkPerformanceData()
        {
            var timeDifferenceInSeconds = (DateTime.Now - m_EtwStartTime).TotalSeconds;

            NetworkPerformanceData networkData;

            lock (m_Counters)
            {
                networkData = new NetworkPerformanceData
                {
                    BytesReceived = Convert.ToInt64(m_Counters.Received / timeDifferenceInSeconds),
                    BytesSent = Convert.ToInt64(m_Counters.Sent / timeDifferenceInSeconds)
                };

            }

            // Reset the counters to get a fresh reading for next time this is called.
            ResetCounters();

            return networkData;
        }

        private void ResetCounters()
        {
            lock (m_Counters)
            {
                m_Counters.Sent = 0;
                m_Counters.Received = 0;
            }
            m_EtwStartTime = DateTime.Now;
        }

        public void Dispose()
        {
            m_EtwSession?.Dispose();
        }
    }

    public sealed class NetworkPerformanceData
    {
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
    }
}

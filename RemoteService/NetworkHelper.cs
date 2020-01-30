using Logging;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace LumenisRemoteService
{
    class NetworkHelper
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();



        public static string GetEthernetAddress()
        {
            List<IPAddress> gatewaysList = null;
            string Address = "0.0.0.0";
            try
            {
                string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                // Get the IP
                IPAddress[] list = Dns.GetHostByName(hostName).AddressList;
                string routerAddress = GetLanGateWayAddress().Substring(0, 6);
                if (list.Any(x => x.ToString().Contains(routerAddress)))
                {
                    list.FirstOrDefault(x => x.ToString().Contains(routerAddress));
                    Address = list.FirstOrDefault(x => x.ToString().Contains(routerAddress)).ToString();
                }
                else
                {
                     Logger.Warning("no proper local IP address that matches gateway address");
                }

                return Address;

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Address;
            }
        }
        private static string GetLanGateWayAddress()//todo what if there are more than 1??
        {
            List<IPAddress> addresses = GetLocalAddresses(false);

            foreach (IPAddress addr in addresses)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return addr.ToString();
                }
            }


            //if first method fail try the second one
            List<GatewayIPAddressInformation> list = GetGatwayAddress();
            try
            {
                foreach (GatewayIPAddressInformation gate in list)
                {

                    if (gate.Address.AddressFamily.ToString() == "InterNetwork")
                    {
                        return gate.Address.ToString();
                    }
                }
                // Logger.Warning("fail to find proper lan gateway");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return string.Empty;
        }

        private static List<System.Net.IPAddress> GetLocalAddresses(bool includeIPv6)
        {
            List<System.Net.IPAddress> addresses = new List<System.Net.IPAddress>();

            System.Net.IPHostEntry hostInfo = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress address in hostInfo.AddressList)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ||
                    (includeIPv6 && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6))
                {
                    addresses.Add(address);
                }
            }

            return addresses;
        }

        private static List<GatewayIPAddressInformation> GetGatwayAddress()
        {
            List<GatewayIPAddressInformation> gatewaysList = null;
            try
            {
                gatewaysList = new List<GatewayIPAddressInformation>();
                var cards = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        var address = card.GetIPProperties().GatewayAddresses;
                        if (address != null)
                        {
                            if (address.Count > 0)
                            {
                                // foreach(GatewayIPAddressInformation ip in address)
                                // {
                                // if(ip.Address.AddressFamily.ToString() == "InterNetwork")
                                // {
                                // return ip.Address.ToString();
                                // }
                                // }

                                System.Net.NetworkInformation.GatewayIPAddressInformation gateway = address[0];
                                gatewaysList.Add(gateway);
                            }
                        }
                    }
                    if (gatewaysList.Count == 0)
                    {
                        //to do: notify of empty gateway and handle user selection
                    }
                    if (gatewaysList.Count > 1)
                    {
                        //to do: notify of too many interfaces gateways and handle user selection
                    }
                }
                return gatewaysList;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return gatewaysList;
            }
        }


        private static bool _trafficDetected = false;

        public static bool TrafficDetected
        {
            get { return _trafficDetected; }
            set { _trafficDetected = value; }
        }

        public static bool _activated = false;

        public static void CheckIfSessionEstablished()
        {
            try
            {
                int port = 443; //<--- This is your value
               // bool isAvailable = true;
                
                // Evaluate current system tcp connections. This is the same information provided
                // by the netstat command line application, just in .Net strongly-typed object
                // form.  We will look through the list, and if our port we would like to use
                // in our TcpClient is occupied, we will set isAvailable to false.
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
             // TcpStatistics statistics =  ipGlobalProperties.GetTcpIPv4Statistics();
                
                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.RemoteEndPoint.Port == port)
                    {
                        
                        Logger.Debug("port 443 state is {0}",tcpi.State.ToString());
                        if(tcpi.State == TcpState.Established)
                        {
                            if (!_activated)
                            {
                                Logger.Debug("traffic monitor activated");
                                NetworkPerformanceReporter.Create();
                                NetworkPerformanceReporter.TracfficDetected += NetworkPerformanceReporter_TracfficDetected;
                                _activated = true;
                            }

                           // isAvailable = false;
                            break;
                        }
                        //else if(tcpi.State == TcpState.TimeWait)
                        //{
                        //    _activated = false;
                       // }
                        
                    }
                }
              //  return isAvailable;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        private static void NetworkPerformanceReporter_TracfficDetected(bool obj)
        {
            try
            {
                _trafficDetected = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }

    class NetworkPerformanceReporter : IDisposable
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        private DateTime m_EtwStartTime;
        private TraceEventSession m_EtwSession;

        private readonly Counters m_Counters = new Counters();

        public static  event Action<bool> TracfficDetected;

        private class Counters
        {
            public long Received;
            public long Sent;
        }

       // private NetworkPerformanceReporter() { }

        public static NetworkPerformanceReporter Create()
        {

            try
            {
                Logger.Debug("Create");
                var networkPerformancePresenter = new NetworkPerformanceReporter();
                networkPerformancePresenter.Initialise();
                return networkPerformancePresenter;
            }
            catch (Exception ex)
            {

                Logger.Error(ex);
                return null;
            }
        }

        private void Initialise()
        {
            Logger.Debug("Initialise");

            // Note that the ETW class blocks processing messages, so should be run on a different thread if you want the application to remain responsive.
            Task.Run(() => StartEtwSession());
        }

        private void StartEtwSession()
        {
            try
            {

                Logger.Debug("StartEtwSession");
                Process[] process = Process.GetProcessesByName("ScreenConnect.ClientService");
                if (process.Length < 1)
                {
                    Logger.Error("process not found");
                    return;
                }
                var processId = process[0].Id;
                ResetCounters();
                string eventSessionName = string.Empty;
               if(Environment.OSVersion.VersionString.Contains("6.2"))
                {
                    //win 10 embedded
                    eventSessionName = "MyKernelAndClrEventsSession";
                }
                else
                {
                    //win 7 embedded
                    eventSessionName = "NT Kernel Logger";
                }
                Logger.Information(string.Format("event session name is {0}",eventSessionName));
                using (m_EtwSession = new TraceEventSession(eventSessionName))
                {

                 
                    try
                    {
                        //var name = KernelTraceEventParser.KernelSessionName;
                       // Logger.Information(string.Format("trace event name is : {0}", name));
                        m_EtwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP,KernelTraceEventParser.Keywords.None);
                    
                    
                        m_EtwSession.Source.Kernel.TcpIpRecv += data =>
                        {
                            if (data.ProcessID == processId)
                            {
                                lock (m_Counters)
                                {
                                   // if(m_Counters.Received != data.size)
                                   // {
                                        m_Counters.Received += data.size;
                                        Logger.Information("received size is : {0}", m_Counters.Received.ToString());
                                        TracfficDetected(true);
                                   // }
                                   

                                }
                            }
                        };

                        //m_EtwSession.Source.Kernel.TcpIpSend += data =>
                        //{
                        //    if (data.ProcessID == processId)
                        //    {
                        //        lock (m_Counters)
                        //        {
                        //            if (m_Counters.Sent != data.size)
                        //            {
                        //                m_Counters.Sent += data.size;
                        //                Logger.Information("sent size is : {0}", m_Counters.Received.ToString());
                        //                TracfficDetected(true);
                        //            }

                        //        }
                        //    }
                        //};

                        m_EtwSession.Source.Process();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
            catch (Exception ex)
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



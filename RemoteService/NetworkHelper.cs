using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace LumenisRemoteService
{
    class NetworkHelper
    {
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
                    // Logger.Warning("no proper local IP address that matches gateway address");
                }

                return Address;

            }
            catch (Exception ex)
            {

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

                return gatewaysList;
            }
        }


        public static bool CheckIfSessionEstablished()
        {
            int port = 443; //<--- This is your value
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }
    }
}

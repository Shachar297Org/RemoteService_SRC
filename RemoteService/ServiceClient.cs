using Logging;
using Lumenis.RemoteServiceApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumenisRemoteService
{
    class ServiceClient
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        RemoteAPI remoteApi = null;

        public ServiceClient()
        {
            try
            {
                remoteApi = new RemoteAPI();
                remoteApi.StartClient();
                remoteApi.StopScreenConnect();
                Logger.Information("internal client stop ScreenConnect service");//user case 3
            }
            catch (Exception ex)
            {
                Logger.Error("internal client failed {0}",ex);            }
        }
    }
}

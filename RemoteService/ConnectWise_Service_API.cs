using Interfaces;
using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace LumenisRemoteService
{
    
   

    /// <summary>
    /// command interface between request support client to connectWise service controller\monitor
    /// </summary>
    public partial class RemoteService : IRemoteService
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        ConnectWiseController _wiseControl = new ConnectWiseController();
       
        public bool StartScreenConnect()
        {
            Logger.Debug("StartScreenConnect");
              return  _wiseControl.Open();
          
        }

        public bool StopService()
        {
            Logger.Debug("StopService");
               return _wiseControl.Close();
           
            
        }

        public ScreeenConnectServiceStatus GetScreenConnectStatus()
        {
            Logger.Debug("GetScreenConnectStatus");
            return _wiseControl.ServiceStatus;
        }

        public ScreenConnectSessionStatus GetSessiontStatus()
        {
            return _wiseControl.SessionStatus;
        }

       

    }
}

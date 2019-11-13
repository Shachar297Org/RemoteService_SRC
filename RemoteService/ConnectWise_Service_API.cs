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
    /// commnad interface between request support client to connectWise service controller\monitor
    /// </summary>
    public partial class RemoteService : IRemoteService
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        ConnectWiseController _wiseControl = new ConnectWiseController();
        //[OperationContract]
        public bool StartScreenConnect()
        {
            Logger.Debug("StartScreenConnect");
           // if(_wiseControl.DetectService())
            //{
              return  _wiseControl.Open();
           // }
            return false;
            //ActionDispatcher.Instance.StartService();
        }

        public bool StopService()
        {
            Logger.Debug("StopService");
           // if (_wiseControl.DetectService())
           // {
               return _wiseControl.Close();
           // }
            //else
            //{
            //    return false;
           //}
            
        }

        public ScreeenConnectServiceStatus GetScreenConnectStatus()
        {
            Logger.Debug("GetScreenConnectStatus");
            _wiseControl.UserAppISActive();
            return _wiseControl.ServiceStatus;
        }

        public ScreeenConnectSessionStatus GetSessiontStatus()
        {
            return _wiseControl.SessionStatus;
        }

       
        public void RenewSessionLimit()
        {
            Logger.Debug("RenewSessionLimit");
            _wiseControl.RenewSessionTimer();
        }

       
        public TimeSpan SessionTimeLeft()
        {
            Logger.Debug("SessionTimeLeft");
            return _wiseControl.SessionTimeLeft();
        }


    }
}

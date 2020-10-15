using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lumenis.RemoteServiceApi
{
    public partial class RemoteAPI : IDisposable
    {
        public bool StartScreenConnect()
        {
          return  _remoteService.StartScreenConnect();
            //ActionDispatcher.Instance.StartService();

        }

        public bool StopScreenConnect()
        {
            return _remoteService.StopService();
        }

        public ScreeenConnectServiceStatus GetScreenConnectStatus()
        {
            return _remoteService.GetScreenConnectStatus();
        }

        public ScreenConnectSessionStatus GetSessionStatus()
        {
            return _remoteService.GetSessiontStatus();
        }

        //public void RenewSessionLimit()
        //{
        //     _remoteService.RenewSessionLimit();
        //}

        //public TimeSpan GetRemainingTime()
        //{
        //    return _remoteService.SessionTimeLeft();
        //}

    }
    
}

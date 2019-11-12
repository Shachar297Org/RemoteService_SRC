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

        public ScreeenConnectServiceStatus GetScreenConnectStatus()
        {
            return _remoteService.GetScreenConnectStatus();
        }
    }
    //class ConnectWise_RemoteAPI
    //{
    //}
}

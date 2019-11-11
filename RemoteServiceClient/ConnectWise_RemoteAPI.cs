using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lumenis.RemoteServiceApi
{
    public partial class RemoteAPI : IDisposable
    {
        public void StartScreenConnect()
        {
            _remoteService.StartScreenConnect();
            //ActionDispatcher.Instance.StartService();

        }

        public string GetScreenConnectStatus()
        {
            return _remoteService.GetScreenConnectStatus();
        }
    }
    //class ConnectWise_RemoteAPI
    //{
    //}
}

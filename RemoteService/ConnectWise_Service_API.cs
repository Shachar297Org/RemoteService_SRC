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
        ConnectWiseController _wiseControl = new ConnectWiseController();
        //[OperationContract]
        public bool StartScreenConnect()
        {
            if(_wiseControl.DetectService())
            {
              return  _wiseControl.Open();
            }
            return false;
            //ActionDispatcher.Instance.StartService();
        }

        public bool StopService()
        {
            if (_wiseControl.DetectService())
            {
                _wiseControl.MonitorSession();
            }
            _wiseControl.Close();
            return false;
        }

        public string GetScreenConnectStatus()
        {
            return "under construction";
        }


    }
}

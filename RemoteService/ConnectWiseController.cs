using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LumenisRemoteService
{
    /// <summary>
    /// Control and monitor connectWise service and active session
    /// </summary>
    /// 

    class ConnectWiseController
    {
        /// <summary>
        /// Detect ScreenConnect service in run state. if in stopped state DetectService will return false.
        /// </summary>
        /// <returns></returns>
        public bool DetectService()
        {
            return false;
        }

        /// <summary>
        /// if screen connect service exist turn it to run state
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            return false;
        }

        /// <summary>
        /// stop screen connect service id exist
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            return false;
        }

        /// <summary>
        /// Monitor if port 443 is in used and it's traffic level
        /// </summary>
        public void MonitorSession()
        {

        }
    }
}

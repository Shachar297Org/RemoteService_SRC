using Interfaces;
//using Logging;
using Lumenis.RemoteServiceApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace COM
{
   // [Guid("8B107015-4483-4E62-B904-A7319B6879E5")]
    //[ComVisible(true)]
   // [ClassInterface(ClassInterfaceType.None)]
   // [ProgId("ProgId.Class1")]
    public class ComLib : ICom
    {
        public delegate void StatusDel(StatusEvent p_status);
       // private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        RemoteAPI remoteApi = null;
        System.Timers.Timer _timer = new System.Timers.Timer(3000);//this timer also for getting the status but also for keep alive check
        ScreeenConnectSessionStatus _lastStatus = ScreeenConnectSessionStatus.None;
        public event StatusDel _newStatusArrived;

        public ComLib()
        {
            remoteApi = new RemoteAPI();
           // remoteApi.StartClient();
            remoteApi.Open(); 
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
           // Logger.Debug("starting client channel");
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                GetStatus();//todo should only be performed if user app is active and not minimized
            }
            catch (Exception ex)
            {
               // Logger.Error(ex);
            }
        }

        private void GetStatus()
        {
            var currentStatus = remoteApi.GetSessionStatus();
            if (currentStatus != _lastStatus)
            {
                _newStatusArrived?.Invoke(new StatusEvent(currentStatus));
            }

        }

        public bool RequestSupport()
        {

            return remoteApi.StartScreenConnect();
           // var result = remoteApi.GetScreenConnectStatus();
            // ServiceStatus = ConvertServiceStatusEnum(result);

        }

        public bool StopSupport()
        {
           return remoteApi.StopScreenConnect();
        }
    }

    public class StatusEvent:EventArgs
    {
        public ScreeenConnectSessionStatus Status { get; private set; }

        public StatusEvent(ScreeenConnectSessionStatus p_stat)
        {
            Status = p_stat;
        }
    }
}

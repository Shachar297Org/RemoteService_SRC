using Interfaces;
//using Logging;
using Lumenis.RemoteServiceApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace COMM
{
   
    public class ComLib : ICom
    {
        public delegate void StatusDel(StatusEvent p_status);
        RemoteAPI remoteApi = null;
        System.Timers.Timer _timer = new System.Timers.Timer(3000);//this timer also for getting the status but also for keep alive check
        public event StatusDel _newStatusArrived;

        public ComLib()
        {
            remoteApi = new RemoteAPI();
            remoteApi.Open(); 
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                GetStatus();//todo should only be performed if user app is active and not minimized
            }
            catch (Exception ex)
            {
            }
        }

        private void GetStatus()
        {
            var currentStatus = remoteApi.GetSessionStatus();
                _newStatusArrived?.Invoke(new StatusEvent(currentStatus));
        }

        public bool RequestSupport()
        {

            try
            {
                return remoteApi.StartScreenConnect();
            }
            catch 
            {

                return false;
            }
        }

        public bool StopSupport()
        {
            try
            {
                return remoteApi.StopScreenConnect();
            }
            catch
            {
                return false;
            }
        }
    }

    public class StatusEvent:EventArgs
    {
        public ScreenConnectSessionStatus Status { get; private set; }

        public StatusEvent(ScreenConnectSessionStatus p_stat)
        {
            Status = p_stat;
        }
    }
}

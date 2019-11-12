using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;

namespace LumenisRemoteService
{
    /// <summary>
    /// Control and monitor connectWise service and active session
    /// </summary>
    /// 

    class ConnectWiseController
    {

        private readonly string JUMP_CLIENT_SERVICE_NAME_PREFIX;
        private ServiceController _service = null;
        private bool _serviceInstalled = false;
        private bool _requestForSupportWasMade = false; //signal if the user app sent request for support
        private bool _userAppIsRunning = false;

        #region Session monitoring flag

        private bool _receivedIp = false;
        private bool _isPortOpened = false;

        #endregion

        private System.Diagnostics.Stopwatch _sessionWatch = new System.Diagnostics.Stopwatch();
        private System.Timers.Timer _timer;
        private System.Timers.Timer _sessionReadyLimittimer;// restrict the time frame which the device can be exposed and be ready to session connection
        private readonly double MONITORINTERVAL = new TimeSpan(0, 0, 10).TotalMilliseconds;
        private readonly double SESSIONLIMITINTERVAL = new TimeSpan(0, 5, 0).TotalMilliseconds;

        public ScreeenConnectServiceStatus ServiceStatus { get; private set; } = ScreeenConnectServiceStatus.None;
        public ScreeenConnectSessionStatus SessionStatus { get; private set; } = ScreeenConnectSessionStatus.None;


        public ConnectWiseController()
        {
            JUMP_CLIENT_SERVICE_NAME_PREFIX = "ScreenConnect";//todo name should also be fetched from configuration file



            _timer = new System.Timers.Timer(MONITORINTERVAL);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();

            _sessionReadyLimittimer = new System.Timers.Timer(SESSIONLIMITINTERVAL);
            _sessionReadyLimittimer.Elapsed += _sessionReadyLimittimer_Elapsed;
            

            //ServiceController[] _services = ServiceController.GetServices();
            _service = ServiceController.GetServices().
                   FirstOrDefault(s => s.ServiceName.ToLower().StartsWith(JUMP_CLIENT_SERVICE_NAME_PREFIX));


            if (_service == null)
            {
                _serviceInstalled = false;
            }
            else
            {
                _serviceInstalled = true;
            }

        }

        private void _sessionReadyLimittimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _sessionReadyLimittimer.Stop();
                _requestForSupportWasMade = false;// will terminate Screen connect service
                _sessionReadyLimittimer.Start();
            }
            catch( Exception ex)
            {

            }
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                MonitorServiceStatus();
                MonitorSession();
                _timer.Start();
            }
            catch (Exception ex)
            {

                
            }
            
        }


        /// <summary>
        /// Detect ScreenConnect service in run state. if in stopped state DetectService will return false.
        /// </summary>
        /// <returns></returns>
        public bool DetectService()
        {
            
            if(_service!= null && _service.Status == ServiceControllerStatus.Running)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// if screen connect service exist turn it to run state
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            try
            {
                if (_service != null && _service.Status == ServiceControllerStatus.Paused || _service.Status == ServiceControllerStatus.Stopped)
                {
                    _service.Start();
                    _requestForSupportWasMade = true;
                    _sessionReadyLimittimer.Start();
                    //System.Threading.Thread.Sleep(10000);
                    return true;
                }
                else // if service status is other than the both above
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public void RenewSessionTimer()
        {
            if(SessionStatus == ScreeenConnectSessionStatus.SessionInStandby || SessionStatus == ScreeenConnectSessionStatus.SessionIsActive)
            {
                _requestForSupportWasMade = true;
                _sessionReadyLimittimer.Start();
                _sessionWatch.Reset();
                _sessionWatch.Start();
            }
          
        }

        public TimeSpan SessionTimeLeft()
        {
           return _sessionWatch.Elapsed;
            
        }

        /// <summary>
        /// stop screen connect service id exist
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            try
            {
                if (_service != null && _service.Status == ServiceControllerStatus.Running)
                {
                    _requestForSupportWasMade = false;
                    _sessionReadyLimittimer.Stop();
                    _service.Stop();
                    //System.Threading.Thread.Sleep(10000);
                    return true;
                }
                else // if service status is other than the both above
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        

       

        /// <summary>
        /// Monitor the service status itself
        /// </summary>
        private void MonitorServiceStatus()
        {
            if(_serviceInstalled)
            {
                
                _service.Refresh();
                if (_service.Status == ServiceControllerStatus.Running)
                {
                    if(!_requestForSupportWasMade || !_userAppIsRunning)// if the service is running (online on dashboard) but no request for support was made. or if user app is down
                    {
                        _service.Stop();
                        System.Threading.Thread.Sleep(5000);
                        return;//wait of the next timer tick
                    }
                    ServiceStatus = ScreeenConnectServiceStatus.Running;
                }
                else if(_service.Status == ServiceControllerStatus.Stopped)
                {
                    ServiceStatus = ScreeenConnectServiceStatus.Stopped;
                }
                else
                {
                    ServiceStatus = ScreeenConnectServiceStatus.Unstable;
                }
            }
            else
            {
                ServiceStatus = ScreeenConnectServiceStatus.NotInstalled;
            }
            _userAppIsRunning = false; // set the user app activity flag to false
        }

       

        internal void UserAppISActive()
        {
            _userAppIsRunning = true;
        }

        #region Session monitoring

        /// <summary>
        /// Monitor if port 443 is in used and it's traffic level
        /// </summary>
        public void MonitorSession()
        {
            var result = NetworkHelper.GetEthernetAddress();
            if (result != null && result != string.Empty && result != "0.0.0.0")
            {
                _receivedIp = true;
            }

            if(_receivedIp)//check if port 443 is in used
            {
                _isPortOpened = NetworkHelper.CheckIfSessionEstablished();
                if(!_isPortOpened)
                {
                    SessionStatus = ScreeenConnectSessionStatus.SessionIsActive;
                }
                else
                {
                    SessionStatus = ScreeenConnectSessionStatus.SessionInStandby;
                }
            }
            else
            {
                SessionStatus = ScreeenConnectSessionStatus.CableDisconnected;
            }
        }


        #endregion 
    }
}

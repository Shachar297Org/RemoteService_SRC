﻿using Interfaces;
using Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        private readonly string JUMP_CLIENT_SERVICE_NAME_PREFIX;
        private ServiceController _service = null;
        private bool _serviceInstalled = false;
        private bool _requestForSupportWasMade = false; //signal if the user app sent request for support
                                                        // private bool _userAppIsRunning = false;
        private bool _sessionWasActiveOnce;// check if session took place
        private object _syncObj = new object();

        #region Session monitoring flag

        private bool _receivedIp = false;

        #endregion

        private System.Timers.Timer _timer;
       

        private System.Timers.Timer _trafficMonitoringTimer;


        private readonly double SERVIICEMONITORINTERVAL = new TimeSpan(0, 0, 10).TotalMilliseconds;
        private readonly double TRAFFICMONITORINTERVAL;// = new TimeSpan(0, 0, 5).TotalMilliseconds;
       // private readonly double SESSIONLIMITINTERVAL = new TimeSpan(0, 5, 0).TotalMilliseconds;
      //  private readonly double TRAFFICMONITORINGINTERVAL = new TimeSpan(0, 0, 40).TotalMilliseconds;
       

        public ScreeenConnectServiceStatus ServiceStatus { get; private set; } = ScreeenConnectServiceStatus.None;
        public ScreenConnectSessionStatus SessionStatus { get; private set; } = ScreenConnectSessionStatus.None;

        int _counter = 1;
        readonly int _maxCounter = 0; //0.5 seconds * 12 equal 60 seconds

        public ConnectWiseController()
        {
            try
            {
                var sessioncounter = Convert.ToString(ConfigurationManager.AppSettings["SessionSampelingCounter"]);
                var sessionInterval = Convert.ToString(ConfigurationManager.AppSettings["SessionSampelingInterval"]);
                if (int.TryParse(sessioncounter, out _maxCounter))
                {
                    Logger.Error(string.Format("failed to parse session interval counter with value of {0}", sessioncounter));
                }
                if (!int.TryParse(sessionInterval, out int interval))
                {
                    Logger.Error(string.Format("failed to parse session interval with value of {0}", sessionInterval));
                }


                TRAFFICMONITORINTERVAL = new TimeSpan(0, 0, interval).TotalMilliseconds;



                JUMP_CLIENT_SERVICE_NAME_PREFIX = "ScreenConnect Client";//todo name should also be fetched from configuration file

                Logger.Information("searching for service name {0}", JUMP_CLIENT_SERVICE_NAME_PREFIX);

                _timer = new System.Timers.Timer(SERVIICEMONITORINTERVAL);
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();

                _trafficMonitoringTimer = new System.Timers.Timer(TRAFFICMONITORINTERVAL);
                _trafficMonitoringTimer.Elapsed += _trafficMonitoringTimer_Elapsed;

                _service = ServiceController.GetServices().
                       FirstOrDefault(s => s.DisplayName.StartsWith(JUMP_CLIENT_SERVICE_NAME_PREFIX));


                if (_service == null)
                {
                    Logger.Warning("service not installed");
                    _serviceInstalled = false;
                }
                else
                {
                    Logger.Information("service installed");
                    _serviceInstalled = true;
                }
                SessionStatus = ScreenConnectSessionStatus.SessionDisconnected;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }


        }

      
        private void _trafficMonitoringTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
           
            if (NetworkHelper.TrafficDetected)
            {
                NetworkHelper.TrafficDetected = false;
                lock (_syncObj)
                {
                    SessionStatus = ScreenConnectSessionStatus.SessionIsActive;
                    _counter = 1;
                }
            }
            else
            {
                if (_counter < _maxCounter)
                {
                    lock (_syncObj)
                    {
                        SessionStatus = ScreenConnectSessionStatus.SessionInStandby;
                        _counter++;
                    }
                }
                else
                {
                    // thereis no traffic between client and server
                    Logger.Debug("closing service because of traffic inactivity. inactivity counter is {0}", _counter);
                    lock (_syncObj)
                    {
                        SessionStatus = ScreenConnectSessionStatus.SessionDisconnected;
                        _counter = 1;
                    }

                    Close();
                }

                Logger.Debug(string.Format("traffic counter value is {0}",_counter));
            }
             
        }


        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                MonitorServiceStatus();
                if (ServiceStatus == ScreeenConnectServiceStatus.Running)
                {
                    MonitorSession(); 
                }
              
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                _timer.Start();
            }
            
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
                    Logger.Debug("starting ScreenConnect service");
                    _service.Start();//user case 1
                    NetworkHelper._activated = false;
                    _sessionWasActiveOnce = false;//reset the flag each time user request support or stop the service
                    lock (_syncObj)
                    {
                        _requestForSupportWasMade = true; 
                    }
                    _trafficMonitoringTimer.Start();
                    return true;
                }
                else // if service status is other than the both above
                {
                    Logger.Warning("didn't start the service. service installation status is {0}. service operation status is {1}",_service == null?"not installed": "installed", _service?.Status);
                    return false;
                }
            }
            catch (Exception ex)
            {

                Logger.Error(ex);
                return false;
            }
            
        }

       

        /// <summary>
        /// stop screen connect service id exist
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            try
            {
                SessionStatus = ScreenConnectSessionStatus.SessionDisconnected;
                if (_service != null && _service.Status == ServiceControllerStatus.Running)
                {
                    Logger.Debug("stop service command");
                    lock (_syncObj)
                    {
                        _requestForSupportWasMade = false; 
                    }
                    _trafficMonitoringTimer.Stop();
                    _service.Stop();//user case 3
                    _sessionWasActiveOnce = false;//reset the flag each time user request support or stop the service
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
            try
            {
                ScreeenConnectServiceStatus _lastStatus = ServiceStatus;
                bool statusChanged = false;
                lock (_syncObj)
                {
                    if (_serviceInstalled)
                    {

                        _service.Refresh();

                        if (_service.Status == ServiceControllerStatus.Running)
                        {
                            if (!_requestForSupportWasMade)// if the service is running (online on dashboard) but no request for support was made. or if user app is down
                            {
                                Logger.Debug("stopping service because request support time frame was over or because user app not running. request flag {0}", _requestForSupportWasMade);
                                Close();
                                System.Threading.Thread.Sleep(5000);
                                return;//wait of the next timer tick
                            }
                            if (_lastStatus != ScreeenConnectServiceStatus.Running)
                            {
                                statusChanged = true;
                                ServiceStatus = ScreeenConnectServiceStatus.Running;
                            }
                        }
                        else if (_service.Status == ServiceControllerStatus.Stopped)
                        {
                            if (_lastStatus != ScreeenConnectServiceStatus.Stopped)
                            {
                                statusChanged = true;
                                ServiceStatus = ScreeenConnectServiceStatus.Stopped;
                            }
                        }
                        else
                        {
                            if (_lastStatus != ScreeenConnectServiceStatus.Unstable)
                            {
                                statusChanged = true;
                                ServiceStatus = ScreeenConnectServiceStatus.Unstable;
                            }
                        }
                    }
                    else
                    {
                        if (_lastStatus != ScreeenConnectServiceStatus.NotInstalled)
                        {
                            statusChanged = true;
                            ServiceStatus = ScreeenConnectServiceStatus.NotInstalled;
                        }
                    }

                    if (statusChanged)
                    {
                        Logger.Debug("service status is {0}", ServiceStatus.ToString());
                    }
                    // Logger.Debug("user app flag is pulled to false");

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

       

        //internal void UserAppISActive()
        //{

        //    lock (_syncObj)
        //    {
        //        //Logger.Debug("user app flag is pulled to true");
        //        if (ServiceStatus == ScreeenConnectServiceStatus.Running)
        //        {
        //            _inactivityTimer.Stop();
        //            _inactivityTimer.Start();//reset the inactivity timer 
        //        }
        //        _userAppIsRunning = true;
        //    }
        //}

        #region Session monitoring

        /// <summary>
        /// Monitor if port 443 is in used and it's traffic level
        /// </summary>
        public void MonitorSession()
        {
            try
            {
                var result = NetworkHelper.GetEthernetAddress();
                if (result != null && result != string.Empty && result != "0.0.0.0")
                {
                    Logger.Information("device received ip address {0}",result);
                    _receivedIp = true;
                }

                if (_receivedIp)//check if port 443 is in used
                {

                    NetworkHelper.CheckIfSessionEstablished();
                    
                }
                else
                {
                    SessionStatus = ScreenConnectSessionStatus.CableDisconnected;
                }


                Logger.Debug("session status is {0}", SessionStatus);
            }
            catch (Exception ex)
            {
                Logger.Error("can't continue monitoring the session");
            }
        }


        #endregion 
    }
}

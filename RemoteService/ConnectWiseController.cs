using Interfaces;
using Logging;
using Microsoft.Diagnostics.Tracing.Parsers.Symbol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace LumenisRemoteService
{

    /*
     * SessionDisconnected
         1. when start service
         2. when no traffic detected and session counter  grater then what defined.
         3. when closing service
         4. ADD: when cable is disconnected.
         5.Add when no socket connection to the portal

     SessionConnectedAndStandby
         1. when no traffic detected and session counter  lesser then what defined.


     SessionConnectedAndActive
         1. when traffic detected
     * */


    /// <summary>
    /// Control and monitor connectWise service and active session
    /// </summary>
    /// 

    class ConnectWiseController
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        private readonly string JUMP_CLIENT_SERVICE_NAME_PREFIX;
        private readonly int MONITORED_PORT;
        private const int DEFAULT_PORT = 8041;
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
                Logger.Information($"Session sampling counter is {sessioncounter} and session interval is {sessionInterval}");


                TRAFFICMONITORINTERVAL = new TimeSpan(0, 0, interval).TotalMilliseconds;
                //Monitored port must be fixed for all installations. this section of code support rare cases of port modification that 
                //will affect all Lumenis installations.
                if (Int32.TryParse(ConfigurationManager.AppSettings["MonitoredPort"], out int val))
                {
                    MONITORED_PORT = val;
                }
                else
                {
                    MONITORED_PORT = DEFAULT_PORT;
                }
                revokeFeature.revokeFeatureInit();
                Logger.Information($"Monitor port is {MONITORED_PORT}");

                JUMP_CLIENT_SERVICE_NAME_PREFIX = Convert.ToString(ConfigurationManager.AppSettings["MonitoredProcessName"]);//todo name should also be fetched from configuration file

                Logger.Information("searching for service name {0}", JUMP_CLIENT_SERVICE_NAME_PREFIX);

                _timer = new System.Timers.Timer(SERVIICEMONITORINTERVAL);
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();

                _trafficMonitoringTimer = new System.Timers.Timer(TRAFFICMONITORINTERVAL);
                _trafficMonitoringTimer.Elapsed += _trafficMonitoringTimer_Elapsed;

                CheckServiceInstallation();
                UpdateSessionStatus(ScreenConnectSessionStatus.SessionDisconnected);
                NetworkHelper._networkState += NetworkHelper__networkState;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }


        }

        bool _networkState = false;
        private void NetworkHelper__networkState(bool obj)
        {
            if (obj)
            {
                _networkState = true;
                UpdateSessionStatus(ScreenConnectSessionStatus.SessionConnectedAndStandby);
            }
                
            else
            {
                _networkState = false;
                UpdateSessionStatus(ScreenConnectSessionStatus.SessionDisconnected);
            }
                
        }

        private void CheckServiceInstallation()
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void UpdateSessionStatus(ScreenConnectSessionStatus p_status, bool p_trafficState)
        {
            lock (_syncObj) NetworkHelper.TrafficDetected = p_trafficState;
            UpdateSessionStatus(p_status);
        }

        private void UpdateSessionStatus(ScreenConnectSessionStatus p_status, int p_counter,bool p_trafficState, bool p_increment = false)
        {
            lock (_syncObj) NetworkHelper.TrafficDetected = p_trafficState;
            UpdateSessionStatus(p_status,p_counter, p_increment);
        }

        private void UpdateSessionStatus(ScreenConnectSessionStatus p_status,int p_counter, bool p_increment = false)
        {
            if(p_increment) Interlocked.Increment(ref _counter);
            else Interlocked.Exchange(ref _counter, p_counter);

            UpdateSessionStatus(p_status);
        }
        private void UpdateSessionStatus(ScreenConnectSessionStatus p_status)
        {
            if (SessionStatus == ScreenConnectSessionStatus.SessionDisconnected && p_status == ScreenConnectSessionStatus.SessionConnectedAndActive)
            {
                Logger.Error($"didn't upfate session state. can't move to {ScreenConnectSessionStatus.SessionConnectedAndActive} directly from {ScreenConnectSessionStatus.SessionDisconnected}");
                return;
            }
             
            lock (_syncObj)
                SessionStatus = p_status;
        }

        private void _trafficMonitoringTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            if (NetworkHelper.TrafficDetected)
            {
                UpdateSessionStatus(ScreenConnectSessionStatus.SessionConnectedAndActive,1,false,false);
            }
            else
            {
                if (_counter <= _maxCounter)
                {
                    UpdateSessionStatus(ScreenConnectSessionStatus.SessionConnectedAndStandby, _counter,false,true);
                    
                }
                else //timeout
                {
                    // there is no traffic between client and server
                    UpdateSessionStatus(ScreenConnectSessionStatus.SessionDisconnected, 1,false,false);
                    Logger.Information("closing service because of traffic inactivity. inactivity counter is {0}", _counter);
                    Close();
                    revokeFeature.IsInTimeOut();
                }

                Logger.Debug(string.Format("traffic counter value is {0}",_counter));
                Logger.Debug($"Session status is {SessionStatus}");
            }
            if (!_networkState)
            {
                UpdateSessionStatus(ScreenConnectSessionStatus.SessionDisconnected, 1,false,false);
                Logger.Error("closing service because of network problem");
                Close();// if service not closed the portal connection become unstable
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
                    NetworkHelper.MonitorSession(MONITORED_PORT);
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
                if (_service == null)
                {
                    Logger.Information("service was not initialized");
                    return false;
                }
                if (_service.Status == ServiceControllerStatus.Paused || _service.Status == ServiceControllerStatus.Stopped)
                {
                    Logger.Debug("starting ScreenConnect service");
                    int count = 0;
                    while (count < 3)
                    {
                        try
                        {
                            if (_service.Status == ServiceControllerStatus.Running) break;//check again in case service state changed during last operation
                            count++;
                            _service.Start();//user case 1 
                           // Thread.Sleep(3000);//wait 3 sec so service status will be updated
                            break;

                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(2000);
                            Logger.Error(ex);
                        } 
                    }
                    NetworkHelper._activated = false;
                    _sessionWasActiveOnce = false;//reset the flag each time user request support or stop the service
                    lock (_syncObj)
                    {
                        _requestForSupportWasMade = true; 
                    }
                    _service.WaitForStatus(ServiceControllerStatus.Running,new TimeSpan(0,0,3));
                    if (_service.Status == ServiceControllerStatus.Running)
                    {
                        Thread.Sleep(5000);
                        NetworkHelper.MonitorSession(MONITORED_PORT);
                    }
                    _trafficMonitoringTimer.Start();// start monitoring session only after service is up and MonitorSession was called
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
                _trafficMonitoringTimer.Stop();
               
                if (_service == null)
                {
                    Logger.Information("service was not initialized");
                    return false;
                }
                if (_service.Status == ServiceControllerStatus.Running)
                {
                    Logger.Debug("stop service command");
                    lock (_syncObj)
                    {
                        _requestForSupportWasMade = false;
                    }
                   
                    int count = 0;
                    while (count < 3)
                    {
                        try
                        {
                            if (_service.Status != ServiceControllerStatus.Running) break;//check again in case service state changed during last operation
                            count++;
                            _service.Stop();//user case 3
                            UpdateSessionStatus(ScreenConnectSessionStatus.SessionDisconnected, false);
                            break;//if no exception is thrown break the loop
                        }
                        catch(InvalidOperationException e)
                        {
                            Logger.Error(e,"fail to close the service");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(2000);
                            Logger.Error(ex);
                        }
                    }
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
                        CheckServiceInstallation();
                        if (_lastStatus != ScreeenConnectServiceStatus.NotInstalled)
                        {
                            statusChanged = true;
                            ServiceStatus = ScreeenConnectServiceStatus.NotInstalled;
                        }
                    }

                    if (statusChanged)
                    {
                        Logger.Debug("service status changed to {0}", ServiceStatus.ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        
    }
}

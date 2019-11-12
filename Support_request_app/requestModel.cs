﻿using Interfaces;
using Lumenis.RemoteServiceApi;
using LumenisRemoteService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Support_request_app
{

    public class BaseNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RequestModel : BaseNotifier
    {
        RemoteAPI remoteApi = new RemoteAPI();
        System.Timers.Timer _timer = new System.Timers.Timer(1000);//this timer also for getting the status but also for keep alive check



        public RequestModel()
        {
            remoteApi.StartClient();
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                GetServiceStatus();//todo should only be performed if user app is active and not minimized
            }
            catch(Exception ex)
            {

            }
        }

        public void RequestSupport()
        {
            remoteApi.StartScreenConnect();
            var result = remoteApi.GetScreenConnectStatus();
            ServiceStatus = ConvertEnum(result);
           
        }

        public void GetServiceStatus()
        {
            var result = remoteApi.GetScreenConnectStatus();
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                ServiceStatus = ConvertEnum(result);
            }));
        }



        private bool _isServiceConnected = false;

        private string _serviceStatus = "None";

        public bool IsServiceConnected { get { return _isServiceConnected; } set { _isServiceConnected = value; OnPropertyChanged("IsServiceConnected"); } }

        //ScreeenConnectServiceStatus
        public string ServiceStatus { get { return _serviceStatus; } set { _serviceStatus = value; OnPropertyChanged("ServiceStatus"); } }

        private string ConvertEnum(ScreeenConnectServiceStatus p_enum)
        {
            switch(p_enum)
            {
                case ScreeenConnectServiceStatus.None: return "Unknown";
                case ScreeenConnectServiceStatus.NotInstalled: return "Service not installed";
                case ScreeenConnectServiceStatus.Running: return "Service is running";
                case ScreeenConnectServiceStatus.Stopped: return "Service stopped";
                default: return "Unknown";
            }
        }
    }


   // IsChecked ="{Binding ManualActivationMode.SR_isChargerOnChecked,ElementName=service_elem}"
    //public class ServiceOperationsRules : BaseNotifier
    //{
    //    #region Manual\Auto switch mode

    //    private bool _isAutoChecked = true;
    //    private bool _isChargerOnChecked = false;
    //    private bool _isChargerOffChecked = false;
    //    private bool _isOsillatorOnChecked = false;
    //    private bool _isOsillatorOffChecked = false;
    //    private bool _isAmpliafierOnChecked = false;
    //    private bool _isAmpliafierOffChecked = false;


    //    public bool SR_isAutoChecked
    //    {
    //        get
    //        {
    //            return _isAutoChecked;
    //        }
    //        set
    //        {
    //            _isAutoChecked = value;
    //            if (_isAutoChecked)
    //            {
    //                // set all other redio box checked status to false
    //                SR_isChargerOnChecked = SR_isChargerOffChecked = SR_isOsillatorOnChecked = SR_isOsillatorOffChecked = SR_isAmpliafierOnChecked = SR_isAmpliafierOffChecked = false;
    //            }
    //            OnPropertyChanged("SR_isAutoChecked");
    //        }
    //    }
    //    public bool SR_isChargerOnChecked { get { return _isChargerOnChecked; } set { _isChargerOnChecked = value; OnPropertyChanged("SR_isChargerOnChecked"); } }
    //    public bool SR_isChargerOffChecked { get { return _isChargerOffChecked; } set { _isChargerOffChecked = value; OnPropertyChanged("SR_isChargerOffChecked"); } }
    //    public bool SR_isOsillatorOnChecked { get { return _isOsillatorOnChecked; } set { _isOsillatorOnChecked = value; OnPropertyChanged("SR_isOsillatorOnChecked"); } }
    //    public bool SR_isOsillatorOffChecked { get { return _isOsillatorOffChecked; } set { _isOsillatorOffChecked = value; OnPropertyChanged("SR_isOsillatorOffChecked"); } }
    //    public bool SR_isAmpliafierOnChecked { get { return _isAmpliafierOnChecked; } set { _isAmpliafierOnChecked = value; OnPropertyChanged("SR_isAmpliafierOnChecked"); } }
    //    public bool SR_isAmpliafierOffChecked { get { return _isAmpliafierOffChecked; } set { _isAmpliafierOffChecked = value; OnPropertyChanged("SR_isAmpliafierOffChecked"); } }

    //    #endregion

    //    private bool _isQSEnabled = true; // if false QS will not run on step 5

    //    public bool SR_isQSEnabled { get { return _isQSEnabled; } set { _isQSEnabled = value; OnPropertyChanged("SR_isQSEnabled"); } }
    //}
}

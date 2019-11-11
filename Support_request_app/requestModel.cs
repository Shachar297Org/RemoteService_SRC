using Lumenis.RemoteServiceApi;
using LumenisRemoteService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
    class RequestModel : BaseNotifier
    {
        RemoteAPI remoteApi = new RemoteAPI();

        public RequestModel()
        {
           
        }

        public void RequestSupport()
        {
            remoteApi.StartScreenConnect();
        }

        public void GetServiceStatus()
        {
           // remoteApi.GetScreenConnectStatus();
        }



        private bool _isServiceConnected = false;

        public bool IsServiceConnected { get { return _isServiceConnected; } set { _isServiceConnected = value; OnPropertyChanged("IsServiceConnected"); } }
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

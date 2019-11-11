using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace LumenisRemoteService
{
    class LumenisRemoteServiceApp
    {
        private static ServiceHost _host = null;
        private const string LOG = "Application";
       // private static string _serviceName;

       // public LumenisRemoteServiceApp()
       // {
            //InitializeComponent();
      //  }

        internal static string Name
        {
            get { return "test"; }
        }

        static public void Start()
        {
            try
            {
                if (_host != null)
                {
                    _host.Close();
                }
                // Create the ServiceHost.
                _host = new ServiceHost(typeof(RemoteService));
                _host.Open();

              

            }
            catch (Exception ex)
            {
                // EventLog.WriteEntry(ServiceName, string.Format("OnStart Exception: {0}", ex.ToString()), EventLogEntryType.Error);
            }
        }

       // protected override void OnStart(string[] args)
       // {
            //_serviceName = ServiceName;
            //if (!EventLog.SourceExists(ServiceName))
            //{
            //    EventLog.CreateEventSource(ServiceName, LOG);
           // }

            
      //  }
    }
}

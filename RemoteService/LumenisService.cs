using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;

namespace LumenisRemoteService
{

    public partial class LumenisService : ServiceBase
    {
        private ServiceHost _host = null;
        private const string LOG = "Application";
        private static string _serviceName;

        public LumenisService()
        {
            InitializeComponent();
        }

        internal static string Name
        {
            get { return _serviceName;  }
        }

        protected override void OnStart(string[] args)
        {
            _serviceName = ServiceName;
            if (!EventLog.SourceExists(ServiceName))
            {
                EventLog.CreateEventSource(ServiceName, LOG);
            }

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
                EventLog.WriteEntry(ServiceName, string.Format("OnStart Exception: {0}", ex.ToString()), EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            try
            {
                // Close the ServiceHost.
                if (_host != null)
                {
                    _host.Close();
                    _host = null;
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, string.Format("OnStop Exception: {0}", ex.Message, EventLogEntryType.Error));
            }
        }
    }
}

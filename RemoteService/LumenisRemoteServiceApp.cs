﻿using Logging;
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
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
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
                Logger.Information("service host started");
                System.Threading.Thread.Sleep(5000);
                ServiceClient client = new ServiceClient();//start internal WCF client which make sure remote service will trun off ScreenConnect service
               




            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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

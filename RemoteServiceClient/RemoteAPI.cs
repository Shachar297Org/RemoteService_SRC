using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
//using Lumenis.RemoteServiceApi.LumenisRemoteService;
//using LumenisRemoteService;
using Interfaces;
using Logging;

namespace Lumenis.RemoteServiceApi
{
    public partial class RemoteAPI : IDisposable
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        // private RemoteServiceClient _remoteService = null;
        ChannelFactory<IRemoteService> _factory;
        IRemoteService _remoteService;
        NetTcpBinding myBinding = null;//NetTcpBinding_RemoteService
        EndpointAddress myEndpoint = null;
        


        public RemoteAPI()
        {
             myBinding = new NetTcpBinding("NetTcpBinding_RemoteService");//NetTcpBinding_RemoteService
             myEndpoint = new EndpointAddress("net.tcp://localhost:49494/RemoteService/ppool");
            Logger.Debug("end point address is {0}","net.tcp://localhost:49494/RemoteService/ppool");
        }

        public void StartClient()
        {
            // _factory = new ChannelFactory<IRemoteService>(myBinding, myEndpoint);
            try
            {
                _factory = new ChannelFactory<IRemoteService>(myBinding, myEndpoint);

                _remoteService = _factory.CreateChannel();// as RemoteService;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }


        

        public void Open()
        {
            if (_factory.State != CommunicationState.Opened)
            {
                _factory.Open(); 
            }
        }

        public void Close()
        {
            _factory.Close();
        }

        public RemoteStatus GetStatus()
        {
            ValidateConnection();
            return _remoteService.GetStatus();
        }

        public void StartRemoteConnection()
        {
            ValidateConnection();
            _remoteService.StartConnection();
        }

        public void StopRemoteConnection()
        {
            ValidateConnection();
            _remoteService.StopConnection();
        }

        public void Enable(bool enable)
        {
            ValidateConnection();
            _remoteService.Enable(enable);
        }

        public void CreateFeature(int featureId)
        {
            ValidateConnection();
            _remoteService.CreateFeature(featureId);
        }

        public bool HasFeature(int featureId)
        {
            ValidateConnection();
            return _remoteService.HasFeature(featureId);
        }

        public bool ExtendFeature(int featureId)
        {
            ValidateConnection();
            return _remoteService.ExtendFeature(featureId);
        }

        public void RemoveFeature(int featureId)
        {
            ValidateConnection();
            _remoteService.RemoveFeature(featureId);
        }

        private void ValidateConnection()
        {
            int numRetries = 3;

            while (_factory.State != CommunicationState.Opened && numRetries-- > 0)
            {
                if (_factory.State == CommunicationState.Closing ||
                    _factory.State == CommunicationState.Opening)
                {
                    Thread.Sleep(3000);
                    if (_factory.State == CommunicationState.Closing ||
                        _factory.State == CommunicationState.Opening)
                    {
                        // try to force closing
                        _factory.Close();
                    }
                }

                if (_factory.State == CommunicationState.Faulted)
                {
                    _factory.Close();
                }

                if (_factory.State == CommunicationState.Closed)
                {
                    _factory = new ChannelFactory<IRemoteService>(myBinding);
                    _factory.Open();
                }

                Thread.Sleep(3000);
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}

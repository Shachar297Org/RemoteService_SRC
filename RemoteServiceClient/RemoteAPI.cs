using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using Lumenis.RemoteServiceApi.LumenisRemoteService;

namespace Lumenis.RemoteServiceApi
{
    public class RemoteAPI : IDisposable
    {
        private RemoteServiceClient _remoteService = null;

        public RemoteAPI()
        {
            _remoteService = new RemoteServiceClient(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:49494/RemoteService"));
        }

        public void Open()
        {
            _remoteService.Open();
        }

        public void Close()
        {
            _remoteService.Close();
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

            while (_remoteService.State != CommunicationState.Opened && numRetries-- > 0)
            {
                if (_remoteService.State == CommunicationState.Closing ||
                    _remoteService.State == CommunicationState.Opening)
                {
                    Thread.Sleep(3000);
                    if (_remoteService.State == CommunicationState.Closing ||
                        _remoteService.State == CommunicationState.Opening)
                    {
                        // try to force closing
                        _remoteService.Close();
                    }
                }

                if (_remoteService.State == CommunicationState.Faulted)
                {
                    _remoteService.Close();
                }

                if (_remoteService.State == CommunicationState.Closed)
                {
                    _remoteService = new RemoteServiceClient();
                    _remoteService.Open();
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

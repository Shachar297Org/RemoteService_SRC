using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
//using Lumenis.RemoteServiceApi.LumenisRemoteService;
//using LumenisRemoteService;
using Interfaces;
//using Logging;

namespace Lumenis.RemoteServiceApi
{


    public partial class RemoteAPI : IDisposable
    {
        //private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        // private RemoteServiceClient _remoteService = null;
        ChannelFactory<IRemoteService> _factory;
        IRemoteService _remoteService;
        NetNamedPipeBinding myBinding = null;//NetTcpBinding_RemoteService
        EndpointAddress myEndpoint = null;
        


        public RemoteAPI()
        {
            //       < client >
            //  < endpoint  address = "net.pipe://localhost/RemoteService/ppool" binding = "netNamedPipeBinding"
            //      bindingConfiguration = "NetNamedPipeBinding_RemoteService" contract = "Interfaces.IRemoteService"
            //      name = "NetNamedPipeBinding_RemoteService" >
            //  </ endpoint >
            //</ client >
            // myBinding = new NetNamedPipeBinding("NetNamedPipeBinding_RemoteService");//NetTcpBinding_RemoteService
            try
            {
                myBinding = new NetNamedPipeBinding();//NetTcpBinding_RemoteService

                myEndpoint = new EndpointAddress("net.pipe://localhost/RemoteService/ppool");
            }
            catch
            {

              
            }
            
            
           // Logger.Debug("end point address is {0}","net.tcp://localhost:49494/RemoteService/ppool");
        }

        //public void StartClient()
        //{
           
        //    try
        //    {
        //        _factory = new ChannelFactory<IRemoteService>(myBinding, myEndpoint);

        //        _remoteService = _factory.CreateChannel(myEndpoint);// as RemoteService;


        //        _factory.Faulted += _factory_Faulted;
        //        _factory.Closed += _factory_Closed;
        //        // _factory.Open();


        //    }
        //    catch (Exception ex)
        //    {
        //        // Logger.Error(ex);
        //    }
        //}
        public void Open()
        {
            try
            {
                _factory = new ChannelFactory<IRemoteService>(myBinding, myEndpoint);

                _remoteService = _factory.CreateChannel();// as RemoteService;


                _factory.Faulted += _factory_Faulted;
                _factory.Closed += _factory_Closed;
                // _factory.Open();


            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
            }
        }


        private void _factory_Closed(object sender, EventArgs e)
        {
           // Logger.Warning("factory closed");
        }

        private void _factory_Faulted(object sender, EventArgs e)
        {

           // Logger.Error("factory error");
        }

       

        public void Close()
        {
            try
            {
                ValidateConnection();
                _factory.Close();
            }
            catch
            {

              
            }
        }

        public RemoteStatus GetStatus()
        {
            try
            {
                ValidateConnection();
                return _remoteService.GetStatus();
            }
            catch
            {

                return new RemoteStatus();
            }
        }

        public void StartRemoteConnection()
        {
            try
            {
                ValidateConnection();
                _remoteService.StartConnection();
            }
            catch
            {

            }
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

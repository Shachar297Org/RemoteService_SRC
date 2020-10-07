using Interfaces;
using Logging;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace LumenisRemoteService
{
   

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class RemoteService:IRemoteService
    {
        public RemoteStatus GetStatus()
        {
            return new RemoteStatus()
            {
               
                IsConnected = false,
                IsEnabled = false
            };
        }

        public void StartConnection()
        {
           // ActionDispatcher.Instance.StartService();
        }

        public void StopConnection()
        {
           // ActionDispatcher.Instance.StopService();
        }

        public void Enable(bool enable)
        {
          //  ActionDispatcher.Instance.Enable(enable);
        }

        public void CreateFeature(int featureId)
        {
            Logger.Information("CreateFeature command received");
            ServiceToken.Instance().Create(featureId);
        }

        public bool HasFeature(int featureId)
        {
            return ServiceToken.Instance().Exists(featureId);
        }

        public bool ExtendFeature(int featureId)
        {
            return ServiceToken.Instance().Extend(featureId);
        }

       // [OperationContract]
        public void RemoveFeature(int featureId)
        {
            Logger.Information("RemoveFeature command received");
            ServiceToken.Instance().Remove(featureId);
        }

    }
}

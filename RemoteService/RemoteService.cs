using System.Runtime.Serialization;
using System.ServiceModel;

namespace LumenisRemoteService
{
    [DataContract]
    public struct RemoteStatus
    {
        [DataMember]
        public bool IsConnected;
        [DataMember]
        public bool IsEnabled;
    }

    [ServiceContract]
    public class RemoteService
    {
        [OperationContract]
        public RemoteStatus GetStatus()
        {
            return new RemoteStatus()
            {
                IsConnected = ActionDispatcher.Instance.IsRunning(),
                IsEnabled = ActionDispatcher.Instance.IsEnabled
            };
        }

        [OperationContract]
        public void StartConnection()
        {
            ActionDispatcher.Instance.StartService();
        }

        [OperationContract]
        public void StopConnection()
        {
            ActionDispatcher.Instance.StopService();
        }

        [OperationContract]
        public void Enable(bool enable)
        {
            ActionDispatcher.Instance.Enable(enable);
        }

        [OperationContract]
        public void CreateFeature(int featureId)
        {
            ServiceToken.Instance().Create(featureId);
        }

        [OperationContract]
        public bool HasFeature(int featureId)
        {
            return ServiceToken.Instance().Exists(featureId);
        }

        [OperationContract]
        public bool ExtendFeature(int featureId)
        {
            return ServiceToken.Instance().Extend(featureId);
        }

        [OperationContract]
        public void RemoveFeature(int featureId)
        {
            ServiceToken.Instance().Remove(featureId);
        }

    }
}

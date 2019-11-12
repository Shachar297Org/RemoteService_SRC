using Interfaces;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace LumenisRemoteService
{
    //[ServiceContract]
    //public interface IRemoteService
    //{
    //    [OperationContract]
    //     RemoteStatus GetStatus();

    //    [OperationContract]
    //     void StartConnection();


    //    [OperationContract]
    //     void StopConnection();



    //    [OperationContract]
    //     void Enable(bool enable);



    //    [OperationContract]
    //     void CreateFeature(int featureId);



    //    [OperationContract]
    //     bool HasFeature(int featureId);



    //    [OperationContract]
    //     bool ExtendFeature(int featureId);



    //    [OperationContract]
    //     void RemoveFeature(int featureId);


    //    #region ConnectWise

    //    [OperationContract]
    //    bool StartScreenConnect();

    //    [OperationContract]

    //    bool StopService();

    //    [OperationContract]

    //    string GetScreenConnectStatus();
       

    //    #endregion

    //}

    //[DataContract]
    //public struct RemoteStatus
    //{
    //    [DataMember]
    //    public bool IsConnected;
    //    [DataMember]
    //    public bool IsEnabled;
    //}

   // [ServiceContract]
    public partial class RemoteService:IRemoteService
    {
        public RemoteStatus GetStatus()
        {
            return new RemoteStatus()
            {
                // IsConnected = ActionDispatcher.Instance.IsRunning(),
                // IsEnabled = ActionDispatcher.Instance.IsEnabled
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
            ServiceToken.Instance().Remove(featureId);
        }

    }
}

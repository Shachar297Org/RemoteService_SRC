using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    [DataContract]
    public enum ScreeenConnectServiceStatus
    {
        [EnumMember]
        None,
        [EnumMember]
        NotInstalled,
        [EnumMember]
        Running,
        [EnumMember]
        Stopped,
        [EnumMember]
        Unstable
    }

    [DataContract]
    public enum ScreenConnectSessionStatus
    {
        [EnumMember]
        None,
       // [EnumMember]
        //CableDisconnected,
        [EnumMember]
        SessionConnectedAndStandby,
        [EnumMember]
        SessionConnectedAndActive,
        [EnumMember]
        SessionDisconnected
    }

    [DataContract]
    public struct RemoteStatus
    {
        [DataMember]
        public bool IsConnected;
        [DataMember]
        public bool IsEnabled;
    }

    [ServiceContract]
    public interface IRemoteService
    {
        //[OperationContract]
        //void Open();

        [OperationContract]
        RemoteStatus GetStatus();

        [OperationContract]
        void StartConnection();


        [OperationContract]
        void StopConnection();



        [OperationContract]
        void Enable(bool enable);



        [OperationContract]
        void CreateFeature(int featureId);



        [OperationContract]
        bool HasFeature(int featureId);



        [OperationContract]
        bool ExtendFeature(int featureId);



        [OperationContract]
        void RemoveFeature(int featureId);


        #region ConnectWise

        [OperationContract]
        bool StartScreenConnect();

        [OperationContract]

        bool StopService();

        [OperationContract]

        ScreeenConnectServiceStatus GetScreenConnectStatus();

        [OperationContract]
        ScreenConnectSessionStatus GetSessiontStatus();

        //[OperationContract]
        //void RenewSessionLimit();

        //[OperationContract]
        //TimeSpan SessionTimeLeft();


        #endregion

    }
}

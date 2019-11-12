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

        string GetScreenConnectStatus();


        #endregion

    }
}

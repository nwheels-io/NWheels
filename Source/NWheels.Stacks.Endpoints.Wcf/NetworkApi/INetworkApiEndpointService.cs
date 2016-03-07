using System.Runtime.Serialization;
using System.ServiceModel;

namespace NWheels.Stacks.Endpoints.Wcf.NetworkApi
{
    [ServiceContract(
        Name = "NetworkingApiEndpoint", 
        Namespace = ContractNames.Namesapce,
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(INetworkApiEndpointServiceCallback))]
    public interface INetworkApiEndpointService
    {
        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void Connect(MessageRequest request);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(MessageRequest request);

        [OperationContract(IsOneWay = true)]
        void SendMessage(MessageRequest request);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface INetworkApiEndpointServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void Connected(MessageRequest request);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void ConnectDeclined(MessageRequest request);

        [OperationContract(IsOneWay = true)]
        void SendMessage(MessageRequest request);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = ContractNames.Namesapce)]
    public class MessageRequest
    {
        [DataMember]
        public byte[] HeadersAndBody { get; set; }
    }
}

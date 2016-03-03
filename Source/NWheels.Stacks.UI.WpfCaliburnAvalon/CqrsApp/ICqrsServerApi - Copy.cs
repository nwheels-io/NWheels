#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing;
using NWheels.Stacks.UI.WpfCaliburnAvalon.CqrsApp.Requests;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.CqrsApp
{
    public static class CqrsApiNames
    {
        public const string Namespace = "NWheels.Cqrs";
    }

    [ServiceContract(
        Namespace = CqrsApiNames.Namespace, 
        CallbackContract = typeof(ICqrsServerApiCallback), 
        SessionMode = SessionMode.Required)]
    public interface ICqrsServerApi
    {
        [OperationContract(IsInitiating = true, IsOneWay = true)]
        void Connect(ConnectRequest request);

        [OperationContract(IsTerminating = true, IsOneWay = true)]
        void Disconnect(DisconnectRequest request);

        [OperationContract(IsOneWay = true)]
        void SendCommands(SendCommandsRequest request);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICqrsServerApiCallback
    {
        [OperationContract(IsOneWay = true)]
        void PushEvents(PushEventsRequest request);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace Requests
    {
        [DataContract(Namespace = CqrsApiNames.Namespace)]
        public class ConnectRequest
        {
            [DataMember]
            public string UserName { get; set; }
            [DataMember]
            public string Password { get; set; }
        }
 
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = CqrsApiNames.Namespace)]
        public class DisconnectRequest
        {
            [DataMember]
            public string SerializedWorkspace { get; set; }
        }
       
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = CqrsApiNames.Namespace)]
        public class SendCommandsRequest
        {
            [DataMember]
            public IList<IServerCommand> Commands { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = CqrsApiNames.Namespace)]
        public class PushEventsRequest
        {
            [DataMember]
            public IList<IPushEvent> Events { get; set; }
        }
    }
}

#endif
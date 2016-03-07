using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class MessageLogBehavior : IEndpointBehavior
    {
        private readonly LoggingMessageInspector _messageInspector;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public MessageLogBehavior(LoggingMessageInspector messageInspector)
        {
            _messageInspector = messageInspector;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
            
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
            
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(_messageInspector);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
            
        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
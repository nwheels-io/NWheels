using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class ErrorHandlerBehavior : IServiceBehavior
    {
        private readonly ErrorHandler _handler;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ErrorHandlerBehavior(ErrorHandler handler)
        {
            _handler = handler;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach ( ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers )
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;

                if ( channelDispatcher != null )
                {
                    channelDispatcher.ErrorHandlers.Add(_handler);
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
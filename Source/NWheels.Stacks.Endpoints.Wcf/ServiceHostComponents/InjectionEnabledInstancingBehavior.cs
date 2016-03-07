using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac;
using NWheels.Extensions;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class InjectionEnabledInstancingBehavior : IServiceBehavior
    {
        private readonly IComponentContext _components;
        private readonly Type _serviceContractType;
        private readonly bool _isSingleton;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public InjectionEnabledInstancingBehavior(IComponentContext components, Type serviceContractType)
        {
            _components = components;
            _serviceContractType = serviceContractType;
            _isSingleton = components.IsServiceRegisteredAsSingleton(_serviceContractType);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }
 
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var instanceProvider = new InjectionEnabledInstanceProvider(_serviceContractType, _components);
            IInstanceContextProvider instanceContextProvider;

            if ( _isSingleton )
            {
                instanceContextProvider = new SingletonInstanceContextProvider(serviceHostBase);
            }
            else
            {
                instanceContextProvider = new PerCallInstanceContextProvider();
            }

            foreach( var dispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>() )
            {
                foreach ( var endpointDispatcher in dispatcher.Endpoints )
                {
                    DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
                    dispatchRuntime.InstanceProvider = instanceProvider;
                    dispatchRuntime.InstanceContextProvider = instanceContextProvider;
                }
            }
        }
 
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
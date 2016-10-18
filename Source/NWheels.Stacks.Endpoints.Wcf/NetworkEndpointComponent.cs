using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;
using Autofac;
using NWheels.Endpoints;
using NWheels.Hosting;
using NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents;

namespace NWheels.Stacks.Endpoints.Wcf
{
    public class NetworkEndpointComponent : LifecycleEventListenerBase
    {
        private readonly NetworkApiEndpointRegistration _endpointRegistration;
        private readonly IFrameworkEndpointsConfig _config;
        private readonly IWcfServiceLogger _logger;
        private readonly IComponentContext _components;
        private readonly Type _contractType;
        private InjectionEnabledServiceHost _serviceHost;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NetworkEndpointComponent(
            NetworkApiEndpointRegistration endpointRegistration, 
            Auto<IFrameworkEndpointsConfig> config, 
            Auto<IWcfServiceLogger> logger, 
            IComponentContext components)
        {
            _endpointRegistration = endpointRegistration;
            _config = config.Instance;
            _logger = logger.Instance;
            _components = components;
            _contractType = _endpointRegistration.Contract;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeActivated()
        {
            _endpointRegistration.ApplyConfiguration(_config);
            _serviceHost = new InjectionEnabledServiceHost(_components, _contractType);

            ConfigureApiEndpoint();

            if ( _endpointRegistration.ShouldPublishMetadata )
            {
                ConfigureMetadataEndpoint();
            }

            ConfigureExceptionDetails();

            _serviceHost.Open();
            _logger.ServiceHostOpen(listenUrl: _endpointRegistration.Address.ToString(), metadataUrl: _endpointRegistration.MetadataAddress.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            _serviceHost.Close(TimeSpan.FromSeconds(10));
            _logger.ServiceHostClosed(listenUrl: _endpointRegistration.Address.ToString(), metadataUrl: _endpointRegistration.MetadataAddress.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureExceptionDetails()
        {
            var debugBehavior = _serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();

            if ( debugBehavior == null )
            {
                debugBehavior = new ServiceDebugBehavior();
                _serviceHost.Description.Behaviors.Add(debugBehavior);
            }

            debugBehavior.IncludeExceptionDetailInFaults = _endpointRegistration.ShouldExposeExceptionDetails;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureApiEndpoint()
        {
            var apiEndpoint = new ServiceEndpoint(
                ContractDescription.GetContract(_endpointRegistration.Contract),
                new BasicHttpBinding() {
                    ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                    MaxReceivedMessageSize = 10 * 1024 * 1024
                },
                new EndpointAddress(_endpointRegistration.Address));

            _serviceHost.AddServiceEndpoint(apiEndpoint);
            apiEndpoint.EndpointBehaviors.Add(new MessageLogBehavior(new LoggingMessageInspector(_logger)));

            _serviceHost.Description.Behaviors.Add(new ErrorHandlerBehavior(new ErrorHandler(_logger)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureMetadataEndpoint()
        {
            var metadataBehavior = new ServiceMetadataBehavior();
            
            metadataBehavior.HttpGetEnabled = true;
            metadataBehavior.HttpGetUrl = _endpointRegistration.MetadataAddress;
            
            _serviceHost.Description.Behaviors.Add(metadataBehavior);
        }
    }
}

using Autofac;
using NWheels.Endpoints;
using NWheels.Hosting;
using NWheels.Processing.Messages;

namespace NWheels.Stacks.Network
{
    public class NetworkEndpointComponent : LifecycleEventListenerBase
    {
        private readonly NetworkApiEndpointRegistration _registration;
        private readonly IComponentContext _components;
        private readonly INetworkEndpointLogger _logger;
        private readonly IFramework _framework;
        private readonly IServiceBus _serviceBus;

        private NetConnectorsBinaryTransport ConnectorsManager;

        public NetworkEndpointComponent(IAbstractNetwrokTransportConfig transportConfiguration,
            IComponentContext components,
            NetworkApiEndpointRegistration registration,
            INetworkEndpointLogger logger,
            IFramework framework,
            IServiceBus serviceBus)
        {
            _logger = logger;
            _framework = framework;
            _components = components;
            _registration = registration;
            _serviceBus = serviceBus;
            ConnectorsManager = new TcpBinaryTransport(transportConfiguration, registration, components, _logger, _framework, _serviceBus);
        }

        public override void Activate()
        {
            var service = _components.Resolve(_registration.Contract); // Contract == IChatService

            _logger.NetworkEndpointListening(_registration.Address.ToString(), _registration.Contract.FullName);
            // open socket for listening
            ConnectorsManager.StartListening(); // _registration.Address.Port ???
        }

        public override void Deactivate()
        {
            // stop listening on socket
            ConnectorsManager.StopListening();
            _logger.NetworkEndpointClosed(_registration.Address.ToString(), _registration.Contract.FullName);
        }
    }
}
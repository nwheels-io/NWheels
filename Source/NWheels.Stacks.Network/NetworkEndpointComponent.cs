using Autofac;
using NWheels.Endpoints;
using NWheels.Hosting;

namespace NWheels.Stacks.Network
{
    public class NetworkEndpointComponent : LifecycleEventListenerBase
    {
        private readonly NetworkApiEndpointRegistration _registration;
        private readonly IComponentContext _components;
        private readonly INetworkEndpointLogger _logger;
        private readonly IFramework _framework;

        private AbstractNetConnectorsManager ConnectorsManager;

        public NetworkEndpointComponent(IComponentContext components, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger, IFramework framework)
        {
            _logger = logger;
            _framework = framework;
            _components = components;
            _registration = registration;
            // -=-= Eli: Todo: Decide upon Id and other parameters
            ConnectorsManager = new TcpConnectorsManager(0, TcpConnectorsManager.AddressListenMode.External, TcpSocketsUtils.DefualtReceiveBufferSize, registration, components, _logger, _framework);
        }

        public override void Activate()
        {
            var service = _components.Resolve(_registration.Contract); // Contract == IChatService

            _logger.NetworkEndpointListening(_registration.Address.ToString(), _registration.Contract.FullName);
            // open socket for listening
            ConnectorsManager.StartListening(_registration.Address.Port, true);
        }

        public override void Deactivate()
        {
            // stop listening on socket
            ConnectorsManager.StopListening();
            _logger.NetworkEndpointClosed(_registration.Address.ToString(), _registration.Contract.FullName);
        }
    }
}
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

        private AbstractNetConnectorsManager ConnectorsManager;

        public NetworkEndpointComponent(IComponentContext components, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
        {
            _logger = logger;
            _components = components;
            _registration = registration;
            ConnectorsManager = new SocketConnectorsManager(0, registration, _logger); // -=-= Eli: Todo: Decide upon Id 
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
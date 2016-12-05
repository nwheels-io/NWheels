using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    public interface IDuplexNetworkApiEndpoint<TServerApi, TClientApi> : IDisposable
    {
        void Broadcast(Action<TClientApi> actionPerClient);
        int ActiveClientCount { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDuplexNetworkApiTarget<TLocalApi, TRemoteApi>
    {
        void OnConnected(IDuplexNetworkApiEndpoint<TLocalApi, TRemoteApi> endpoint, TRemoteApi remoteProxy);
        void OnDisconnected(IDuplexNetworkApiEndpoint<TLocalApi, TRemoteApi> endpoint, TRemoteApi remoteProxy, ConnectionCloseReason reason);
    }
}

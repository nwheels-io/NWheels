using System;

namespace NWheels.Endpoints.Factories
{
    public interface IDuplexNetworkApiProxyFactory
    {
        Type GetOrBuildProxyType(Type remoteApiContract, Type localApiContract);
        object CreateProxyInstance(Type remoteApiContract, Type localApiContract, IDuplexNetworkEndpointTransport transport, object localServer);
        TRemoteApi CreateProxyInstance<TRemoteApi, TLocalApi>(IDuplexNetworkEndpointTransport transport, TLocalApi localServer);
    }
}
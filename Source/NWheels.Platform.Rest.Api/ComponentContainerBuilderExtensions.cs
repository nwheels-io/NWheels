using NWheels.Injection;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public static class ComponentContainerBuilderExtensions
    {
        public static HttpEndpointInjectorPort ServeRestApiRequests<TProtocol>(this HttpEndpointInjectorPort port)
            where TProtocol : MessageProtocol
        {
            var components = port.Components;
            var protocol = new LazySlim<TProtocol>(factory: () => components.Resolve<TProtocol>());

            port.OnRequest = (context) => {
                var restApiService = components.Resolve<IRestApiService>();
                return restApiService.HandleHttpRequest(context, protocol.Value.ProtocolName);
            };

            return port;
        }
    }
}

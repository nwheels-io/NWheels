using NWheels.Injection;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public static class ComponentContainerBuilderExtensions
    {
        public static HttpEndpointInjectorPort RouteRequestsToRestApiService(this HttpEndpointInjectorPort port, string protocolName)
        {
            var components = port.Components;

            port.Handler = (context) => {
                var restApiService = components.Resolve<IRestApiService>();
                return restApiService.HandleHttpRequest(context, protocolName);
            };

            return port;
        }
    }
}

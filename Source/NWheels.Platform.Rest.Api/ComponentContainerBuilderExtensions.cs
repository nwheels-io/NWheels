using NWheels.Injection;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public static class ComponentContainerBuilderExtensions
    {
        public static HttpEndpointInjectorPort RouteRequestsToRestApiService(this HttpEndpointInjectorPort port)
        {
            port.Handler = (context) => {
                var restApiService = port.Components.Resolve<IRestApiService>();
                return restApiService.HandleHttpRequest(context);
            };

            return port;
        }
    }
}

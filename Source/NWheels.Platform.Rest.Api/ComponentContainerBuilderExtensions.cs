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
                var resource = restApiService.GetProtocolHandler<IHttpResourceProtocolHandler>(context.Request.Path);
                return resource.HandleHttpRequest(context);
            };

            return port;
        }
    }
}

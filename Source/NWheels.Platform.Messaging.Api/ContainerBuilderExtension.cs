using Microsoft.AspNetCore.Http;
using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public static class ComponentContainerBuilderExtensions
    {
        public static void ContributeHttpEndpoint<TComponent>(
            this IComponentContainerBuilder containerBuilder, 
            IHttpEndpointConfiguration configuration,
            Func<HttpContext, Task> handler)
            where TComponent : class
        {
            containerBuilder.RegisterInstance<HttpEndpointInjectorPort>(new HttpEndpointInjectorPort(configuration, handler));
        }
    }
}

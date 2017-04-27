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
        public static void ContributeHttpEndpoint(
            this IComponentContainerBuilder containerBuilder, 
            string name,
            Func<HttpContext, Task> handler)
        {
            containerBuilder.RegisterComponentInstance<HttpEndpointInjectorPort>(new HttpEndpointInjectorPort(name, handler));
        }
    }
}

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
        public static HttpEndpointInjectorPort ContributeHttpEndpoint(
            this IComponentContainerBuilder containerBuilder, 
            string name,
            Func<HttpContext, Task> onRequest = null)
        {
            var port = new HttpEndpointInjectorPort(containerBuilder, name, onRequest);
            containerBuilder.RegisterComponentInstance<HttpEndpointInjectorPort>(port);
            return port;
        }
    }
}

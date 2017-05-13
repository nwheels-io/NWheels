using Microsoft.AspNetCore.Http;
using NWheels.Injection;
using NWheels.Microservices;
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
            int? listenPortNumber = null,
            Action<IHttpEndpointConfig> onConfiguration = null,
            Func<HttpContext, Task> onRequest = null)
        {
            if (listenPortNumber.HasValue)
            {
                onConfiguration += (http) => {
                    http.Port = listenPortNumber.Value;
                };
            }

            var port = new HttpEndpointInjectorPort(containerBuilder, name, onConfiguration, onRequest);
            containerBuilder.RegisterComponentInstance<HttpEndpointInjectorPort>(port);
            return port;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ContributeMessageProtocol<TProtocol>(this IComponentContainerBuilder containerBuilder)
            where TProtocol : MessageProtocol
        {
            containerBuilder.RegisterComponentType<TProtocol>()
                .SingleInstance()
                .ForService<MessageProtocol>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IMicroserviceHostBuilder UseMessageProtocol<TProtocol>(this IMicroserviceHostBuilder hostBuilder)
            where TProtocol : MessageProtocol
        {
            hostBuilder.ContributeComponents((existingComponents, newComponents) => {
                newComponents.ContributeMessageProtocol<TProtocol>();
            });

            return hostBuilder;
        }
    }
}

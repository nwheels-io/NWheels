using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Api;
using NWheels.Communication.Api.Http;

namespace NWheels.Communication.Api.Extensions
{
    public static class BuilderExtensions
    {
        public static void UseHttpEndpoint(
            this MicroserviceHostBuilder builder, 
            string name = "Default", 
            Action<IHttpEndpointConfigElement> configure = null)
        {
            builder.ContributeComponents((exitingComponents, newComponents) => {
                newComponents.ContributeHttpEndpoint(name, configure);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static void ContributeHttpEndpoint(
            this IComponentContainerBuilder builder, 
            string name = "Default", 
            Action<IHttpEndpointConfigElement> configure = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Endpoint name must be a non-empty string", nameof(name));
            }

            var adapterPort = new HttpEndpointAdapterInjectionPort(builder, configure); 
            builder.RegisterAdapterPort(adapterPort);
        }
    }
}

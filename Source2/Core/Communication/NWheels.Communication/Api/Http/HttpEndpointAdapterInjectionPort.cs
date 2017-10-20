using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Communication.Api.Http
{
    public class HttpEndpointAdapterInjectionPort : AdapterInjectionPort<IHttpEndpoint, IHttpEndpointConfigElement>
    {
        public HttpEndpointAdapterInjectionPort(
            IComponentContainerBuilder containerBuilder, 
            IHttpEndpointConfigElement defaultConfiguration) 
            : base(containerBuilder, defaultConfiguration)
        {
        }
    }
}

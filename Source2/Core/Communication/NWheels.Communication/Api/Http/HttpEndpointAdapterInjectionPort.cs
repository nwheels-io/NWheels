using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Api;

namespace NWheels.Communication.Api.Http
{
    public class HttpEndpointAdapterInjectionPort : AdapterInjectionPort<IHttpEndpoint, IHttpEndpointConfigElement>
    {
        // public HttpEndpointAdapterInjectionPort(
        //     IComponentContainerBuilder containerBuilder, 
        //     IHttpEndpointConfigElement defaultConfiguration) 
        //     : base(containerBuilder, defaultConfiguration)
        // {
        // }
        public HttpEndpointAdapterInjectionPort(
            IComponentContainerBuilder containerBuilder, 
            ConfiguratorAction configurator) 
            : base(containerBuilder, ConfigurationFactory, configurator)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void CompleteAdapterComponentRegistration(IComponentRegistrationBuilder registration)
        {
            registration.ForService<ILifecycleComponent>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static IHttpEndpointConfigElement ConfigurationFactory(IComponentContainer components)
        {
            return components.Resolve<IHttpEndpointConfigElement>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public class HttpEndpointInjectorPort : InjectorPort
    {
        public HttpEndpointInjectorPort(
            IComponentContainerBuilder containerBuilder, 
            string name, 
            Action<IHttpEndpointConfig> onConfiguration, 
            Func<HttpContext, Task> onRequest)
            : base(containerBuilder)
        {
            this.Name = name;
            this.OnConfiguration = onConfiguration;
            this.OnRequest = onRequest;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ConfigureHttpEndpoint(IComponentContainer existingComponents)
        {
            if (OnConfiguration != null)
            {
                var configuration = existingComponents.Resolve<IMessagingPlatformConfiguration>();

                if (!configuration.Endpoints.TryGetValue(this.Name, out IEndpointConfig endpointConfig))
                {
                    endpointConfig = configuration.NewHttpEndpointConfig();
                    endpointConfig.Name = this.Name;
                    configuration.Endpoints.Add(this.Name, endpointConfig);
                }

                var httpEndpointConfig = (IHttpEndpointConfig)endpointConfig;
                OnConfiguration(httpEndpointConfig);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; }
        public Action<IHttpEndpointConfig> OnConfiguration { get; set; }
        public Func<HttpContext, Task> OnRequest { get; set; }
    }
}

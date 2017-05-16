using NWheels.Microservices;
using System;

namespace NWheels.Frameworks.Uidl.Adapters.WebAngular
{
    public static class InjectionExtensions
    {
        public static IMicroserviceHostBuilder UseAngular(this IMicroserviceHostBuilder hostBuilder)
        {
            hostBuilder.UseFrameworkFeature<WebAngularFeatureLoader>();
            return hostBuilder;
        }
    }
}

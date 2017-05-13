using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public static class MicroserviceHostBuilderExtensions
    {
        public static IMicroserviceHostBuilder UseRestApi(this IMicroserviceHostBuilder hostBuilder)
        {
            hostBuilder.UseFrameworkFeature<RestApiFeatureLoader>();
            return hostBuilder;
        }
    }
}

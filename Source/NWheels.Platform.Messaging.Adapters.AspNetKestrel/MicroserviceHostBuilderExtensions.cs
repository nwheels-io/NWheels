using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging.Adapters.AspNetKestrel
{
    public static class MicroserviceHostBuilderExtensions
    {
        public static IMicroserviceHostBuilder UseKestrel(this IMicroserviceHostBuilder hostBuilder)
        {
            return hostBuilder.UseFrameworkFeature<KestrelFeatureLoader>();
        }
    }
}

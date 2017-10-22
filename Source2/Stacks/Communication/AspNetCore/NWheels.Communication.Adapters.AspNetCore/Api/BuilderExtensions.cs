using System;
using NWheels.Communication.Adapters.AspNetCore.Runtime;
using NWheels.Microservices.Api;

namespace NWheels.Communication.Adapters.AspNetCore.Api
{
    public static class BuilderExtensions
    {
        public static MicroserviceHostBuilder UseAspNetCore(this MicroserviceHostBuilder hostBuilder)
        {
            hostBuilder.UseFrameworkFeature<AspNetCoreFeature>();
            return hostBuilder;
        }
    }
}

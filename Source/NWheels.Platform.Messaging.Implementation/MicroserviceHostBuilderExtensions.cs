using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    public static class MicroserviceHostBuilderExtensions
    {
        public static IMicroserviceHostBuilder UseMessaging(this IMicroserviceHostBuilder hostBuilder)
        {
            return hostBuilder;
        }
    }
}

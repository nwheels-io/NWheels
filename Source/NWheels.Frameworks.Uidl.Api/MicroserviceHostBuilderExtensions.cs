using NWheels.Frameworks.Uidl.Abstractions;
using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl
{
    public static class MicroserviceHostBuilderExtensions
    {
        public static IMicroserviceHostBuilder UseUidl(this IMicroserviceHostBuilder hostBuilder)
        {
            return hostBuilder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IMicroserviceHostBuilder UseWebAppHttpEndpoint<TApp>(this IMicroserviceHostBuilder hostBuilder, int? listenPortNumber = null)
            where TApp : IAbstractUIApp
        {
            return hostBuilder;
        }
    }
}

using NWheels.Microservices;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public static class MicroserviceHostBuilderExtensions
    {
        public static IMicroserviceHostBuilder UseRest(this IMicroserviceHostBuilder hostBuilder)
        {
            hostBuilder.UseFrameworkFeature<RestApiFeatureLoader>();
            return hostBuilder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IMicroserviceHostBuilder UseRestApiHttpEndpoint<TProtocol>(this IMicroserviceHostBuilder hostBuilder, int? listenPortNumber = null)
            where TProtocol : MessageProtocol
        {
            hostBuilder.ContributeComponents((existingComonents, newComponents) => {
                newComponents
                    .ContributeHttpEndpoint("rest-api", listenPortNumber: listenPortNumber)
                    .ServeRestApiRequests<TProtocol>();
            });

            return hostBuilder;
        }
    }
}

using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using NWheels.Communication.Api;
using NWheels.Communication.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Api;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime.Extensions
{
    public static class CompositionExtensions
    {
        public static MicroserviceHostBuilder UseRestApiModel(this MicroserviceHostBuilder hostBuilder)
        {
            hostBuilder.UseFrameworkFeature<RestApiFeature>();
            return hostBuilder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BuilderExtensions.HttpEndpointConfigurationBuilder RestApiMiddleware<TProtocol>(
            this BuilderExtensions.HttpEndpointConfigurationBuilder endpointBuilder,
            string baseUriPath,
            params Type[] resourceTypes)
            where TProtocol : class, IRestApiProtocol, ICommunicationMiddleware<HttpContext>
        {
            if (endpointBuilder == null)
            {
                throw new ArgumentNullException(nameof(endpointBuilder));
            }
            if (baseUriPath == null)
            {
                throw new ArgumentNullException(nameof(baseUriPath));
            }
            if (resourceTypes?.Length == 0)
            {
                throw new ArgumentException("At least one resource is required", nameof(resourceTypes));
            }

            endpointBuilder.Middleware<TProtocol>();

            var catalog = new ResourceCatalog(baseUriPath, resourceTypes, new Type[] { typeof(TProtocol) });
            endpointBuilder.NewComponents.RegisterComponentInstance(catalog);

            return endpointBuilder;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels
{
    namespace RestApi
    {
        using NWheels.Microservices;

        public class ResourceCatalogBuilder
        {
        }

        public static class MicroserviceHostBuilderExtensions
        {
            public static MicroserviceHostBuilder ExposeRestApiResources(
                this MicroserviceHostBuilder hostBuilder,
                Action<ResourceCatalogBuilder> buildCatalog)
            {
                return hostBuilder;
            }
        }

        public static class ComponentContainerBuilderExtensions
        {
            public static IComponentContainerBuilder RegisterRestApiResources(
                this IComponentContainerBuilder containerBuilder,
                Action<ResourceCatalogBuilder> buildCatalog)
            {
                return containerBuilder;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Runtime;

namespace NWheels.Microservices.Api
{
    public static class ComponentContributionExtensions
    {
        public static IComponentConditionBuilder ContributeLifecycleComponent<T>(this IComponentContainerBuilder builder) where T : class, ILifecycleComponent
        {
            return builder.RegisterComponentType<T>().SingleInstance().ForService<ILifecycleComponent>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MicroserviceHostBuilder UseLifecycleComponent<T>(this MicroserviceHostBuilder host) where T : class, ILifecycleComponent
        {
            host.UseComponents((existing, builder) => builder.ContributeLifecycleComponent<T>());
            return host;
        }
    }
}

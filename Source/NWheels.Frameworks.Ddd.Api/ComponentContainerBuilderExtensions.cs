using NWheels.Injection;
using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Ddd
{
    public static class ComponentContainerBuilderExtensions
    {
        public static IComponentRegistrationBuilder ContributeTransactionScript<TComponent>(this IComponentContainerBuilder containerBuilder)
            where TComponent : class
        {
            return containerBuilder.RegisterComponentType<TComponent>().InstancePerDependency();
        }
    }
}

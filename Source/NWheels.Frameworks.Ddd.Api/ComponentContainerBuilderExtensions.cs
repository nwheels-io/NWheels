using NWheels.Injection;
using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Ddd
{
    public static class ComponentContainerBuilderExtensions
    {
        public static void ContributeTransactionScript<TComponent>(this IComponentContainerBuilder containerBuilder)
            where TComponent : class
        {
            containerBuilder.RegisterInstance<TxRegistration>(new TxRegistration(typeof(TComponent)));
        }
    }
}

using NWheels.Frameworks.Uidl.Abstractions;
using NWheels.Frameworks.Uidl.Injection;
using NWheels.Frameworks.Uidl.Web;
using NWheels.Injection;
using NWheels.Microservices;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl
{
    public static class InjectionExtensions
    {
        public static IMicroserviceHostBuilder UseUidl(this IMicroserviceHostBuilder hostBuilder)
        {
            return hostBuilder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ContributeWebApp<TApp>(this IComponentContainerBuilder containerBuilder, string urlPathBase)
            where TApp : IWebApp
        {
            containerBuilder.RegisterComponentInstance(new WebAppInjectorPort(containerBuilder, typeof(TApp), urlPathBase));
        }
    }
}

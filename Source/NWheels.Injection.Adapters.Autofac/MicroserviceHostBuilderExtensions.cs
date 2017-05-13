using NWheels.Extensions;
using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Injection.Adapters.Autofac
{
    public static class MicroserviceHostBuilderExtensions
    {
        public static IMicroserviceHostBuilder UseAutofac(this IMicroserviceHostBuilder builder)
        {
            ((MicroserviceHostBuilder)builder).BootConfig.MicroserviceConfig.InjectionAdapter = new MicroserviceConfig.InjectionAdapterElement {
                Assembly = typeof(ComponentContainer).AssemblyShortName()
            };

            return builder;
        }
    }
}

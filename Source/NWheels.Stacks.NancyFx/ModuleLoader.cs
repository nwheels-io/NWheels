using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using NWheels.Endpoints;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.UI.ServerSide;

namespace NWheels.Stacks.NancyFx
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Logging().RegisterLogger<IWebApplicationLogger>();

            builder.RegisterType<WebApplicationComponent>().InstancePerDependency();

            builder.RegisterAdapter<WebAppEndpointRegistration, ILifecycleEventListener>(
                (context, registration) => {
                    var component = context.Resolve<WebApplicationComponent>(TypedParameter.From(registration));
                    return component;
                });

            builder.RegisterType<WebModuleLoggingHook>().SingleInstance();

            builder.RegisterType<ViewModelObjectFactory>().SingleInstance();
            builder.RegisterType<WebApiDispatcherFactory>().SingleInstance();

            StaticConfiguration.Caching.EnableRuntimeViewDiscovery = true;
            StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;
        }
    }
}

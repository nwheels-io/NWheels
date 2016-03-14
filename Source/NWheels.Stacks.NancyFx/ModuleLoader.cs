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
using NWheels.UI.Factories;
using NWheels.UI;

namespace NWheels.Stacks.NancyFx
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Logging().RegisterLogger<IWebApplicationLogger>();

            builder.RegisterType<WebApplicationComponent>().InstancePerDependency();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkUIConfig>();

            builder.RegisterAdapter<WebAppEndpointRegistration, ILifecycleEventListener>(
                (context, registration) => {
                    var component = context.Resolve<WebApplicationComponent>(TypedParameter.From(registration));
                    return component;
                });

            builder.RegisterType<WebModuleLoggingHook>().SingleInstance();
            builder.RegisterType<WebModuleSessionHook>().SingleInstance();

            builder.RegisterType<ViewModelObjectFactory>().As<ViewModelObjectFactory, IViewModelObjectFactory>().SingleInstance();
            builder.RegisterType<WebApiDispatcherFactory>().SingleInstance();
            builder.RegisterType<WebApplicationModule>().InstancePerDependency();

            builder.RegisterType<ViewModelObjectFactory>().As<ViewModelObjectFactory, IViewModelObjectFactory>().SingleInstance();
            builder.RegisterType<QueryResultAggregatorObjectFactory>().As<IQueryResultAggregatorObjectFactory>().SingleInstance();

            StaticConfiguration.Caching.EnableRuntimeViewDiscovery = true;
            StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;

            builder.NWheelsFeatures().Logging().RegisterLogger<IHttpApiEndpointLogger>();
            builder.RegisterType<HttpApiEndpointComponent>().InstancePerDependency();
            builder.RegisterAdapter<HttpApiEndpointRegistration, ILifecycleEventListener>((ctx, endpoint) =>
                ctx.Resolve<HttpApiEndpointComponent>(TypedParameter.From(endpoint))
            );

            builder.RegisterPipeline<ApplicationEntityService.IEntityHandlerExtension>().InstancePerDependency();
        }
    }
}

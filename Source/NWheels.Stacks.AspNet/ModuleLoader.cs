using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Autofac;
using NWheels.Endpoints;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.UI;
using NWheels.UI.Factories;

namespace NWheels.Stacks.AspNet
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DynamicControllerTypeResolver>().SingleInstance();
            builder.RegisterType<DynamicControllerSelector>().SingleInstance();
            builder.RegisterType<WebApiControllerFactory>().SingleInstance();
            builder.RegisterType<UidlApplicationController>().InstancePerRequest();
            builder.NWheelsFeatures().Logging().RegisterLogger<ControllerInitializer.ILogger>();
            builder.NWheelsFeatures().Logging().RegisterLogger<IWebApplicationLogger>();
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<ControllerInitializer>().As<ControllerInitializer>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkUIConfig>();

            builder.RegisterType<ViewModelObjectFactory>().As<ViewModelObjectFactory, IViewModelObjectFactory>().SingleInstance();
            builder.RegisterType<QueryResultAggregatorObjectFactory>().As<IQueryResultAggregatorObjectFactory>().SingleInstance();

            //builder.RegisterAdapter<HttpApiEndpointRegistration, IHttpController>((ctx, endpoint) => 
            //    ctx.Resolve<WebApiControllerFactory>().CreateControllerInstance(ctx.Resolve(endpoint.Contract))
            //).InstancePerRequest();

            builder.RegisterType<UidlApplicationComponent>().InstancePerDependency();
            builder.RegisterAdapter<WebAppEndpointRegistration, ILifecycleEventListener>(
                (context, registration) => {
                    var component = context.Resolve<UidlApplicationComponent>(TypedParameter.From(registration));
                    
                    var updater = new ContainerBuilder();
                    updater.RegisterInstance(component).As<IWebModuleContext>();
                    updater.Update(context.ComponentRegistry);

                    return component;
                });
        }
    }
}

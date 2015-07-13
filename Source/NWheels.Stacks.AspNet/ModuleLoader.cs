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

namespace NWheels.Stacks.AspNet
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DynamicControllerTypeResolver>().SingleInstance();
            builder.RegisterType<DynamicControllerSelector>().SingleInstance();
            builder.RegisterType<WebApiControllerFactory>().SingleInstance();
            builder.NWheelsFeatures().Logging().RegisterLogger<ControllerInitializer.ILogger>();
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<ControllerInitializer>().As<ControllerInitializer>();

            //builder.RegisterAdapter<HttpApiEndpointRegistration, IHttpController>((ctx, endpoint) => 
            //    ctx.Resolve<WebApiControllerFactory>().CreateControllerInstance(ctx.Resolve(endpoint.Contract))
            //).InstancePerRequest();

        }
    }
}

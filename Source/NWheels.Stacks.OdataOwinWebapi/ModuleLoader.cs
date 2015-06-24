using System.Reflection;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;
using NWheels.Endpoints;
using NWheels.Entities;
using NWheels.Hosting;
using Module = Autofac.Module;

namespace NWheels.Stacks.OdataOwinWebapi
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterType<WebApplicationComponent>().InstancePerDependency();
            builder.RegisterType<EntityServiceComponent>().InstancePerDependency();

            //builder.RegisterAdapter<WebAppEndpointRegistration, ILifecycleEventListener>(
            //    (context, endpoint) => context.Resolve<WebApplicationComponent>(TypedParameter.From(endpoint)));

            builder.RegisterAdapter<RestApiEndpointRegistration, ILifecycleEventListener>(
                (context, endpoint) => context.Resolve<EntityServiceComponent>(TypedParameter.From(endpoint)));

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
        }
    }
}

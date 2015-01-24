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
using NWheels.Entities;
using NWheels.Hosting;
using NWheels.UI.Endpoints;
using Module = Autofac.Module;

namespace NWheels.Puzzle.OdataOwinWebapi
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WebApplicationComponent>().InstancePerDependency();
            builder.RegisterType<EntityServiceComponent>().InstancePerDependency();

            builder.RegisterAdapter<IWebAppEndpoint, ILifecycleEventListener>(
                (context, endpoint) => context.Resolve<WebApplicationComponent>(TypedParameter.From(endpoint)));

            builder.RegisterAdapter<IEntityRepositoryEndpoint, ILifecycleEventListener>(
                (context, endpoint) => context.Resolve<EntityServiceComponent>(TypedParameter.From(endpoint)));

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
        }
    }
}

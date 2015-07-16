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

namespace NWheels.Stacks.ODataBreeze
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BreezeEndpointComponent>().InstancePerDependency();

            builder.RegisterAdapter<RestApiEndpointRegistration, ILifecycleEventListener>(
                (context, endpoint) => context.Resolve<BreezeEndpointComponent>(TypedParameter.From(endpoint)));

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
        }
    }
}

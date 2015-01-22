using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.Owin.Hosting;
using NWheels.Hosting;
using NWheels.UI.Endpoints;

namespace NWheels.Puzzle.OdataOwinWebapi
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAdapter<IWebAppEndpoint, ILifecycleEventListener>(
                (context, endpoint) => context.Resolve<WebApplicationEndpointComponent>(TypedParameter.From(endpoint)));
        }
    }
}

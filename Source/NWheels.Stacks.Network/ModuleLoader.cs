using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Endpoints;
using NWheels.Extensions;

namespace NWheels.Stacks.Network
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAdapter<NetworkApiEndpointRegistration, NetworkEndpointComponent>((components, registration) =>
                components.Resolve<NetworkEndpointComponent>(TypedParameter.From(registration)));

            builder.NWheelsFeatures().Logging().RegisterLogger<INetworkEndpointLogger>();
        }
    }
}

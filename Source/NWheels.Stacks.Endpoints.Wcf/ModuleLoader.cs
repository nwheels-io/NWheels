using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Endpoints;
using NWheels.Hosting;

namespace NWheels.Stacks.Endpoints.Wcf
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SoapEndpointComponent>();
            
            builder.RegisterAdapter<SoapApiEndpointRegistration, SoapEndpointComponent>(
                (context, endpoint) => context.Resolve<SoapEndpointComponent>(TypedParameter.From(endpoint)))
                .As<ILifecycleEventListener>();

        }

        #endregion
    }
}

using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;

namespace NWheels.Platform.Messaging.Adapters.AspNetKestrel
{
    [DefaultFeatureLoader]
    public class KestrelFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeAdapterComponents(IComponentContainer input, IComponentContainerBuilder output)
        {
            base.ContributeCompiledComponents(input, output);

            var allInjectors = input.ResolveAll<HttpEndpointInjectorPort>();

            foreach (var injector in allInjectors)
            {
                //output.Register<>
            }
        }
    }
}

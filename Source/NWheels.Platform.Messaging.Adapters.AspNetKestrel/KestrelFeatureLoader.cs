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
        public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            base.ContributeCompiledComponents(existingComponents, newComponents);

            var allPorts = existingComponents.ResolveAll<HttpEndpointInjectorPort>();

            foreach (var port in allPorts)
            {
                port.ConfigureHttpEndpoint(existingComponents);

                newComponents.RegisterComponentType<KestrelHttpEndpoint>()
                    .WithParameter<HttpEndpointInjectorPort>(port)
                    .SingleInstance()
                    .ForServices<IEndpoint, ILifecycleListenerComponent>();
            }
        }
    }
}

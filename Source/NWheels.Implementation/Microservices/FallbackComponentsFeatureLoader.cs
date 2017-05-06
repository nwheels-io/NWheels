using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using NWheels.Execution;

namespace NWheels.Microservices
{
    [DefaultFeatureLoader]
    public class FallbackComponentsFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            base.ContributeComponents(existingComponents, newComponents);

            newComponents.RegisterComponentType<DefaultInvocationScheduler>()
                .SingleInstance()
                .ForService<IInvocationScheduler>()
                .AsFallback();
        }
    }
}

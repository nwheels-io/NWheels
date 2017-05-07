using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;

namespace NWheels.Execution
{
    [DefaultFeatureLoader]
    public class InvocationSchedulerFeatureLoader : FeatureLoaderBase
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

using NWheels.Kernel.Api.Injection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices.Runtime
{
    [FeatureLoader(Name = "MicroserviceHostBuilderContributions")]
    public class MicroserviceHostBuilderContributionsFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            base.ContributeComponents(existingComponents, newComponents);

            var hostBuilder = existingComponents.Resolve<MicroserviceHostBuilder>();
            hostBuilder.ApplyComponentContributions(existingComponents, newComponents);
        }
    }
}

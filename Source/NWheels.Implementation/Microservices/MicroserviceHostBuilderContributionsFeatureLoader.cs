using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;

namespace NWheels.Microservices
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

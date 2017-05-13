using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices
{
    public interface IMicroserviceHostBuilder
    {
        IMicroserviceHostBuilder UseFrameworkFeature<TFeature>() where TFeature : IFeatureLoader;
        IMicroserviceHostBuilder UseApplicationFeature<TFeature>() where TFeature : IFeatureLoader;
        IMicroserviceHostBuilder ContributeComponents(Action<IComponentContainer, IComponentContainerBuilder> contribute);
    }
}

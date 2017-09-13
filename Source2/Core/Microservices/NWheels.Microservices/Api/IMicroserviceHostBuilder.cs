using NWheels.Kernel.Api.Injection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices.Api
{
    public interface IMicroserviceHostBuilder
    {
        IMicroserviceHostBuilder UseFrameworkFeature<TFeature>() where TFeature : IFeatureLoader;
        IMicroserviceHostBuilder UseApplicationFeature<TFeature>() where TFeature : IFeatureLoader;
        IMicroserviceHostBuilder ContributeComponents(Action<IComponentContainer, IComponentContainerBuilder> contribute);
    }
}

using NWheels.Injection;
using NWheels.Microservices;

namespace NWheels.Platform.Messaging
{
    [FeatureLoader(Name = "HttpEndpoint")]
    public class HttpEndpointFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
        {
            base.ContributeComponents(containerBuilder);

            containerBuilder.ContributeLifecycleListener<KestrelLifecycleListenerComponent>();
        }
    }
}
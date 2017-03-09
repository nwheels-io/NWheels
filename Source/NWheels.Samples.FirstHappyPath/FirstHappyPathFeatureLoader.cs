using NWheels.Microservices;
using NWheels.Injection;

namespace NWheels.Samples.FirstHappyPath
{
    [DefaultFeatureLoader]
    public class FirstHappyPathFeatureLoader : FeatureLoaderBase
    {
        public override void RegisterComponents(IComponentContainerBuilder containerBuilder)
        {
            containerBuilder.Register<ILifecycleListenerComponent, FirstLifecycleListenerComponent>();
        }
    }
}

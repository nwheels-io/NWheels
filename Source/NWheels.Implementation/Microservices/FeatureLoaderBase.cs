using NWheels.Injection;

namespace NWheels.Microservices
{
    public abstract class FeatureLoaderBase : IFeatureLoader
    {
        public abstract void RegisterComponents(IComponentContainerBuilder containerBuilder);

        public abstract void RegisterConfigSections();
    }
}

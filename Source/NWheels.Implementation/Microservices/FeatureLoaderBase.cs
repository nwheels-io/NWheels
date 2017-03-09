using NWheels.Injection;

namespace NWheels.Microservices
{
    public abstract class FeatureLoaderBase : IFeatureLoader
    {
        public abstract void RegisterComponents(IComponentContainerBuilder containerBuilder);

        public virtual void RegisterConfigSections()
        {
        }

        public virtual void CompileComponents(IComponentContainer input, IComponentContainerBuilder output)
        {
        }
    }
}

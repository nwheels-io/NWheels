using NWheels.Injection;

namespace NWheels.Microservices
{
    public interface IFeatureLoader
    {
        void RegisterConfigSections();

        void RegisterComponents(IComponentContainerBuilder containerBuilder);

        void CompileComponents(IComponentContainer input, IComponentContainerBuilder output);
    }
}

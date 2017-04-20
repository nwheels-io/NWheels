using NWheels.Injection;

namespace NWheels.Microservices
{
    public interface IFeatureLoader
    {
        void ContributeConfigSections();

        void ContributeComponents(IComponentContainerBuilder containerBuilder);

        void ContributeAdapterComponents(IComponentContainer input, IComponentContainerBuilder output);

        void CompileComponents(IComponentContainer input);

        void ContributeCompiledComponents(IComponentContainer input, IComponentContainerBuilder output);
    }
}

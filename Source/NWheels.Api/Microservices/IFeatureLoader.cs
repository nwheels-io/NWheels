using NWheels.Injection;

namespace NWheels.Microservices
{
    public interface IFeatureLoader
    {
        void ContributeConfigSections();

        void ContributeComponents(IComponentContainerBuilder containerBuilder);

        void CompileComponents(IInternalComponentContainer input);

        void ContributeCompiledComponents(IInternalComponentContainer input, IComponentContainerBuilder output);
    }
}

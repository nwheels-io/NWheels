using NWheels.Injection;

namespace NWheels.Microservices
{
    public interface IFeatureLoader
    {
        void ContributeConfigSections(IComponentContainerBuilder newComponents);

        void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);

        void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);

        void CompileComponents(IComponentContainer existingComponents);

        void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);
    }
}

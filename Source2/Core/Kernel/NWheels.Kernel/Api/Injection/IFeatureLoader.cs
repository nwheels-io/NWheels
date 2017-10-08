namespace NWheels.Kernel.Api.Injection
{
    public interface IFeatureLoader
    {
        void InjectBootComponents(IComponentContainer bootComponents);

        void ContributeConfigSections(IComponentContainerBuilder newComponents);

        void ContributeConfiguration(IComponentContainer existingComponents);

        void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);

        void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);

        void CompileComponents(IComponentContainer existingComponents);

        void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);
    }
}

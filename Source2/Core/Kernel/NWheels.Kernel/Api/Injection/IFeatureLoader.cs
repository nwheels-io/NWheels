namespace NWheels.Kernel.Api.Injection
{
    public interface IFeatureLoader
    {
        void ContributeConfigSections(IComponentContainerBuilder newComponents);

        void ContributeConfiguration(IComponentContainer existingComponents);

        void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);

        void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);

        void CompileComponents(IComponentContainer existingComponents);

        void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents);
    }

    public interface IFeatureLoaderPhaseExtension
    {
        void BeforeContributeConfigSections(IComponentContainer components);
        void BeforeContributeConfiguration(IComponentContainer components);
        void BeforeContributeComponents(IComponentContainer components);
        void BeforeContributeAdapterComponents(IComponentContainer comcomponents);
        void BeforeCompileComponents(IComponentContainer comcomponents);
        void BeforeContributeCompiledComponents(IComponentContainer comcomponents);
        void AfterContributeCompiledComponents(IComponentContainer comcomponents);
    }
}

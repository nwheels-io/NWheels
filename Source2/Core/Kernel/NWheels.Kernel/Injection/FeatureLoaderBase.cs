namespace NWheels.Injection
{
    public abstract class FeatureLoaderBase : IFeatureLoader
    {
        public virtual void ContributeConfigSections(IComponentContainerBuilder newComponents)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ContributeConfiguration(IComponentContainer existingComponents)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void CompileComponents(IComponentContainer existingComponents)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
        }
    }
}

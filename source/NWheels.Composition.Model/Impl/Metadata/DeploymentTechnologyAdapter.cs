namespace NWheels.Composition.Model.Impl.Metadata
{
    public abstract class DeploymentTechnologyAdapter<TMetadata> : 
        TechnologyAdapter<TMetadata>,
        IDeploymentTechnologyAdapter
        where TMetadata : IMetadataObject
    {
        void IDeploymentTechnologyAdapter.GenerateDeploymentOutputs(ITechnologyAdapterContext context)
        {
            this.GenerateDeploymentOutputs(new TechnologyAdapterContext<TMetadata>(context));
        }

        protected abstract void GenerateDeploymentOutputs(TechnologyAdapterContext<TMetadata> context);
    }
}

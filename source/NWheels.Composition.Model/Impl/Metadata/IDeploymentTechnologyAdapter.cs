using MetaPrograms;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public interface IDeploymentTechnologyAdapter : ITechnologyAdapter
    {
        void GenerateDeploymentOutputs(ITechnologyAdapterContext context);
    }
}

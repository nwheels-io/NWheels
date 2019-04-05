using MetaPrograms;

namespace NWheels.Composition.Model.Metadata
{
    public interface ITechnologyAdapter
    {
        void Execute(ITechnologyAdapterContext context);
    }

    public interface ITechnologyAdapterContext
    {
        IMetadataObject Input { get; }
        ICodeGeneratorOutput Output { get; }
    }
}

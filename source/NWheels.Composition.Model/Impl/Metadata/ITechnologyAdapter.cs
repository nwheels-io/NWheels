using MetaPrograms;

namespace NWheels.Composition.Model.Impl.Metadata
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

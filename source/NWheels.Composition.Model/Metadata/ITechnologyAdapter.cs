using MetaPrograms;

namespace NWheels.Composition.Model.Metadata
{
    public interface ITechnologyAdapter
    {
        void Execute(ITechnologyAdapterContext context);
    }

    public interface ITechnologyAdapterContext
    {
        MetadataObject Input { get; }
        ICodeGeneratorOutput Output { get; }
    }
}

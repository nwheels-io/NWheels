namespace NWheels.Composition.Model.Metadata
{
    public interface IAnyModelParser
    {
    }

    public interface IModelParser<TCompiled> : IAnyModelParser
    {
    }

    public interface IAnyRootModelParser : IAnyModelParser
    {
    }

    public interface IRootModelParser<TCompiled> : IAnyRootModelParser, IModelParser<TCompiled>
    {
    }

    public abstract class ModelParser<TCompiled> : IModelParser<TCompiled>
    {
    }
}

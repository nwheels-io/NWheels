namespace NWheels.Composition.Model.Parsers
{
    public interface IAnyModelUnitParser
    {
    }

    public interface IModelUnitParser<TUnit> : IAnyModelUnitParser
    {
    }

    public interface IAnyRootUnitParser : IAnyModelUnitParser
    {
    }

    public interface IRootUnitParser<TUnit> : IAnyRootUnitParser, IModelUnitParser<TUnit>
    {
    }

    public abstract class ModelUnitParser<TUnit> : IModelUnitParser<TUnit>
    {
    }
}

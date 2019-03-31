namespace NWheels.Composition.Model.Parsers
{
    public interface IParserContributionRegistry
    {
        void ContributeNonRootUnitParser<TUnit, TParser>() 
            where TUnit : ProgrammingModelUnit
            where TParser : ModelUnitParser<TUnit>;

        void ContributeRootUnitParser<TUnit, TParser>() 
            where TUnit : ProgrammingModelUnit
            where TParser : ModelUnitParser<TUnit>, IRootUnitParser<TUnit>;
    }
}
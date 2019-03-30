namespace NWheels.Composition.Model.Parsers
{
    public abstract class ProgrammingModelEntryPoint
    {
        public abstract void ContributeParsers(IParserContributionRegistry parsers);

        public interface IParserContributionRegistry
        {
            void ContributeNonRootParser<TUnit, TParser>() where TParser : ModelUnitParser<TUnit>;
            void ContributeRootParser<TUnit, TParser>() where TParser : ModelUnitParser<TUnit>, IRootUnitParser<TUnit>;
        }
    }
}

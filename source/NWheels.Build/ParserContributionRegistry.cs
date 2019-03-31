using Autofac;
using NWheels.Composition.Model.Parsers;

namespace NWheels.Build
{
    public class ParserContributionRegistry : IParserContributionRegistry
    {
        public void RegisterParsers(ContainerBuilder builder)
        {
            throw new System.NotImplementedException();
        }

        void IParserContributionRegistry.ContributeNonRootUnitParser<TUnit, TParser>()
        {
            throw new System.NotImplementedException();
        }

        void IParserContributionRegistry.ContributeRootUnitParser<TUnit, TParser>()
        {
            throw new System.NotImplementedException();
        }
    }
}
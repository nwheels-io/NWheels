using Autofac;
using NWheels.Composition.Model.Parsers;

namespace NWheels.Build
{
    public class ParserContributionRegistry : ProgrammingModelEntryPoint.IParserContributionRegistry
    {
        public void RegisterParsers(ContainerBuilder builder)
        {
            throw new System.NotImplementedException();
        }

        void ProgrammingModelEntryPoint.IParserContributionRegistry.ContributeNonRootParser<TUnit, TParser>()
        {
            throw new System.NotImplementedException();
        }

        void ProgrammingModelEntryPoint.IParserContributionRegistry.ContributeRootParser<TUnit, TParser>()
        {
            throw new System.NotImplementedException();
        }
    }
}
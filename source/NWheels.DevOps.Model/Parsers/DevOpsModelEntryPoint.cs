using NWheels.Composition.Model.Parsers;

[assembly: ProgrammingModel(typeof(NWheels.DevOps.Model.Parsers.DevOpsModelEntryPoint))]

namespace NWheels.DevOps.Model.Parsers
{
    public class DevOpsModelEntryPoint : ProgrammingModelEntryPoint
    {
        public override void ContributeParsers(IParserContributionRegistry parsers)
        {
            throw new System.NotImplementedException();
        }
    }
}

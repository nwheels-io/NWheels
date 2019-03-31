using NWheels.Composition.Model.Parsers;

[assembly: ProgrammingModel(typeof(NWheels.UI.Model.Parsers.UIModelEntryPoint))]

namespace NWheels.UI.Model.Parsers
{

    public class UIModelEntryPoint : ProgrammingModelEntryPoint
    {
        public override void ContributeParsers(IParserContributionRegistry parsers)
        {
            throw new System.NotImplementedException();
        }
    }
}
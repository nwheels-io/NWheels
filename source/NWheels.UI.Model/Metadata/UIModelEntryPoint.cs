using NWheels.Composition.Model.Metadata;
using NWheels.UI.Model.Metadata;
using NWheels.UI.Model.Metadata.Web;
using NWheels.UI.Model.Web;

[assembly: ProgrammingModel(typeof(UIModelEntryPoint))]

namespace NWheels.UI.Model.Metadata
{
    public class UIModelEntryPoint : ProgrammingModelEntryPoint
    {
        public override void ContributeParsers(IModelParserRegistry parsers)
        {
            parsers.RegisterParser<WebAppParser>();
        }
    }
}
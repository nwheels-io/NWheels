using System.Xml.Linq;
using NWheels.UI.Model.Impl.Metadata;

namespace NWheels.UI.Adapters.Web.Wix.Components
{
    public class SeatingPlanGenerator : WixComponentGenerator<SeatingPlanMetadata>
    {
        public SeatingPlanGenerator(UIComponentMetadata compMeta) : base(compMeta)
        {
        }

        public override WixComponentDefinition GenerateDefinition()
        {
            return new WixComponentDefinition();
        }

        public override XElement GenerateHtml()
        {
            return new XElement("SaetingMapHtml");
        }
    }
}
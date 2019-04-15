using System.Collections.Generic;
using System.Xml.Linq;
using NWheels.Composition.Model.Impl.Metadata;
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
            return new WixComponentDefinition {
                ComponentType = "wysiwyg.viewer.components.HtmlComponent",
                Skin = "wysiwyg.viewer.skins.HtmlComponentSkin",
                Style = "htco1",
                Layout = new WixComponentLayout {
                    Width = 980,
                    Height = 359,
                    X = 0,
                    Y = 0
                },
                Data = new Dictionary<string, object> {
                    {"type", "HtmlComponent"},
                    {"sourceType", "tempUrl"},
                    {"metadata", 
                        new Dictionary<string, object> {
                            {"schemaVersion", "1.0"},    
                            {"isPreset", false},    
                            {"isHidden", false}    
                        } 
                    },
                    {"freezeFrame", false}
                },
                Connections = new WixComponentConnections {
                    Items = new List<WixComponentConnectionItem> {
                        new WixComponentConnectionItem {
                            Type = "WixCodeConnectionItem",
                            Role = "html1" //TODO: auto-generate this
                        }
                    }
                }
            };
        }

        public override string GenerateHtml()
        {
            return EmbeddedResource.LoadAsString("Web.Wix.Code.seating-plan-component.html");
        }
    }
}

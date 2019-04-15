using System.Collections.Generic;
using NWheels.UI.Model.Impl.Metadata;

namespace NWheels.UI.Adapters.Web.Wix.Components
{
    public class TextContentGenerator : WixComponentGenerator<TextContentMetadata>
    {
        public TextContentGenerator(UIComponentMetadata compMeta) : base(compMeta)
        {
        }

        public override WixComponentDefinition GenerateDefinition()
        {
            return new WixComponentDefinition {
                ComponentType = "wysiwyg.viewer.components.WRichText",
                Skin = "wysiwyg.viewer.skins.WRichTextNewSkin",
                Style = "txtNew",
                Layout = new WixComponentLayout {
                    Width = 980,
                    Height = 31,
                    X = 0,
                    Y = GridLayoutGenerator.NextY
                },
                Data = new Dictionary<string, object> {
                    {"type", "StyledText"},
                    {"text", $"<p>{CompMeta.Text ?? ""}</p>"},
                    {"linkList", new string[0]}
                },
                Props = new Dictionary<string, object> {
                    {"type", "WRichTextProperties"},
                    {"isHidden", false}, //TODO: conditional visibility? 
                    {"brightness", 1},
                    {"packed", false},
                    {"metadata", new WixComponentMetadata()}
                },
                Connections = new WixComponentConnections {
                    Items = new List<WixComponentConnectionItem> {
                        new WixComponentConnectionItem {
                            Type = "WixCodeConnectionItem",
                            Role = CompMeta.Header.Name
                        }
                    }
                }
            };
        }
    }
}
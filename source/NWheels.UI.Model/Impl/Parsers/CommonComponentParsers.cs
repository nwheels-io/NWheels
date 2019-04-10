using System.Reflection;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;

namespace NWheels.UI.Model.Impl.Parsers
{
    public class CommonComponentParsers
    {
        public TextContentMetadata TextContent(PreprocessedProperty prop)
        {
            return new TextContentMetadata(MetadataObjectHeader.NoSourceType()) {
                Text = prop.ConstructorArguments[0].ClrValue as string 
            };
        }
    }
}

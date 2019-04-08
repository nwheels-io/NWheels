using System.Linq;
using System.Runtime.InteropServices;
using MetaPrograms.Expressions;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;

namespace NWheels.UI.Model.Impl.Parsers.Web
{
    public class WebPageParser : IModelParser
    {
        public MetadataObject CreateMetadataObject(IModelPreParserContext context)
        {
            return new WebPageMetadata(context.Input);
        }

        public void Parse(IModelParserContext context)
        {
            var pageMeta = (WebPageMetadata) context.Output;

            foreach (var prop in context.Input.GetAllProperties())
            {
                if (context.TryGetMetadata(prop.Type) is UIComponentMetadata compMeta)
                {
                    pageMeta.Components.Add(compMeta);
                }
                else
                {
                    pageMeta.Components.Add(GetWellKnownCompMeta(prop));
                }
            }

            UIComponentMetadata GetWellKnownCompMeta(PreprocessedProperty prop)
            {
                if (prop.Type == context.Code.GetClrTypeMember<TextContent>())
                {
                    return new TextContentMetadata(MetadataObjectHeader.NoSourceType()) {
                        Text = prop.ConstructorArguments[0].ClrValue as string 
                    };
                }

                return null;
            }
        }
    }
}

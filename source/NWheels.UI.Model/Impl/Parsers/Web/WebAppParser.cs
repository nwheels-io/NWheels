using System.Linq;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;

namespace NWheels.UI.Model.Impl.Parsers.Web
{
    public class WebAppParser : IModelParser
    {
        public MetadataObject CreateMetadataObject(IModelPreParserContext context)
        {
            return new WebAppMetadata(context.Input);
        }

        public void Parse(IModelParserContext context)
        {
            var siteMeta = (WebAppMetadata) context.Output;
 
            var pageMetaQuery = context.Input.GetAllProperties()
                .Select(prop => context.GetMetadata(prop.Type))
                .OfType<WebPageMetadata>();
            
            siteMeta.Pages.AddRange(pageMetaQuery);
        }
    }
}

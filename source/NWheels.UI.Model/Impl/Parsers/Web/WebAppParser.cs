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
                .Select(prop => new {
                    prop, 
                    meta = context.GetMetadata(prop.Type) as WebPageMetadata
                })
                .Where(tuple => tuple.meta != null)
                .Select(tuple => new WebAppMetadata.PageItem {
                    Name = tuple.prop.Name,
                    Metadata = tuple.meta
                });
            
            siteMeta.Pages.AddRange(pageMetaQuery);
        }
    }
}

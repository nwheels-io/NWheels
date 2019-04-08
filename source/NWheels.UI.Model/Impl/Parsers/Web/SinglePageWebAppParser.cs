using System.Linq;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;

namespace NWheels.UI.Model.Impl.Parsers.Web
{
    public class SinglePageWebAppParser : IModelParser
    {
        public MetadataObject CreateMetadataObject(IModelPreParserContext context)
        {
            return new WebAppMetadata(context.Input);
        }

        public void Parse(IModelParserContext context)
        {
            var siteMeta = (WebAppMetadata) context.Output;
            var indexPageType = context.Input.ConcreteType.BaseType.GenericArguments[0];
            var indexPageMeta = (WebPageMetadata)context.GetMetadata(indexPageType);
 
            siteMeta.Pages.Add(new WebAppMetadata.PageItem {
                IsIndex = true,
                Name = "Index", //siteMeta.Title ?? MetadataObject.Header(siteMeta).Name,
                Metadata = indexPageMeta
            });
        }
    }
}

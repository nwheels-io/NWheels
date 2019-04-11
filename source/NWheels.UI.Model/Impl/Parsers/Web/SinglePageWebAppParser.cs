using System.Linq;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;

namespace NWheels.UI.Model.Impl.Parsers.Web
{
    public class SinglePageWebAppParser : IModelParser
    {
        public MetadataObject CreateMetadataObject(IModelPreParserContext context)
        {
            var parentProp = context.Input.ReferencedBy.FirstOrDefault();
            var header = (parentProp != null
                ? new MetadataObjectHeader(
                    context.Input, 
                    name: parentProp.Name, 
                    namespaceName: parentProp.Property.DeclaringType.Namespace,
                    qualifiedName: $"{parentProp.Property.DeclaringType.FullName}.{parentProp.Name}")
                : new MetadataObjectHeader(context.Input));

            return new WebAppMetadata(header);
        }

        public void Parse(IModelParserContext context)
        {
            var siteMeta = (WebAppMetadata) context.Output;
            
            // TODO: allow inheritance use case as well (...ConcreteType.BaseType.GenericArguments[0])
            // depends on MetaPrograms populating TypeMember.GenericTypeDefinition
            var indexPageType = context.Input.ConcreteType.GenericArguments[0]; 
            
            var indexPageMeta = (WebPageMetadata)context.GetMetadata(indexPageType);

            siteMeta.Pages.Add(new WebAppMetadata.PageItem {
                IsIndex = true,
                Name = "Index", //siteMeta.Title ?? MetadataObject.Header(siteMeta).Name,
                Metadata = indexPageMeta
            });
        }
    }
}

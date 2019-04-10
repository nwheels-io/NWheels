using System.Linq;
using System.Runtime.InteropServices;
using MetaPrograms.Expressions;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.Composition.Model.Impl.Parsers;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;

namespace NWheels.UI.Model.Impl.Parsers.Web
{
    public class WebPageParser : IModelParser, IModelParserWithInit
    {
        private PropertyParsersMap _parsersMap;
        
        public void Initialize(IModelPreParserContext context)
        {
            _parsersMap = new PropertyParsersMap(context);
            _parsersMap.RegisterParsers(new CommonComponentParsers());
            _parsersMap.RegisterParsers(new WebComponentParsers());
        }

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
                var parser = _parsersMap.GetParser(prop);
                var compMeta = (UIComponentMetadata)parser(prop, context);
                return compMeta;
            }
        }
    }
}

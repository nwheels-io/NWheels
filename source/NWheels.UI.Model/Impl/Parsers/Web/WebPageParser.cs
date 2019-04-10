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
        private PropertyParsersMap _propParsers;
        
        public void Initialize(IModelPreParserContext context)
        {
            _propParsers = new PropertyParsersMap(context);
            _propParsers.RegisterParsers(new CommonComponentParsers());
            _propParsers.RegisterParsers(new WebComponentParsers());
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
                var parser = _propParsers.GetParser(prop);
                var propMeta = parser(prop, context);

                if (propMeta is UIComponentMetadata compMeta)
                {
                    pageMeta.Components.Add(compMeta);
                }
            }
        }
    }
}

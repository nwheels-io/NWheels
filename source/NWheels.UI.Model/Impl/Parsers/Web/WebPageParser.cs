using System;
using System.Linq;
using System.Runtime.InteropServices;
using MetaPrograms.Expressions;
using MetaPrograms.Members;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.Composition.Model.Impl.Parsers;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;
using NWheels.UI.Model.Web;

namespace NWheels.UI.Model.Impl.Parsers.Web
{
    public class WebPageParser : IModelParser, IModelParserWithInit
    {
        private PropertyParsersMap _propParsers;
        
        public void Initialize(IModelPreParserContext context)
        {
            _propParsers = new PropertyParsersMap(context);
            _propParsers.RegisterParserMethods(new ComponentParsers());
            _propParsers.RegisterParserMethods(new WebComponentParsers());
        }

        public MetadataObject CreateMetadataObject(IModelPreParserContext context)
        {
            return new WebPageMetadata(context.Input);
        }

        public void Parse(IModelParserContext context)
        {
            var pageMeta = (WebPageMetadata) context.Output;
            pageMeta.Title = ParseTitle();
            
            foreach (var prop in context.Input.GetAllProperties())
            {
                var parser = _propParsers.GetParser(prop);
                var propMeta = parser(prop, context);

                switch (propMeta)
                {
                    case UIComponentMetadata compMeta:
                        pageMeta.Components.Add(compMeta);
                        break;
                    case IBackendApiMetadata apiMeta:
                        pageMeta.BackendApis.Add(apiMeta);
                        break;
                    default:
                        pageMeta.UnknownChildren.Add(propMeta);
                        break;
                }
            }

            string ParseTitle()
            {
                return context.Input.ConcreteType.TryGetPropertyValue<string>(nameof(WebPage.Title));
            }
        }
        
    }
}

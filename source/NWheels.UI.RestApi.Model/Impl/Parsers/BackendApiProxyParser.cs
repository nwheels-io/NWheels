using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.RestApi.Model.Impl.Metadata;

namespace NWheels.UI.RestApi.Model.Impl.Parsers
{
    public class BackendApiProxyParser : IModelParser
    {
        public MetadataObject CreateMetadataObject(IModelPreParserContext context)
        {
            return new BackendApiMetadata(context.Input);
        }

        public void Parse(IModelParserContext context)
        {
            //TODO: implement this
            //context.Input.ParsedMetadata
        }
    }
}

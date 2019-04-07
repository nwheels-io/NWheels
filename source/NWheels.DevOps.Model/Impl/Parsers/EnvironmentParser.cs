using System.Linq;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.DevOps.Model.Impl.Metadata;

namespace NWheels.DevOps.Model.Impl.Parsers
{
    public class EnvironmentParser : IModelParser
    {
        public MetadataObject CreateMetadataObject(IModelPreParserContext context)
        {
            return new EnvironmentMetadata(context.Input);
        }

        public void Parse(IModelParserContext context)
        {
            var output = (EnvironmentMetadata) context.Output;
            
            output.Dummy = "hello from environment parser!";
        }
    }
}

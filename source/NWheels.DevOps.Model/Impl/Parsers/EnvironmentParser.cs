using System.Linq;
using NWheels.Composition.Model.Metadata;
using NWheels.DevOps.Model.Impl.Metadata;

namespace NWheels.DevOps.Model.Impl.Parsers
{
    public class EnvironmentParser : IModelParser
    {
        public void Parse(IModelParserContext context)
        {
            var metadata = new EnvironmentMetadata(new MetadataObjectHeader(context.Input)) {
                Dummy = "hello from environment parser!"
            };

            context.Input.ParsedMetadata = metadata;
            context.Output.Add(metadata);
        }
    }
}

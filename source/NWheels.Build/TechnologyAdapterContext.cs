using System;
using Buildalyzer;
using MetaPrograms;
using NWheels.Composition.Model.Metadata;

namespace NWheels.Build
{
    public class TechnologyAdapterContext : ITechnologyAdapterContext
    {
        public TechnologyAdapterContext(MetadataObject input, ICodeGeneratorOutput output)
        {
            Input = input;
            Output = output;
        }

        public MetadataObject Input { get; }
        public ICodeGeneratorOutput Output { get; }
    }
}

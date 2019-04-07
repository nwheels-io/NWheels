using System;
using Buildalyzer;
using MetaPrograms;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Build
{
    public class TechnologyAdapterContext : ITechnologyAdapterContext
    {
        public TechnologyAdapterContext(IMetadataObject input, ICodeGeneratorOutput output)
        {
            Input = input;
            Output = output;
        }

        public IMetadataObject Input { get; }
        public ICodeGeneratorOutput Output { get; }
    }
}

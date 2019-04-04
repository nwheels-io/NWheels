using System;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public abstract class MetadataObject
    {
        public string Name { get; }

        public PreprocessedType Preprocessor { get; }
        public abstract Type TechnologyAdapterType { get; }
        public abstract Type TechnologyAdapterConfigType { get; }
    }
}

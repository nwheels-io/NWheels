using System;

namespace NWheels.Composition.Model.Metadata
{
    public abstract class MetadataObject
    {
        public string Name { get; }
        
        public Type TechnologyAdapterType { get; }
    }
}
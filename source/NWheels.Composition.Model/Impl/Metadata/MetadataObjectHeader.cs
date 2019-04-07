using System.Collections.Generic;
using System.Linq;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public class MetadataObjectHeader
    {
        public MetadataObjectHeader(PreprocessedType sourceType)
        {
            SourceType = sourceType;
            QualifiedName = sourceType?.ConcreteType.FullName ?? string.Empty;
            TechnologyAdapters = sourceType?.ReferencedBy
                .Where(refProp => refProp.TechnologyAdapter != null)
                .Select(refProp => TechnologyAdapterMetadata.FromSource(refProp.TechnologyAdapter))
                .ToArray()
                ?? new TechnologyAdapterMetadata[0];
        }

        public string QualifiedName { get; }
        public PreprocessedType SourceType { get; }
        public IReadOnlyList<TechnologyAdapterMetadata> TechnologyAdapters { get; }
        public List<string> DeployableFiles { get; } = new List<string>();

        public static MetadataObjectHeader NoSourceType()
        {
            return new MetadataObjectHeader(null);
        }

        public static implicit operator MetadataObjectHeader(PreprocessedType sourceType)
        {
            return new MetadataObjectHeader(sourceType);
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public class MetadataObjectHeader
    {
        public MetadataObjectHeader(PreprocessedType sourceType)
        {
            QualifiedName = sourceType.ConcreteType.FullName;
            SourceType = sourceType;
            TechnologyAdapters = sourceType.ReferencedBy
                .Where(refProp => refProp.TechnologyAdapter != null)
                .Select(refProp => TechnologyAdapterMetadata.FromSource(refProp.TechnologyAdapter))
                .ToArray();
        }

        public string QualifiedName { get; }
        public PreprocessedType SourceType { get; }
        public IReadOnlyList<TechnologyAdapterMetadata> TechnologyAdapters { get; }
    }
}

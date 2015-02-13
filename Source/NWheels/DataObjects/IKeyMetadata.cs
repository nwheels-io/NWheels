using System.Collections.Generic;

namespace NWheels.DataObjects
{
    public interface IKeyMetadata : IMetadataElement
    {
        string Name { get; }
        KeyKind Kind { get; }
        IReadOnlyList<IPropertyMetadata> Properties { get; }
    }
}

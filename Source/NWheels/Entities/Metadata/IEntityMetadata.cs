using System;
using System.Collections.Generic;

namespace NWheels.Entities.Metadata
{
    public interface IEntityMetadata
    {
        string Name { get; }
        Type ContractType { get; }
        Type ImplementationType { get; }
        bool IsAbstract { get; }
        IEntityMetadata BaseEntity { get; }
        IReadOnlyList<IEntityMetadata> DerivedEntities { get; }
        IReadOnlyList<IEntityPropertyMetadata> Properties { get; }
        IEntityKeyMetadata PrimaryKey { get; }
        IReadOnlyList<IEntityKeyMetadata> AllKeys { get; }
        string DefaultDisplayFormat { get; }
        IReadOnlyList<IEntityPropertyMetadata> DefaultDisplayProperties { get; }
        IReadOnlyList<IEntityPropertyMetadata> DefaultSortProperties { get; }
        IEntityRelationalMapping RelationalMapping { get; }
    }
}

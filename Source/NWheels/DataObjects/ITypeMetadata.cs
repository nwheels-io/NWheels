using System;
using System.Collections.Generic;

namespace NWheels.DataObjects
{
    public interface ITypeMetadata
    {
        string Name { get; }
        Type ContractType { get; }
        Type ImplementationType { get; }
        bool IsAbstract { get; }
        ITypeMetadata BaseType { get; }
        IReadOnlyList<ITypeMetadata> DerivedTypes { get; }
        IReadOnlyList<IPropertyMetadata> Properties { get; }
        IKeyMetadata PrimaryKey { get; }
        IReadOnlyList<IKeyMetadata> AllKeys { get; }
        string DefaultDisplayFormat { get; }
        IReadOnlyList<IPropertyMetadata> DefaultDisplayProperties { get; }
        IReadOnlyList<IPropertyMetadata> DefaultSortProperties { get; }
        ITypeRelationalMapping RelationalMapping { get; }
    }
}

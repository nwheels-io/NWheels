using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NWheels.Entities.Metadata
{
    public interface IEntityPropertyMetadata
    {
        string Name { get; }
        EntityPropertyKind Kind { get; }
        Type ClrType { get; }
        DataType SemanticDataType { get; }
        PropertyInfo ContractPropertyInfo { get; }
        PropertyInfo ImplementationPropertyInfo { get; }
        IEntityRelationMetadata Relation { get; }
        IEntityPropertyValidationMetadata Validation { get; }
        string DefaultDisplayName { get; }
        string DefaultDisplayFormat { get; }
        bool DefaultSortAscending { get; }
        IEntityPropertyRelationalMapping RelationalMapping { get; }
    }
}

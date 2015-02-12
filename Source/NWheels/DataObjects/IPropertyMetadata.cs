using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NWheels.DataObjects
{
    public interface IPropertyMetadata
    {
        string Name { get; }
        PropertyKind Kind { get; }
        Type ClrType { get; }
        DataType SemanticDataType { get; }
        PropertyInfo ContractPropertyInfo { get; }
        PropertyInfo ImplementationPropertyInfo { get; }
        IRelationMetadata Relation { get; }
        IPropertyValidationMetadata Validation { get; }
        string DefaultDisplayName { get; }
        string DefaultDisplayFormat { get; }
        bool DefaultSortAscending { get; }
        IPropertyRelationalMapping RelationalMapping { get; }
    }
}

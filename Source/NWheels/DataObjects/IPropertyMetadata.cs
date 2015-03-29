using System;
using System.Reflection;

namespace NWheels.DataObjects
{
    public interface IPropertyMetadata : IMetadataElement
    {
        string Name { get; }
        PropertyKind Kind { get; }
        Type ClrType { get; }
        ISemanticDataType SemanticType { get; }
        PropertyContractAttribute ContractAttribute { get; }
        PropertyInfo ContractPropertyInfo { get; }
        PropertyInfo ImplementationPropertyInfo { get; }
        IRelationMetadata Relation { get; }
        IPropertyValidationMetadata Validation { get; }
        object DefaultValue { get; }
        string DefaultDisplayName { get; }
        string DefaultDisplayFormat { get; }
        bool DefaultSortAscending { get; }
        IPropertyRelationalMapping RelationalMapping { get; }
    }
}

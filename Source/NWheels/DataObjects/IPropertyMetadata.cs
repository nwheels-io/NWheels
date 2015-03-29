using System;
using System.Collections.Generic;
using System.Reflection;

namespace NWheels.DataObjects
{
    public interface IPropertyMetadata : IMetadataElement
    {
        bool HasContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute;
        TAttribute TryGetContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute;
        string Name { get; }
        PropertyKind Kind { get; }
        Type ClrType { get; }
        ISemanticDataType SemanticType { get; }
        IReadOnlyList<PropertyContractAttribute> ContractAttributes { get; }
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

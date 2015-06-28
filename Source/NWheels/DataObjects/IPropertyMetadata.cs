using System;
using System.Collections.Generic;
using System.Reflection;
using NWheels.DataObjects.Core;

namespace NWheels.DataObjects
{
    public interface IPropertyMetadata : IMetadataElement
    {
        bool HasContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute;
        TAttribute TryGetContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute;
        bool TryGetImplementationPropertyInfo(object implementorKey, out PropertyInfo implementationProperty);
        ITypeMetadata DeclaringContract { get; }
        string Name { get; }
        string ContractQualifiedName { get; }
        PropertyKind Kind { get; }
        PropertyRole Role { get; }
        Type ClrType { get; }
        ISemanticDataType SemanticType { get; }
        PropertyAccess Access { get; }
        bool IsSensitive { get; }
        IReadOnlyList<PropertyContractAttribute> ContractAttributes { get; }
        PropertyInfo ContractPropertyInfo { get; }
        IRelationMetadata Relation { get; }
        IPropertyValidationMetadata Validation { get; }
        object DefaultValue { get; }
        Type DefaultValueGeneratorType { get; }
        string DefaultDisplayName { get; }
        string DefaultDisplayFormat { get; }
        bool DefaultSortAscending { get; }
        IPropertyRelationalMapping RelationalMapping { get; }
    }
}

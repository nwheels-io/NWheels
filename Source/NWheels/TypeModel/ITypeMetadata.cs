using System;
using System.Collections.Generic;
using System.Reflection;
using NWheels.Conventions.Core;
using NWheels.DataObjects.Core;

namespace NWheels.DataObjects
{
    public interface ITypeMetadata : IMetadataElement
    {
        IPropertyMetadata GetPropertyByName(string name);
        IPropertyMetadata GetPropertyByDeclaration(PropertyInfo declarationInContract);
        bool TryGetPropertyByName(string name, out IPropertyMetadata property);
        bool TryGetPropertyByDeclaration(PropertyInfo declarationInContract, out IPropertyMetadata property);
        string Name { get; }
        string NamespaceQualifier { get; }
        string QualifiedName { get; }
        Type ContractType { get; }
        IReadOnlyList<Type> MixinContractTypes { get; }
        Type DomainObjectType { get; }
        bool TryGetImplementation(Type factoryType, out Type implementationType);
        IEnumerable<KeyValuePair<Type, Type>> GetAllImplementations();
        bool IsEntity { get; }
        bool IsEntityPart { get; }
        bool IsAbstract { get; }
        ITypeMetadata BaseType { get; }
        IReadOnlyList<ITypeMetadata> DerivedTypes { get; }
        int InheritanceDepth { get; }
        IReadOnlyList<IPropertyMetadata> Properties { get; }
        IKeyMetadata PrimaryKey { get; }
        IReadOnlyList<IKeyMetadata> AllKeys { get; }
        PropertyMetadataBuilder PartitionProperty { get; }
        string DefaultDisplayFormat { get; }
        IReadOnlyList<IPropertyMetadata> DefaultDisplayProperties { get; }
        IReadOnlyList<IPropertyMetadata> DefaultSortProperties { get; }
        ITypeRelationalMapping RelationalMapping { get; }
    }
}

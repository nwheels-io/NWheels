using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        LambdaExpression MakePropertyExpression(IPropertyMetadata property);
        IQueryable<TBase> MakeOfType<TBase>(IQueryable<TBase> query);
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
        bool IsViewModel { get; }
        bool IsAbstract { get; }
        ITypeMetadata BaseType { get; }
        IReadOnlyList<ITypeMetadata> DerivedTypes { get; }
        int InheritanceDepth { get; }
        IReadOnlyList<IPropertyMetadata> Properties { get; }
        IReadOnlyCollection<MethodInfo> Methods { get; }
        IKeyMetadata PrimaryKey { get; }
        IReadOnlyList<IKeyMetadata> AllKeys { get; }
        IPropertyMetadata EntityIdProperty { get; }
        IPropertyMetadata PartitionProperty { get; }
        string DefaultDisplayFormat { get; }
        IReadOnlyList<IPropertyMetadata> DefaultDisplayProperties { get; }
        IReadOnlyList<IPropertyMetadata> DefaultSortProperties { get; }
        ITypeRelationalMapping RelationalMapping { get; }
    }
}

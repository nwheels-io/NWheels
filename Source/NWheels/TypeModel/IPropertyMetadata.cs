using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.DataObjects.Core;
using NWheels.Entities;

namespace NWheels.DataObjects
{
    public interface IPropertyMetadata : IMetadataElement
    {
        bool HasContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute;
        TAttribute TryGetContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute;
        bool TryGetImplementation(Type factoryType, out PropertyInfo implementationProperty);
        IEnumerable<KeyValuePair<Type, PropertyInfo>> GetAllImplementations();
        bool TryGetDefaultValueOperand(MethodWriterBase writer, out IOperand<TypeTemplate.TProperty> valueOperand);
        object ParseStringValue(string s);
        LambdaExpression MakePropertyExpression<TEntity>();
        LambdaExpression MakePropertyAsObjectExpression<TEntity>();
        Expression<Func<TEntity, bool>> MakeBinaryExpression<TEntity>(IPropertyMetadata[] navigationPath, object value, Func<Expression, Expression, Expression> binaryFactory);
        Expression<Func<TEntity, bool>> MakeBinaryExpression<TEntity>(IPropertyMetadata[] navigationPath, string valueString, Func<Expression, Expression, Expression> binaryFactory);
        Expression<Func<TEntity, bool>> MakeForeignKeyBinaryExpression<TEntity>(IPropertyMetadata[] navigationPath, object value, Func<Expression, Expression, Expression> binaryFactory);
        Expression<Func<TEntity, bool>> MakeForeignKeyBinaryExpression<TEntity>(IPropertyMetadata[] navigationPath, string valueString, Func<Expression, Expression, Expression> binaryFactory);
        IQueryable<TEntity> MakeOrderBy<TEntity>(IPropertyMetadata[] navigationPath, IQueryable<TEntity> query, bool first, bool ascending);
        object ReadValue(object target);
        void WriteValue(object target, object value);
        ITypeMetadata DeclaringContract { get; }
        string Name { get; }
        string ContractQualifiedName { get; }
        int PropertyIndex { get; }
        PropertyKind Kind { get; }
        PropertyRole Role { get; }
        Type ClrType { get; }
        ISemanticDataType SemanticType { get; }
        PropertyAccess Access { get; }
        bool IsSensitive { get; }
        bool IsCollection { get; }
        bool IsReadOnly { get; }
        bool IsCalculated { get; }
        bool IsCalculatedForceUpdateOnSave { get; }
        bool IsPartition { get; }
        string PartitionValuePropertyName { get; }
        int? NumericPrecision { get; }
        IReadOnlyList<PropertyContractAttribute> ContractAttributes { get; }
        PropertyInfo ContractPropertyInfo { get; }
        IRelationMetadata Relation { get; }
        IPropertyValidationMetadata Validation { get; }
        object DefaultValue { get; }
        Type DefaultValueGeneratorType { get; }
        bool? DefaultDisplayVisible { get; }
        string DefaultDisplayName { get; }
        string DefaultDisplayFormat { get; }
        bool DefaultSortAscending { get; }
        IPropertyRelationalMapping RelationalMapping { get; }
    }
}

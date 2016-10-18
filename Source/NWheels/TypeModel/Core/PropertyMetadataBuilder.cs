using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.Entities;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;

namespace NWheels.DataObjects.Core
{
    public class PropertyMetadataBuilder : MetadataElement<IPropertyMetadata>, IPropertyMetadata
    {
        private readonly ConcurrentDictionary<Type, PropertyInfo> _implementationPropertyByFactoryType;
        private readonly object _dynamicMethodSyncRoot = new object();
        private Func<object, object> _valueReaderDynamicMethod = null;
        private Action<object, object> _valueWriterDynamicMethod = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMetadataBuilder()
        {
            _implementationPropertyByFactoryType = new ConcurrentDictionary<Type, PropertyInfo>();

            this.ContractAttributes = new List<PropertyContractAttribute>();
            this.Validation = new PropertyValidationMetadataBuilder();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMetadataBuilder(TypeMetadataBuilder declaringContract, PropertyInfo declaration, int propertyIndex) 
            : this()
        {
            this.Name = declaration.Name;
            this.ClrType = declaration.PropertyType;
            this.DeclaringContract = declaringContract;
            this.ContractPropertyInfo = declaration;
            this.PropertyIndex = propertyIndex;
            this.Access = PropertyAccess.ReadWrite;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IMetadataElement Members

        public override string ReferenceName
        {
            get { return this.Name; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IPropertyMetadata Members

        public bool HasContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute
        {
            return this.ContractAttributes.OfType<TAttribute>().Any();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAttribute TryGetContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute
        {
            return this.ContractAttributes.OfType<TAttribute>().FirstOrDefault();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetImplementation(Type factoryType, out PropertyInfo implementationProperty)
        {
            return _implementationPropertyByFactoryType.TryGetValue(factoryType, out implementationProperty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<KeyValuePair<Type, PropertyInfo>> GetAllImplementations()
        {
            return _implementationPropertyByFactoryType.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ParseStringValue(string s)
        {
            object parsedValue = null;

            if ( !ParseUtility.TryParse(s, this.ClrType, out parsedValue) )
            {
                var parseMethod = this.ClrType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
                parsedValue = parseMethod.Invoke(null, new object[] { s });
            }

            return parsedValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LambdaExpression MakePropertyExpression<TEntity>()
        {
            return GetExpressionFactory(this.ClrType).Property<TEntity>(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LambdaExpression MakePropertyAsObjectExpression<TEntity>()
        {
            return GetExpressionFactory(this.ClrType).PropertyAsObject<TEntity>(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Expression<Func<TEntity, bool>> MakeBinaryExpression<TEntity>(
            IPropertyMetadata[] navigationPath, 
            object value, 
            Func<Expression, Expression, Expression> binaryFactory)
        {
            return GetExpressionFactory(this.ClrType).Binary<TEntity>(navigationPath, value, binaryFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Expression<Func<TEntity, bool>> MakeBinaryExpression<TEntity>(
            IPropertyMetadata[] navigationPath, 
            string valueString,
            Func<Expression, Expression, Expression> binaryFactory)
        {
            return GetExpressionFactory(this.ClrType).Binary<TEntity>(navigationPath, ParseStringValue(valueString), binaryFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Expression<Func<TEntity, bool>> MakeForeignKeyBinaryExpression<TEntity>(
            IPropertyMetadata[] navigationPath, 
            object value,
            Func<Expression, Expression, Expression> binaryFactory)
        {
            return GetExpressionFactory(this.ClrType).ForeignKeyBinary<TEntity>(navigationPath, value, binaryFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Expression<Func<TEntity, bool>> MakeForeignKeyBinaryExpression<TEntity>(
            IPropertyMetadata[] navigationPath, 
            string valueString,
            Func<Expression, Expression, Expression> binaryFactory)
        {
            var relatedEntityKeyProperty = GetRelatedEntityKeyProperty(this);
            var parsedValue = relatedEntityKeyProperty.ParseStringValue(valueString);

            return GetExpressionFactory(this.ClrType).ForeignKeyBinary<TEntity>(navigationPath, parsedValue, binaryFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TEntity> MakeOrderBy<TEntity>(IPropertyMetadata[] navigationPath, IQueryable<TEntity> query, bool first, bool ascending)
        {
            return GetExpressionFactory(this.ClrType).OrderBy<TEntity>(query, navigationPath, first, ascending);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public object ReadValue(object target)
        {
            if ( ContractPropertyInfo == null )
            {
                throw new InvalidOperationException("Cannot read value of a property which is not a member of contract interface");
            }

            if ( target == null )
            {
                throw new ArgumentNullException("target");
            }

            if ( !this.ContractPropertyInfo.DeclaringType.IsInstanceOfType(target) )
            {
                return null;
            }

            if ( _valueReaderDynamicMethod == null )
            {
                lock ( _dynamicMethodSyncRoot )
                {
                    if ( _valueReaderDynamicMethod == null )
                    {
                        _valueReaderDynamicMethod = CompileValueReaderMethod();
                    }
                }
            }

            return _valueReaderDynamicMethod(target);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteValue(object target, object value)
        {
            if ( ContractPropertyInfo == null )
            {
                throw new InvalidOperationException("Cannot write value of a property which is not a member of contract interface");
            }

            if ( _valueWriterDynamicMethod == null )
            {
                lock ( _dynamicMethodSyncRoot )
                {
                    if ( _valueWriterDynamicMethod == null )
                    {
                        _valueWriterDynamicMethod = CompileValueWriterMethod();
                    }
                }
            }

            _valueWriterDynamicMethod(target, value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataBuilder DeclaringContract { get; set; }
        public string Name { get; set; }
        public int PropertyIndex { get; private set; }
        public PropertyKind Kind { get; set; }
        public PropertyRole Role { get; set; }
        public Type ClrType { get; set; }
        public ISemanticDataType SemanticType { get; set; }
        public PropertyAccess Access { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsCalculated { get; set; }
        public bool IsPartition { get; set; }
        public string PartitionValuePropertyName { get; set; }
        public int? NumericPrecision { get; set; }
        public List<PropertyContractAttribute> ContractAttributes { get; set; }
        public System.Reflection.PropertyInfo ContractPropertyInfo { get; set; }
        public object DefaultValue { get; set; }
        public Type DefaultValueGeneratorType { get; set; }
        public bool? DefaultDisplayVisible { get; set; }
        public string DefaultDisplayName { get; set; }
        public string DefaultDisplayFormat { get; set; }
        public bool DefaultSortAscending { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsCollection
        {
            get
            {
                return this.ClrType.IsCollectionType();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsReadOnly
        {
            get
            {
                return (
                    this.IsCalculated || 
                    ((this.Access & PropertyAccess.Write) == 0) ||
                    (this.Kind == PropertyKind.Scalar && this.ContractPropertyInfo != null && !this.ContractPropertyInfo.CanWrite));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeMetadata IPropertyMetadata.DeclaringContract
        {
            get { return this.DeclaringContract; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<PropertyContractAttribute> IPropertyMetadata.ContractAttributes
        {
            get { return this.ContractAttributes; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IRelationMetadata IPropertyMetadata.Relation
        {
            get { return this.Relation; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyValidationMetadata IPropertyMetadata.Validation
        {
            get { return this.Validation; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyRelationalMapping IPropertyMetadata.RelationalMapping
        {
            get { return this.RelationalMapping; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UpdateImplementation(Type factoryType, PropertyInfo implementationProperty)
        {
            _implementationPropertyByFactoryType[factoryType] = implementationProperty;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public PropertyRelationalMappingBuilder RelationalMapping { get; set; }
        public RelationMetadataBuilder Relation { get; set; }
        public PropertyValidationMetadataBuilder Validation { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ContractQualifiedName
        {
            get
            {
                return (DeclaringContract.Name + "." + this.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(ITypeMetadataVisitor visitor)
        {
            Name = visitor.VisitAttribute("Name", Name);
            PropertyIndex = visitor.VisitAttribute("PropertyIndex", PropertyIndex);
            DeclaringContract = visitor.VisitAttribute("DeclaringContract", DeclaringContract);
            Kind = visitor.VisitAttribute("Kind", Kind);
            Role = visitor.VisitAttribute("Role", Role);
            ClrType = visitor.VisitAttribute("ClrType", ClrType);
            SemanticType = visitor.VisitAttribute("SemanticType", SemanticType);
            Access = visitor.VisitAttribute("Access", Access);
            ContractAttributes = visitor.VisitAttribute("ContractAttributes", ContractAttributes);
            ContractPropertyInfo = visitor.VisitAttribute("ContractPropertyInfo", ContractPropertyInfo);
            DefaultValueGeneratorType = visitor.VisitAttribute("DefaultValueGeneratorType", DefaultValueGeneratorType);
            DefaultDisplayVisible = visitor.VisitAttribute("DefaultDisplayVisible", DefaultDisplayVisible);
            DefaultDisplayName = visitor.VisitAttribute("DefaultDisplayName", DefaultDisplayName);
            DefaultDisplayFormat = visitor.VisitAttribute("DefaultDisplayFormat", DefaultDisplayFormat);
            DefaultSortAscending = visitor.VisitAttribute("DefaultSortAscending", DefaultSortAscending);
            NumericPrecision = visitor.VisitAttribute("NumericPrecision", NumericPrecision);
            IsSensitive = visitor.VisitAttribute("IsSensitive", IsSensitive);
            IsCalculated = visitor.VisitAttribute("IsCalculated", IsCalculated);
            IsPartition = visitor.VisitAttribute("IsPartition", IsPartition);
            PartitionValuePropertyName = visitor.VisitAttribute("PartitionValuePropertyName", PartitionValuePropertyName);

            Relation = visitor.VisitElement<IRelationMetadata, RelationMetadataBuilder>(Relation);
            Validation = visitor.VisitElement<IPropertyValidationMetadata, PropertyValidationMetadataBuilder>(Validation);
            RelationalMapping = visitor.VisitElement<IPropertyRelationalMapping, PropertyRelationalMappingBuilder>(RelationalMapping);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetDefaultValueOperand(MethodWriterBase writer, out IOperand<TT.TProperty> valueOperand)
        {
            if ( DefaultValue != null )
            {
                if ( ClrType.IsInstanceOfType(DefaultValue) )
                {
                    if ( DefaultValue is System.Type )
                    {
                        valueOperand = 
                            Static.Func<string, bool, Type>(Type.GetType, writer.Const(((Type)DefaultValue).AssemblyQualifiedName), writer.Const(true))
                            .CastTo<TT.TProperty>();
                    }
                    else
                    {
                        var valueOperandType = typeof(Constant<>).MakeGenericType(DefaultValue.GetType());
                        valueOperand = ((IOperand)Activator.CreateInstance(valueOperandType, DefaultValue)).CastTo<TT.TProperty>();
                    }
                }
                else if ( DefaultValue is string )
                {
                    valueOperand = Static.Func(ParseUtility.Parse<TT.TProperty>, writer.Const((string)DefaultValue));
                }
                else
                {
                    throw new ContractConventionException(
                        this.GetType(),
                        ContractPropertyInfo.DeclaringType,
                        ContractPropertyInfo,
                        "Specified default value could not be parsed");
                }

                return true;
            }

            valueOperand = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return Name + " : " + ClrType.FriendlyName();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RelationMetadataBuilder SafeGetRelation()
        {
            if ( this.Relation == null )
            {
                this.Relation = new RelationMetadataBuilder();
            }

            return this.Relation;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyRelationalMappingBuilder SafeGetRelationalMapping()
        {
            if ( this.RelationalMapping == null )
            {
                this.RelationalMapping = new PropertyRelationalMappingBuilder();
            }

            return this.RelationalMapping;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<object, object> CompileValueReaderMethod()
        {
            var compiler = DeclaringContract.OwnerMetadataCache.DynamicMethodCompiler;

            using ( TT.CreateScope<TT.TContract, TT.TProperty>(DeclaringContract.ContractType, ClrType) )
            {
                var dynamicMethodDelegate = compiler.ForTemplatedDelegate<Func<object, object>>().CompileStaticFunction<object, object>(
                    string.Format("{0}_{1}_ReadValue", DeclaringContract.Name, this.Name),
                    (w, target) => {
                        w.Return(target.CastTo<TT.TContract>().Prop<TT.TProperty>(ContractPropertyInfo).CastTo<object>());
                    }
                );

                return (Func<object, object>)dynamicMethodDelegate;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Action<object, object> CompileValueWriterMethod()
        {
            var compiler = DeclaringContract.OwnerMetadataCache.DynamicMethodCompiler;

            using ( TT.CreateScope<TT.TContract, TT.TProperty>(DeclaringContract.ContractType, ClrType) )
            {
                var dynamicMethodDelegate = compiler.ForTemplatedDelegate<Action<object, object>>().CompileStaticVoidMethod<object, object>(
                    string.Format("{0}_{1}_WriteValue", DeclaringContract.Name, this.Name),
                    (w, target, value) => {
                        target.CastTo<TT.TContract>().Prop<TT.TProperty>(ContractPropertyInfo).Assign(value.CastTo<TT.TProperty>());
                    }
                );

                return (Action<object, object>)dynamicMethodDelegate;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly ConcurrentDictionary<Type, ExpressionFactory> _s_expressionFactoryByPropertyType = 
            new ConcurrentDictionary<Type, ExpressionFactory>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ExpressionFactory GetExpressionFactory(Type propertyType)
        {
            return _s_expressionFactoryByPropertyType.GetOrAdd(propertyType, ExpressionFactory.Create);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static IPropertyMetadata GetRelatedEntityKeyProperty(IPropertyMetadata foreignKeyProperty)
        {
            if ( foreignKeyProperty.Relation.RelatedPartyKey != null )
            {
                return foreignKeyProperty.Relation.RelatedPartyKey.Properties[0];
            }
            else if ( foreignKeyProperty.Relation.RelatedPartyType.PrimaryKey != null )
            {
                return foreignKeyProperty.Relation.RelatedPartyType.PrimaryKey.Properties[0];
            }
            else
            {
                return foreignKeyProperty.Relation.RelatedPartyType.Properties.First(p => p.Role == PropertyRole.Key);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class ExpressionFactory
        {
            public abstract LambdaExpression Property<TEntity>(IPropertyMetadata metaProperty);
            public abstract LambdaExpression PropertyAsObject<TEntity>(IPropertyMetadata metaProperty);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract Expression<Func<TEntity, bool>> Binary<TEntity>(
                IPropertyMetadata[] navigationPath, 
                object value, 
                Func<Expression, Expression, Expression> expressionFactory);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract Expression<Func<TEntity, bool>> ForeignKeyBinary<TEntity>(
                IPropertyMetadata[] navigationPath, 
                object value, 
                Func<Expression, Expression, Expression> expressionFactory);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract IQueryable<TEntity> OrderBy<TEntity>(
                IQueryable<TEntity> query, 
                IPropertyMetadata[] navigationPath, 
                bool first, 
                bool ascending);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Expression GetPropertyTargetExpression<TEntity>(IPropertyMetadata metaProperty, Expression target)
            {
                if ( metaProperty.ContractPropertyInfo.DeclaringType != null && metaProperty.ContractPropertyInfo.DeclaringType != typeof(TEntity) )
                {
                    return Expression.Convert(target, metaProperty.ContractPropertyInfo.DeclaringType);
                }

                return target;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract MethodInfo EntityIdGetValueMethod { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ExpressionFactory Create(Type propertyType)
            {
                var expressionFactoryType = typeof(ExpressionFactory<>).MakeGenericType(propertyType);
                var expressionFactory = (ExpressionFactory)Activator.CreateInstance(expressionFactoryType);
                return expressionFactory;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ExpressionFactory<TProperty> : ExpressionFactory
        {
            private readonly MethodInfo _entityIdGetValueMethod;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExpressionFactory()
            {
                Expression<Func<object, TProperty>> entityIdGetValueLambda = obj => EntityId.GetValue<TProperty>(obj);
                _entityIdGetValueMethod = entityIdGetValueLambda.GetMethodInfo();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override LambdaExpression Property<TEntity>(IPropertyMetadata metaProperty)
            {
                return metaProperty.ContractPropertyInfo.PropertyExpression<TEntity, TProperty>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override LambdaExpression PropertyAsObject<TEntity>(IPropertyMetadata metaProperty)
            {
                return metaProperty.ContractPropertyInfo.PropertyAsObjectExpression<TEntity, TProperty>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Expression<Func<TEntity, bool>> Binary<TEntity>(
                IPropertyMetadata[] navigationPath, 
                object value,
                Func<Expression, Expression, Expression> expressionFactory)
            {
                ParameterExpression parameter;
                Expression targetExpression;
                Expression propertyExpression;
                BuildNavigationTargetExpression<TEntity>(navigationPath, out parameter, out targetExpression, out propertyExpression);

                Expression binary = expressionFactory(
                    propertyExpression,
                    Expression.Constant(value, navigationPath.Last().ClrType));

                return Expression.Lambda<Func<TEntity, bool>>(binary, new[] { parameter });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void BuildNavigationTargetExpression<TEntity>(
                IPropertyMetadata[] navigationPath, 
                out ParameterExpression parameter,
                out Expression targetExpression,
                out Expression propertyExpression)
            {
                parameter = Expression.Parameter(typeof(TEntity), "e");
                targetExpression = parameter;
                propertyExpression = null;

                for ( int i = 0 ; i < navigationPath.Length ; i++ )
                {
                    targetExpression = GetPropertyTargetExpression<TEntity>(navigationPath[i], targetExpression);
                    propertyExpression = Expression.Property(targetExpression, navigationPath[i].ContractPropertyInfo);

                    if ( i < navigationPath.Length - 1 )
                    {
                        targetExpression = propertyExpression;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Expression<Func<TEntity, bool>> ForeignKeyBinary<TEntity>(
                IPropertyMetadata[] navigationPath, 
                object value, 
                Func<Expression, Expression, Expression> expressionFactory)
            {
                if ( navigationPath.Length > 1 )
                {
                    throw new NotSupportedException();
                }

                var metaProperty = navigationPath[0];
                var relatedKeyMetaProperty = GetRelatedEntityKeyProperty(metaProperty);
                var entityIdGetValueMethod = GetExpressionFactory(relatedKeyMetaProperty.ClrType).EntityIdGetValueMethod;
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var target = GetPropertyTargetExpression<TEntity>(metaProperty, parameter);
                var left = Expression.Call(
                    null,
                    entityIdGetValueMethod,
                    new Expression[] { Expression.Property(target, metaProperty.ContractPropertyInfo) });
                var right = Expression.Constant(value, relatedKeyMetaProperty.ClrType);
                var binary = expressionFactory(left, right);

                return Expression.Lambda<Func<TEntity, bool>>(binary, new[] { parameter });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IQueryable<TEntity> OrderBy<TEntity>(
                IQueryable<TEntity> query, 
                IPropertyMetadata[] navigationPath, 
                bool first, 
                bool ascending)
            {
                ParameterExpression parameter;
                Expression targetExpression;
                Expression propertyExpression;
                BuildNavigationTargetExpression<TEntity>(navigationPath, out parameter, out targetExpression, out propertyExpression);
                
                var lambda = Expression.Lambda<Func<TEntity, TProperty>>(
                    propertyExpression, 
                    new[] { parameter }
                );

                if ( first && ascending )
                {
                    return query.OrderBy<TEntity, TProperty>(lambda);
                }
                else if ( first )
                {
                    return query.OrderByDescending<TEntity, TProperty>(lambda);
                }
                else if ( ascending )
                {
                    return ((IOrderedQueryable<TEntity>)query).ThenBy<TEntity, TProperty>(lambda);
                }
                else 
                {
                    return ((IOrderedQueryable<TEntity>)query).ThenByDescending<TEntity, TProperty>(lambda);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override MethodInfo EntityIdGetValueMethod
            {
                get
                {
                    return _entityIdGetValueMethod;
                }
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NWheels.Conventions.Core;
using NWheels.Entities;
using NWheels.Extensions;

namespace NWheels.DataObjects.Core
{
    public class TypeMetadataBuilder : MetadataElement<ITypeMetadata>, ITypeMetadata
    {
        private readonly object _relationalMappingSyncRoot = new object();
        private readonly TypeMetadataCache _ownerMetadataCache;
        private readonly ConcreteToAbstractListAdapter<TypeMetadataBuilder, ITypeMetadata> _derivedTypesAdapter;
        private readonly ConcreteToAbstractListAdapter<PropertyMetadataBuilder, IPropertyMetadata> _propertiesAdapter;
        private readonly ConcreteToAbstractListAdapter<KeyMetadataBuilder, IKeyMetadata> _allKeysAdapter;
        private readonly ConcreteToAbstractListAdapter<PropertyMetadataBuilder, IPropertyMetadata> _defaultDisplayPropertiesAdapter;
        private readonly ConcreteToAbstractListAdapter<PropertyMetadataBuilder, IPropertyMetadata> _defaultSortPropertiesAdapter;
        private readonly ConcurrentDictionary<Type, Type> _implementationTypeByFactoryType;
        private Dictionary<string, PropertyMetadataBuilder> _propertyByName;
        private Dictionary<PropertyInfo, PropertyMetadataBuilder> _propertyByDeclaration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataBuilder()
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataBuilder(TypeMetadataCache ownerMetadataCache)
        {
            _ownerMetadataCache = ownerMetadataCache;

            this.DerivedTypes = new List<TypeMetadataBuilder>();
            this.MixinContractTypes = new List<Type>();
            this.Properties = new List<PropertyMetadataBuilder>();
            this.Methods = new List<MethodInfo>();
            this.AllKeys = new List<KeyMetadataBuilder>();
            this.DefaultDisplayProperties = new List<PropertyMetadataBuilder>();
            this.DefaultSortProperties = new List<PropertyMetadataBuilder>();

            _derivedTypesAdapter = new ConcreteToAbstractListAdapter<TypeMetadataBuilder, ITypeMetadata>(this.DerivedTypes);
            _propertiesAdapter = new ConcreteToAbstractListAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.Properties);
            _allKeysAdapter = new ConcreteToAbstractListAdapter<KeyMetadataBuilder, IKeyMetadata>(this.AllKeys);
            _defaultDisplayPropertiesAdapter = new ConcreteToAbstractListAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.DefaultDisplayProperties);
            _defaultSortPropertiesAdapter = new ConcreteToAbstractListAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.DefaultSortProperties);
            _implementationTypeByFactoryType = new ConcurrentDictionary<Type, Type>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IMetadataElement Members

        public override string ReferenceName
        {
            get { return this.Name; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region ITypeMetadata Members

        public bool TryGetImplementation(Type factoryType, out Type implementationType)
        {
            return _implementationTypeByFactoryType.TryGetValue(factoryType, out implementationType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<KeyValuePair<Type, Type>> GetAllImplementations()
        {
            return _implementationTypeByFactoryType.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LambdaExpression MakePropertyExpression(IPropertyMetadata property)
        {
            return GetExpressionFactory(property.ContractPropertyInfo.DeclaringType).MakePropertyExpression(property);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TBase> MakeOfType<TBase>(IQueryable<TBase> query)
        {
            return GetExpressionFactory(this.ContractType).OfType<TBase>(query);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; set; }
        public string NamespaceQualifier { get; set; }
        public Type ContractType { get; set; }
        public Type DomainObjectType { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsEntity { get; set; }
        public bool IsEntityPart { get; set; }
        public bool IsViewModel { get; set; }
        public string DisplayName { get; set; }
        public string DefaultDisplayFormat { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string QualifiedName
        {
            get
            {
                return (!string.IsNullOrEmpty(NamespaceQualifier) ? NamespaceQualifier + "." : string.Empty) + Name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeMetadata ITypeMetadata.BaseType
        {
            get { return this.BaseType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<ITypeMetadata> ITypeMetadata.DerivedTypes
        {
            get { return _derivedTypesAdapter; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<Type> ITypeMetadata.MixinContractTypes
        {
            get { return this.MixinContractTypes; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<IPropertyMetadata> ITypeMetadata.Properties
        {
            get { return _propertiesAdapter; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyCollection<MethodInfo> ITypeMetadata.Methods
        {
            get { return this.Methods; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IKeyMetadata ITypeMetadata.PrimaryKey
        {
            get { return this.PrimaryKey; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<IKeyMetadata> ITypeMetadata.AllKeys
        {
            get { return _allKeysAdapter; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyMetadata ITypeMetadata.EntityIdProperty
        {
            get { return this.EntityIdProperty; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyMetadata ITypeMetadata.PartitionProperty
        {
            get { return this.PartitionProperty; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<IPropertyMetadata> ITypeMetadata.DefaultDisplayProperties
        {
            get { return _defaultDisplayPropertiesAdapter; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<IPropertyMetadata> ITypeMetadata.DefaultSortProperties
        {
            get { return _defaultSortPropertiesAdapter; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeRelationalMapping ITypeMetadata.RelationalMapping
        {
            get { return this.RelationalMapping; }
        }
        
        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataBuilder BaseType { get; set; }
        public List<TypeMetadataBuilder> DerivedTypes { get; private set; }
        public int InheritanceDepth { get; set; }
        public List<Type> MixinContractTypes { get; private set; }
        public List<PropertyMetadataBuilder> Properties { get; private set; }
        public List<MethodInfo> Methods { get; private set; }
        public KeyMetadataBuilder PrimaryKey { get; set; }
        public List<KeyMetadataBuilder> AllKeys { get; private set; }
        public PropertyMetadataBuilder PartitionProperty { get; set; }
        public List<PropertyMetadataBuilder> DefaultDisplayProperties { get; private set; }
        public List<PropertyMetadataBuilder> DefaultSortProperties { get; private set; }
        public TypeRelationalMappingBuilder RelationalMapping { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMetadataBuilder EntityIdProperty
        {
            get
            {
                return (PrimaryKey != null && PrimaryKey.Properties.Count == 1 ? PrimaryKey.Properties[0] : null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(ITypeMetadataVisitor visitor)
        {
            Name = visitor.VisitAttribute("Name", Name);
            NamespaceQualifier = visitor.VisitAttribute("NamespaceQualifier", NamespaceQualifier);
            ContractType = visitor.VisitAttribute("ContractType", ContractType);
            DomainObjectType = visitor.VisitAttribute("DomainObjectType", DomainObjectType);
            IsAbstract = visitor.VisitAttribute("IsAbstract", IsAbstract);

            BaseType = visitor.VisitElementReference<ITypeMetadata, TypeMetadataBuilder>("BaseType", BaseType);
            visitor.VisitElementList<ITypeMetadata, TypeMetadataBuilder>("DerivedTypes", DerivedTypes);
            InheritanceDepth = visitor.VisitAttribute("InheritanceDepth", InheritanceDepth);
            
            visitor.VisitElementList<IPropertyMetadata, PropertyMetadataBuilder>("Properties", Properties);
            visitor.VisitElementList<IKeyMetadata, KeyMetadataBuilder>("AllKeys", AllKeys);
            PrimaryKey = visitor.VisitElementReference<IKeyMetadata, KeyMetadataBuilder>("PrimaryKey", PrimaryKey);
            PartitionProperty = visitor.VisitElementReference<IPropertyMetadata, PropertyMetadataBuilder>("PartitionProperty", PartitionProperty);

            DisplayName = visitor.VisitAttribute("DisplayName", DisplayName);
            DefaultDisplayFormat = visitor.VisitAttribute("DefaultDisplayFormat", DefaultDisplayFormat);

            visitor.VisitElementList<IPropertyMetadata, PropertyMetadataBuilder>("DefaultDisplayProperties", DefaultDisplayProperties);
            visitor.VisitElementList<IPropertyMetadata, PropertyMetadataBuilder>("DefaultSortProperties", DefaultSortProperties);

            RelationalMapping = visitor.VisitElement<ITypeRelationalMapping, TypeRelationalMappingBuilder>(RelationalMapping);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureRelationalMapping(MetadataConventionSet conventions)
        {
            EnsureRelationalMapping(conventions, new HashSet<TypeMetadataBuilder>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPropertyMetadata GetPropertyByName(string name)
        {
            if ( _propertyByName != null )
            {
                return _propertyByName[name];
            }
            else
            {
                return this.Properties.First(p => p.Name == name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPropertyMetadata GetPropertyByDeclaration(PropertyInfo declarationInContract)
        {
            if ( _propertyByDeclaration != null )
            {
                return _propertyByDeclaration[declarationInContract];
            }
            else
            {
                return this.Properties.First(p => p.ContractPropertyInfo == declarationInContract);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetPropertyByName(string name, out IPropertyMetadata property)
        {
            PropertyMetadataBuilder metaProperty;
            var result = TryGetPropertyByName(name, out metaProperty);
            property = metaProperty;
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetPropertyByDeclaration(PropertyInfo declarationInContract, out IPropertyMetadata property)
        {
            PropertyMetadataBuilder metaProperty;
            var result = TryGetPropertyByDeclaration(declarationInContract, out metaProperty);
            property = metaProperty;
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetPropertyByName(string name, out PropertyMetadataBuilder property)
        {
            if ( _propertyByName != null )
            {
                return _propertyByName.TryGetValue(name, out property);
            }
            else
            {
                property = this.Properties.FirstOrDefault(p => p.Name == name);
                return (property != null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetPropertyByDeclaration(PropertyInfo declarationInContract, out PropertyMetadataBuilder property)
        {
            if ( _propertyByDeclaration != null )
            {
                return _propertyByDeclaration.TryGetValue(declarationInContract, out property);
            }
            else
            {
                property = this.Properties.FirstOrDefault(p => p.ContractPropertyInfo == declarationInContract);
                return (property != null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UpdateImplementation(IEntityObjectFactory factory, Type implementationType)
        {
            UpdateImplementation(factory.GetType(), implementationType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UpdateImplementation(Type factoryType, Type implementationType)
        {
            _implementationTypeByFactoryType[factoryType] = implementationType;

            var implementationProperties = implementationType.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name);

            foreach ( var property in this.Properties.Where(p => implementationProperties.ContainsKey(p.Name)) )
            {
                property.UpdateImplementation(factoryType, implementationProperties[property.Name]);
            }

            _ownerMetadataCache.UpdateMetaTypeImplementation(this, implementationType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return Name + (BaseType != null ? " : " + BaseType.Name : "");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        public TypeRelationalMappingBuilder SafeGetRelationalMapping()
        {
            if ( this.RelationalMapping == null )
            {
                this.RelationalMapping = new TypeRelationalMappingBuilder();
            }

            return this.RelationalMapping;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void RegisterDerivedType(TypeMetadataBuilder derivedType)
        {
            var existingRegistrationIndex = this.DerivedTypes.FindIndex(t => t.ContractType == derivedType.ContractType);

            if ( existingRegistrationIndex >= 0 )
            {
                this.DerivedTypes.RemoveAt(existingRegistrationIndex);
            }

            this.DerivedTypes.Add(derivedType);

            if ( this.BaseType != null )
            {
                this.BaseType.RegisterDerivedType(derivedType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void EndBuild()
        {
            this.PartitionProperty = this.Properties.FirstOrDefault(p => p.IsPartition);
            _propertyByName = this.Properties.ToDictionary(p => p.Name);
            _propertyByDeclaration = this.Properties.ToDictionary(p => p.ContractPropertyInfo);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal HashSet<Type> GetBaseContracts()
        {
            var allContracts = this.ContractType.GetInterfaces().Where(t =>
                this.IsEntity ?
                DataObjectContractAttribute.IsDataObjectContract(t) :
                DataObjectPartContractAttribute.IsDataObjectPartContract(t))
                .ToArray();

            var baseContractSet = new HashSet<Type>(allContracts);

            foreach ( var contract in allContracts )
            {
                baseContractSet.ExceptWith(contract.GetInterfaces());
            }

            return baseContractSet;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void EnsureBasePropertiesInherited()
        {
            if ( this.BaseType != null )
            {
                this.BaseType.EnsureBasePropertiesInherited();

                foreach ( var baseProperty in this.BaseType.Properties )
                {
                    if ( !_propertyByName.ContainsKey(baseProperty.Name) )
                    {
                        this.Properties.Add(baseProperty);
                        _propertyByName.Add(baseProperty.Name, baseProperty);

                        if ( baseProperty.ContractPropertyInfo != null )
                        {
                            _propertyByDeclaration.Add(baseProperty.ContractPropertyInfo, baseProperty);
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal bool MetadataConventionsPreviewed { get; set; }
        internal bool MetadataConventionsApplied { get; set; }
        internal bool MetadataConventionsFinalized { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnsureRelationalMapping(MetadataConventionSet conventions, HashSet<TypeMetadataBuilder> visitedTypes)
        {
            if ( !visitedTypes.Add(this) )
            {
                return;
            }

            conventions.ApplyRelationalMappingConventions(this);

            foreach ( var property in this.Properties.Where(p => p.Kind == PropertyKind.Relation) )
            {
                property.Relation.RelatedPartyType.EnsureRelationalMapping(conventions, visitedTypes);
            }

            //if ( this.RelationalMapping == null )
            //{
            //    lock ( _relationalMappingSyncRoot )
            //    {
            //        if ( this.RelationalMapping == null )
            //        {
            //            conventions.ApplyRelationalMappingConventions(this);
            //        }

            //        foreach ( var property in this.Properties.Where(p => p.Kind == PropertyKind.Relation) )
            //        {
            //            property.Relation.RelatedPartyType.EnsureRelationalMapping(conventions);
            //        }
            //    }
            //}
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly ConcurrentDictionary<Type, ExpressionFactory> _s_expressionFactoryByPropertyType =
            new ConcurrentDictionary<Type, ExpressionFactory>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ExpressionFactory GetExpressionFactory(Type contractType)
        {
            return _s_expressionFactoryByPropertyType.GetOrAdd(contractType, ExpressionFactory.Create);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class ExpressionFactory
        {
            public abstract LambdaExpression MakePropertyExpression(IPropertyMetadata property);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract IQueryable<TBase> OfType<TBase>(IQueryable source);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ExpressionFactory Create(Type contractType)
            {
                var expressionFactoryType = typeof(ExpressionFactory<>).MakeGenericType(contractType);
                var expressionFactory = (ExpressionFactory)Activator.CreateInstance(expressionFactoryType);
                return expressionFactory;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ExpressionFactory<TEntity> : ExpressionFactory
        {
            #region Overrides of ExpressionFactory

            public override LambdaExpression MakePropertyExpression(IPropertyMetadata property)
            {
                return property.MakePropertyExpression<TEntity>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IQueryable<TBase> OfType<TBase>(IQueryable source)
            {
                return (IQueryable<TBase>)source.OfType<TEntity>();
            }

            #endregion
        }
    }
}

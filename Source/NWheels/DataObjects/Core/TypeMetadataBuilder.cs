using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NWheels.DataObjects.Core
{
    public class TypeMetadataBuilder : MetadataElement<ITypeMetadata>, ITypeMetadata
    {
        private readonly object _relationalMappingSyncRoot = new object();
        private readonly ConcreteToAbstractCollectionAdapter<TypeMetadataBuilder, ITypeMetadata> _derivedTypesAdapter;
        private readonly ConcreteToAbstractCollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata> _propertiesAdapter;
        private readonly ConcreteToAbstractCollectionAdapter<KeyMetadataBuilder, IKeyMetadata> _allKeysAdapter;
        private readonly ConcreteToAbstractCollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata> _defaultDisplayPropertiesAdapter;
        private readonly ConcreteToAbstractCollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata> _defaultSortPropertiesAdapter;
        private Dictionary<string, PropertyMetadataBuilder> _propertyByName;
        private Dictionary<PropertyInfo, PropertyMetadataBuilder> _propertyByDeclaration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataBuilder()
        {
            this.DerivedTypes = new List<TypeMetadataBuilder>();
            this.MixinContractTypes = new List<Type>();
            this.Properties = new List<PropertyMetadataBuilder>();
            this.AllKeys = new List<KeyMetadataBuilder>();
            this.DefaultDisplayProperties = new List<PropertyMetadataBuilder>();
            this.DefaultSortProperties = new List<PropertyMetadataBuilder>();

            _derivedTypesAdapter = new ConcreteToAbstractCollectionAdapter<TypeMetadataBuilder, ITypeMetadata>(this.DerivedTypes);
            _propertiesAdapter = new ConcreteToAbstractCollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.Properties);
            _allKeysAdapter = new ConcreteToAbstractCollectionAdapter<KeyMetadataBuilder, IKeyMetadata>(this.AllKeys);
            _defaultDisplayPropertiesAdapter = new ConcreteToAbstractCollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.DefaultDisplayProperties);
            _defaultSortPropertiesAdapter = new ConcreteToAbstractCollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.DefaultSortProperties);
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

        public string Name { get; set; }
        public Type ContractType { get; set; }
        public Type ImplementationType { get; set; }
        public bool IsAbstract { get; set; }
        public string DefaultDisplayFormat { get; set; }

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
        public List<Type> MixinContractTypes { get; private set; }
        public List<PropertyMetadataBuilder> Properties { get; private set; }
        public KeyMetadataBuilder PrimaryKey { get; set; }
        public List<KeyMetadataBuilder> AllKeys { get; private set; }
        public List<PropertyMetadataBuilder> DefaultDisplayProperties { get; private set; }
        public List<PropertyMetadataBuilder> DefaultSortProperties { get; private set; }
        public TypeRelationalMappingBuilder RelationalMapping { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(IMetadataElementVisitor visitor)
        {
            Name = visitor.VisitAttribute("Name", Name);
            ContractType = visitor.VisitAttribute("ContractType", ContractType);
            ImplementationType = visitor.VisitAttribute("ImplementationType", ImplementationType);
            IsAbstract = visitor.VisitAttribute("IsAbstract", IsAbstract);

            BaseType = visitor.VisitElement<ITypeMetadata, TypeMetadataBuilder>(BaseType);

            visitor.VisitElementList<ITypeMetadata, TypeMetadataBuilder>("DerivedTypes", DerivedTypes);
            visitor.VisitElementList<IPropertyMetadata, PropertyMetadataBuilder>("Properties", Properties);
            visitor.VisitElementList<IKeyMetadata, KeyMetadataBuilder>("AllKeys", AllKeys);
            PrimaryKey = visitor.VisitElementReference<IKeyMetadata, KeyMetadataBuilder>("PrimaryKey", PrimaryKey);

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

        public void UpdateImplementation(Type implementationType)
        {
            this.ImplementationType = implementationType;

            var implementationProperties = implementationType.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name);

            foreach ( var property in this.Properties )
            {
                property.ImplementationPropertyInfo = implementationProperties[property.Name];
            }

            //var implementationProperties = implementationType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            //var implementationPropertyInfoByGetterMethod = implementationProperties.ToDictionary(p => p.GetMethod);

            //var map = implementationType.GetInterfaceMap(this.ContractType);

            //for ( int i = 0 ; i < map.InterfaceMethods.Length ; i++ )
            //{
            //    if ( map.InterfaceMethods[i].IsSpecialName && map.InterfaceMethods[i].Name.StartsWith("get_") )
            //    {
            //        var implementationPropertyInfo = implementationPropertyInfoByGetterMethod[map.TargetMethods[i]];
            //        var metadataProperty = _propertyByName[implementationPropertyInfo.Name.Split('.').Last()];
            //        metadataProperty.ImplementationPropertyInfo = implementationPropertyInfo;
            //    }
            //}
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

        internal void EndBuild()
        {
            _propertyByName = this.Properties.ToDictionary(p => p.Name);
            _propertyByDeclaration = this.Properties.ToDictionary(p => p.ContractPropertyInfo);
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
    }
}

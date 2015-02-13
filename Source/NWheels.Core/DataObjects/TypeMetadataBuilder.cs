using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public class TypeMetadataBuilder : MetadataElement<ITypeMetadata>, ITypeMetadata
    {
        private readonly CollectionAdapter<TypeMetadataBuilder, ITypeMetadata> _derivedTypesAdapter;
        private readonly CollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata> _propertiesAdapter;
        private readonly CollectionAdapter<KeyMetadataBuilder, IKeyMetadata> _allKeysAdapter;
        private readonly CollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata> _defaultDisplayPropertiesAdapter;
        private readonly CollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata> _defaultSortPropertiesAdapter;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataBuilder()
        {
            this.DerivedTypes = new List<TypeMetadataBuilder>();
            this.Properties = new List<PropertyMetadataBuilder>();
            this.AllKeys = new List<KeyMetadataBuilder>();
            this.DefaultDisplayProperties = new List<PropertyMetadataBuilder>();
            this.DefaultSortProperties = new List<PropertyMetadataBuilder>();

            _derivedTypesAdapter = new CollectionAdapter<TypeMetadataBuilder, ITypeMetadata>(this.DerivedTypes);
            _propertiesAdapter = new CollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.Properties);
            _allKeysAdapter = new CollectionAdapter<KeyMetadataBuilder, IKeyMetadata>(this.AllKeys);
            _defaultDisplayPropertiesAdapter = new CollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.DefaultDisplayProperties);
            _defaultSortPropertiesAdapter = new CollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.DefaultSortProperties);
        }

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

            visitor.VisitElementList<ITypeMetadata, TypeMetadataBuilder>(DerivedTypes);
            visitor.VisitElementList<IPropertyMetadata, PropertyMetadataBuilder>(Properties);
            visitor.VisitElementList<IKeyMetadata, KeyMetadataBuilder>(AllKeys);
            PrimaryKey = AllKeys.SingleOrDefault(key => key.Kind == KeyKind.Primary);

            DefaultDisplayFormat = visitor.VisitAttribute("DefaultDisplayFormat", DefaultDisplayFormat);

            visitor.VisitElementList<IPropertyMetadata, PropertyMetadataBuilder>("DefaultDisplayProperties", DefaultDisplayProperties);
            visitor.VisitElementList<IPropertyMetadata, PropertyMetadataBuilder>("DefaultSortProperties", DefaultSortProperties);

            RelationalMapping = visitor.VisitElement<ITypeRelationalMapping, TypeRelationalMappingBuilder>(RelationalMapping);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace NWheels.DataObjects.Core.Conventions
{
    public class AttributeMetadataConvention : IMetadataConvention
    {
        private readonly Dictionary<Type, Action<Attribute, TypeMetadataBuilder>> _typeAttributes;
        private readonly Dictionary<Type, Action<Attribute, PropertyMetadataBuilder>> _propertyAttributes;
        private TypeMetadataCache _cache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AttributeMetadataConvention()
        {
            _typeAttributes = new Dictionary<Type, Action<Attribute, TypeMetadataBuilder>>();
            _propertyAttributes = new Dictionary<Type, Action<Attribute, PropertyMetadataBuilder>>();
            
            InitializeDefaultAttributes();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InjectCache(TypeMetadataCache cache)
        {
            _cache = cache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Preview(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(TypeMetadataBuilder type)
        {
            ApplyToType(type);
            ApplyToProperties(type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Finalize(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ApplyToType(TypeMetadataBuilder type)
        {
            var typeAttributes = type.ContractType.GetCustomAttributes(inherit: true).Cast<Attribute>().ToArray();

            foreach ( var attribute in typeAttributes )
            {
                Action<Attribute, TypeMetadataBuilder> applier;

                if ( TryGetAttributeApplier(_typeAttributes, attribute.GetType(), out applier) )
                {
                    applier(attribute, type);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ApplyToProperties(TypeMetadataBuilder type)
        {
            foreach ( var property in type.Properties )
            {
                ApplyToProperty(property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ApplyToProperty(PropertyMetadataBuilder property)
        {
            var propertyAttributes = property.ContractPropertyInfo.GetCustomAttributes(inherit: true).Cast<Attribute>().ToArray();

            foreach ( var attribute in propertyAttributes )
            {
                Action<Attribute, PropertyMetadataBuilder> applier;

                if ( TryGetAttributeApplier(_propertyAttributes, attribute.GetType(), out applier) )
                {
                    applier(attribute, property);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryGetAttributeApplier<TDelegate>(Dictionary<Type, TDelegate> appliersByAttributeType, Type attributeType, out TDelegate applier)
            where TDelegate : class
        {
            for ( var type = attributeType ; type != null && type != typeof(Attribute) ; type = type.BaseType )
            {
                if ( appliersByAttributeType.TryGetValue(type, out applier) )
                {
                    return true;
                }
            }

            applier = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeDefaultAttributes()
        {
            AddPropertyAttribute<PropertyContract.KeyAttribute>((attr, prop) => {
                prop.Role = PropertyRole.Key;
                prop.Kind = PropertyKind.Scalar;
                prop.Validation.IsRequired = true;
            });
            AddPropertyAttribute<PropertyContract.VersionAttribute>((attr, prop) => {
                prop.Role = PropertyRole.Version;
                prop.Kind = PropertyKind.Scalar;
            });
            AddPropertyAttribute<PropertyContract.RequiredAttribute>((attr, prop) => {
                prop.Validation.IsRequired = true;
                prop.Validation.IsEmptyAllowed = attr.AllowEmpty;
            });
            AddPropertyAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>((attr, prop) => {
                prop.Validation.IsRequired = true;
            });
            AddPropertyAttribute<PropertyContract.ReadOnlyAttribute>((attr, prop) => 
                prop.Access = PropertyAccess.ReadOnly);
            AddPropertyAttribute<PropertyContract.WriteOnlyAttribute>((attr, prop) => 
                prop.Access = PropertyAccess.WriteOnly);
            AddPropertyAttribute<PropertyContract.SearchOnlyAttribute>((attr, prop) => 
                prop.Access = PropertyAccess.SearchOnly);
            AddPropertyAttribute<PropertyContract.AccessAttribute>((attr, prop) => 
                prop.Access = attr.AllowedAccessFlags);
            AddPropertyAttribute<PropertyContract.UniqueAttribute>((attr, prop) => 
                prop.Validation.IsUnique = true);
            AddPropertyAttribute<PropertyContract.DefaultValueAttribute>((attr, prop) => 
                prop.DefaultValue = attr.Value);
            AddPropertyAttribute<System.ComponentModel.DefaultValueAttribute>((attr, prop) =>
                prop.DefaultValue = attr.Value);
            AddPropertyAttribute<PropertyContract.Semantic.DataTypeAttribute>((attr, prop) => 
                prop.SemanticType = _cache.GetSemanticTypeInstance(attr.Type, prop.ClrType));
            AddPropertyAttribute<PropertyContract.Validation.FutureAttribute>((attr, prop) =>
                prop.SemanticType = _cache.GetSemanticTypeInstance(typeof(SemanticType.DefaultOf<DateTime>), prop.ClrType));
            AddPropertyAttribute<PropertyContract.Storage.StorageTypeAttribute>((attr, prop) =>
                prop.SafeGetRelationalMapping().StorageType = _cache.GetStorageTypeInstance(attr.Type, prop.ClrType));
            AddPropertyAttribute<PropertyContract.Storage.RelationalMappingAttribute>((attr, prop) => attr.ApplyTo(prop));
            AddPropertyAttribute<PropertyContract.Validation.LengthAttribute>((attr, prop) => {
                prop.Validation.MinLength = attr.Min;
                prop.Validation.MaxLength = attr.Max;
            });
            AddPropertyAttribute<PropertyContract.Validation.MaxLengthAttribute>((attr, prop) =>
                prop.Validation.MaxLength = attr.MaxLength);
            AddPropertyAttribute<PropertyContract.Validation.MaxValueAttribute>((attr, prop) =>
                prop.Validation.MaxValue = attr.Value);
            AddPropertyAttribute<PropertyContract.Validation.MinLengthAttribute>((attr, prop) =>
                prop.Validation.MinLength = attr.MinLength);
            AddPropertyAttribute<PropertyContract.Validation.MinValueAttribute>((attr, prop) =>
                prop.Validation.MinValue = attr.Value);
            AddPropertyAttribute<PropertyContract.Validation.PastAttribute>((attr, prop) =>
                prop.SemanticType = _cache.GetSemanticTypeInstance(typeof(SemanticType.DefaultOf<DateTime>), prop.ClrType));
            AddPropertyAttribute<PropertyContract.Validation.RangeAttribute>((attr, prop) => {
                prop.Validation.MinValue = attr.Min;
                prop.Validation.MaxValue = attr.Max;
                prop.Validation.MinValueExclusive = attr.MinExclusive;
                prop.Validation.MaxValueExclusive = attr.MaxExclusive;
            });
            AddPropertyAttribute<PropertyContract.Validation.RegularExpressionAttribute>((attr, prop) =>
                prop.Validation.RegularExpression = attr.Expression);
            AddPropertyAttribute<PropertyContract.Validation.ValidatorAttribute>((attr, prop) => {
                throw new NotSupportedException("Validator attribute is not yet supported.");
            });
            AddPropertyAttribute<PropertyContract.Presentation.DisplayFormatAttribute>((attr, prop) =>
                prop.DefaultDisplayFormat = attr.Format);
            AddPropertyAttribute<PropertyContract.Presentation.DisplayNameAttribute>((attr, prop) =>
                prop.DefaultDisplayName = attr.Text);
            AddPropertyAttribute<PropertyContract.Presentation.SortAttribute>((attr, prop) =>
                prop.DefaultSortAscending = attr.Ascending);
            AddPropertyAttribute<PropertyContract.Relation.ManyToManyAttribute>((attr, prop) =>
                prop.SafeGetRelation().RelationKind = RelationKind.ManyToMany);
            AddPropertyAttribute<PropertyContract.Relation.ManyToOneAttribute>((attr, prop) =>
                prop.SafeGetRelation().RelationKind = RelationKind.ManyToOne);
            AddPropertyAttribute<PropertyContract.Relation.OneToManyAttribute>((attr, prop) =>
                prop.SafeGetRelation().RelationKind = RelationKind.OneToMany);
            AddPropertyAttribute<PropertyContract.Relation.OneToOneAttribute>((attr, prop) =>
                prop.SafeGetRelation().RelationKind = RelationKind.OneToOne);
            AddPropertyAttribute<PropertyContract.Security.SensitiveAttribute>((attr, prop) =>
                prop.IsSensitive = true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddTypeAttribute<TAttribute>(Action<TAttribute, TypeMetadataBuilder> applier) where TAttribute : Attribute
        {
            _typeAttributes.Add(typeof(TAttribute), (attr, type) => applier((TAttribute)attr, type));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddPropertyAttribute<TAttribute>(Action<TAttribute, PropertyMetadataBuilder> applier) where TAttribute : Attribute
        {
            _propertyAttributes.Add(typeof(TAttribute), (attr, prop) => applier((TAttribute)attr, prop));
        }
    }
}

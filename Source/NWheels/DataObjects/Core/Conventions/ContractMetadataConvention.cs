using System;
using Hapil;
using NWheels.Extensions;

namespace NWheels.DataObjects.Core.Conventions
{
    public class ContractMetadataConvention : IMetadataConvention
    {
        private TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InjectCache(TypeMetadataCache cache)
        {
            _metadataCache = cache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Preview(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(TypeMetadataBuilder type)
        {
            foreach ( var property in type.Properties )
            {
                ApplyToProperty(property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Finalize(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ApplyToProperty(PropertyMetadataBuilder property)
        {
            Type collectionElementType;

            if ( DataObjectPartContractAttribute.IsDataObjectPartContract(property.ContractPropertyInfo.PropertyType) )
            {
                ThisIsContractPartProperty(property);
            }
            else if ( DataObjectContractAttribute.IsDataObjectContract(property.ContractPropertyInfo.PropertyType) )
            {
                ThisIsManyToOneProperty(property);
            }
            else if ( property.ClrType.IsCollectionType(out collectionElementType) && DataObjectContractAttribute.IsDataObjectContract(collectionElementType) )
            {
                ThisIsOneToManyProperty(property, collectionElementType);
            }
            else if ( property.Name.EqualsIgnoreCase("Id") )
            {
                ThisIsKeyProperty(property);
            }
            else
            {
                ThisIsScalarProperty(property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ThisIsScalarProperty(PropertyMetadataBuilder property)
        {
            property.Kind = PropertyKind.Scalar;

            if ( property.ClrType.IsValueType && !property.ClrType.IsNullableValueType() )
            {
                property.Validation.IsRequired = true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ThisIsKeyProperty(PropertyMetadataBuilder property)
        {
            property.IsKey = true;
            property.Kind = PropertyKind.Scalar;
            property.Validation.IsRequired = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ThisIsRelationProperty(PropertyMetadataBuilder property)
        {
            property.Kind = PropertyKind.Relation;

            if ( property.Relation == null )
            {
                property.Relation = new RelationMetadataBuilder();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ThisIsOneToManyProperty(PropertyMetadataBuilder property, Type collectionElementType)
        {
            ThisIsRelationProperty(property);
            
            property.Relation.RelationKind = RelationKind.OneToMany;
            property.Relation.RelatedPartyType = _metadataCache.FindTypeMetadataAllowIncomplete(collectionElementType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ThisIsManyToOneProperty(PropertyMetadataBuilder property)
        {
            ThisIsRelationProperty(property);

            property.Relation.RelationKind = RelationKind.ManyToOne;
            property.Relation.RelatedPartyType = _metadataCache.FindTypeMetadataAllowIncomplete(property.ClrType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ThisIsContractPartProperty(PropertyMetadataBuilder property)
        {
            property.Kind = PropertyKind.Part;
        }
    }
}

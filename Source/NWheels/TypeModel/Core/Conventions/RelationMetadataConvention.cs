using System.Linq;
using NWheels.TypeModel;

namespace NWheels.DataObjects.Core.Conventions
{
    public class RelationMetadataConvention : IMetadataConvention
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
            CopyKeysFromBase(type);
            FindOrAddPrimaryKey(type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Finalize(TypeMetadataBuilder type)
        {
            CompleteRelations(type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private KeyMetadataBuilder FindOrAddPrimaryKey(TypeMetadataBuilder type)
        {
            if ( type.PrimaryKey == null )
            {
                foreach ( var property in type.Properties.Where(p => p.Role == PropertyRole.Key) )
                {
                    if ( type.PrimaryKey == null )
                    {
                        type.PrimaryKey = new KeyMetadataBuilder {
                            Kind = KeyKind.Primary,
                            Name = "PK_" + type.Name
                        };

                        type.AllKeys.Add(type.PrimaryKey);
                    }

                    type.PrimaryKey.Properties.Add(property);
                }
            }

            return type.PrimaryKey;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CopyKeysFromBase(TypeMetadataBuilder thisType)
        {
            for ( var baseType = thisType.BaseType ; baseType != null ; baseType = baseType.BaseType )
            {
                var basePrimaryKey = FindOrAddPrimaryKey(baseType);

                foreach ( var key in baseType.AllKeys )
                {
                    if ( !thisType.AllKeys.Any(k => k.Name == key.Name) )
                    {
                        thisType.AllKeys.Add(key);
                    }
                }

                if ( thisType.PrimaryKey == null )
                {
                    thisType.PrimaryKey = basePrimaryKey;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CompleteRelations(TypeMetadataBuilder type)
        {
            foreach ( var property in type.Properties.Where(p => p.Relation != null) )
            {
                CompleteRelationMetadata(type, property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CompleteRelationMetadata(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            if ( property.Relation.RelatedPartyType == null )
            {
                return;
            }

            switch ( property.Relation.Multiplicity )
            {
                case RelationMultiplicity.OneToOne:
                case RelationMultiplicity.ManyToOne:
                    AddToOneRelation(type, property);
                    break;
                case RelationMultiplicity.OneToMany:
                case RelationMultiplicity.ManyToMany:
                    AddToManyRelation(type, property);
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddToOneRelation(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            _metadataCache.Conventions.ApplyTypeConventions(property.Relation.RelatedPartyType, doPreview: true, doApply: true, doFinalize: false);

            var thisKey = FindOrAddForeignKey(type, property);

            property.Relation.ThisPartyKey = thisKey;
            property.Relation.ThisPartyKind = RelationPartyKind.Dependent;
            property.Relation.RelatedPartyKind = RelationPartyKind.Principal;
            property.Relation.RelatedPartyKey = FindOrAddPrimaryKey(property.Relation.RelatedPartyType);
            property.Relation.InverseProperty = TryFindInverseProperty(type, property);

            CompleteInversePropertyRelation(property);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddToManyRelation(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            _metadataCache.Conventions.ApplyTypeConventions(property.Relation.RelatedPartyType, doPreview: true, doApply: true, doFinalize: false);

            var relatedProperty = property.Relation.RelatedPartyType.Properties.FirstOrDefault(p => p.ClrType == type.ContractType);
            var relatedKey = (
                relatedProperty != null ?
                FindOrAddForeignKey(property.Relation.RelatedPartyType, relatedProperty) :
                FindOrAddPrimaryKey(property.Relation.RelatedPartyType));

            property.Relation.ThisPartyKey = type.PrimaryKey;
            property.Relation.ThisPartyKind = RelationPartyKind.Principal;
            property.Relation.RelatedPartyKind = RelationPartyKind.Dependent;
            property.Relation.RelatedPartyKey = relatedKey;
            property.Relation.InverseProperty = TryFindInverseProperty(type, property);

            CompleteInversePropertyRelation(property);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CompleteInversePropertyRelation(PropertyMetadataBuilder property)
        {
            if ( property.Relation.InverseProperty != null )
            {
                property.Relation.InverseProperty.Relation.InverseProperty = property;

                if ( property.Relation.InverseProperty.Relation.RelatedPartyKey == null )
                {
                    property.Relation.InverseProperty.Relation.RelatedPartyKey = property.Relation.ThisPartyKey;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static PropertyMetadataBuilder TryFindInverseProperty(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            if (property.Relation.InversePropertySuppressed)
            {
                return null;
            }

            return property.Relation.RelatedPartyType.Properties.FirstOrDefault(p =>
                p.Relation != null &&
                p.Relation.RelatedPartyType != null &&
                p.Relation.RelatedPartyType.ContractType.IsAssignableFrom(type.ContractType));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static KeyMetadataBuilder FindOrAddForeignKey(TypeMetadataBuilder type, PropertyMetadataBuilder relationProperty)
        {
            var existingKey = type.AllKeys.FirstOrDefault(k => k.Properties.Select(p => p.ClrType).SingleOrDefault() == relationProperty.ClrType);

            if ( existingKey != null )
            {
                return existingKey;
            }

            var newKey = new KeyMetadataBuilder {
                Kind = KeyKind.Foreign,
                Name = "FK_" + relationProperty.Name
            };

            newKey.Properties.Add(relationProperty);
            type.AllKeys.Add(newKey);

            return newKey;
        }
    }
}

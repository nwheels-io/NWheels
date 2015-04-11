using System.Linq;

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
                foreach ( var property in type.Properties.Where(p => p.IsKey) )
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

        private void CompleteRelations(TypeMetadataBuilder type)
        {
            foreach ( var property in type.Properties.Where(p => p.Kind == PropertyKind.Relation) )
            {
                CompleteRelationMetadata(type, property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CompleteRelationMetadata(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            switch ( property.Relation.RelationKind )
            {
                case RelationKind.OneToOne:
                case RelationKind.ManyToOne:
                    AddToOneRelation(type, property);
                    break;
                case RelationKind.OneToMany:
                case RelationKind.ManyToMany:
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddToManyRelation(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            _metadataCache.Conventions.ApplyTypeConventions(property.Relation.RelatedPartyType, doPreview: true, doApply: true, doFinalize: false);

            var relatedProperty = property.Relation.RelatedPartyType.Properties.FirstOrDefault(p => p.ClrType == type.ContractType);
            var relatedKey = (relatedProperty != null ? FindOrAddForeignKey(property.Relation.RelatedPartyType, relatedProperty) : null);

            property.Relation.ThisPartyKey = type.PrimaryKey;
            property.Relation.ThisPartyKind = RelationPartyKind.Principal;
            property.Relation.RelatedPartyKind = RelationPartyKind.Dependent;
            property.Relation.RelatedPartyKey = relatedKey;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static KeyMetadataBuilder FindOrAddForeignKey(TypeMetadataBuilder type, PropertyMetadataBuilder relationProperty)
        {
            var existingKey = type.AllKeys.FirstOrDefault(k => k.Properties.SingleOrDefault() == relationProperty);

            if ( existingKey != null )
            {
                return existingKey;
            }

            var newKey = new KeyMetadataBuilder
            {
                Kind = KeyKind.Foreign,
                Name = "FK_" + relationProperty.Name
            };

            newKey.Properties.Add(relationProperty);
            type.AllKeys.Add(newKey);

            return newKey;
        }
    }
}

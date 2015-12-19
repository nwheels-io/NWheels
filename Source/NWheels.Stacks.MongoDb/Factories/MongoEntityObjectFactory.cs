using System;
using System.Collections.Generic;
using Autofac;
using Hapil;
using Hapil.Operands;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Factories;
using NWheels.Extensions;
using NWheels.TypeModel;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class MongoEntityObjectFactory : EntityObjectFactory
    {
        public MongoEntityObjectFactory(IComponentContext components, DynamicModule module, TypeMetadataCache metadataCache)
            : base(components, module, metadataCache)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var conventionContext = new ConventionContext();
            var metaType = MetadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);
            var propertyMap = BuildPropertyStrategyMap(context, metaType, conventionContext);

            UpdateProperyRelationalMappings(propertyMap);

            propertyMap.NeedImplementationTypeKey += (sender, args) => {
                args.TypeKeyToUse = CreateImplementationTypeKey(args.ContractType);
            };
            
            return new IObjectFactoryConvention[] {
                new BaseTypeConvention(MetadataCache, metaType), 
                new PropertyImplementationConvention(metaType, propertyMap),
                new EntityContractMethodsNotSupportedConvention(),
                new MaterializationConstructorConvention(metaType, propertyMap),
                new InitializationConstructorConvention(metaType, propertyMap),
                new ImplementIObjectConvention(), 
                new EntityObjectStateConvention(), 
                new ImplementIEntityObjectConvention(metaType, propertyMap), 
                new ImplementIEntityPartObjectConvention(metaType, propertyMap), 
                new ImplementIPartitionedObjectConvention(metaType), 
                //new EnsureDomainObjectConvention(metaType), 
                new DependencyInjectionConvention(metaType, propertyMap, forceApply: true),
                new ContextImplTypeInjectionConvention(conventionContext),
                new NestedObjectsConvention(propertyMap), 
                new ObjectIdGeneratorConvention(propertyMap, metaType), 
                new BsonIgnoreConvention(MetadataCache, metaType),
                new BsonDiscriminatorConvention(context, MetadataCache, metaType),
                new BsonIgnoreExtraElementsConvention(),
                new BsonIdAttributeConvention(metaType)
                //new LazyLoadDomainObjectConvention(metaType)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override TypeKey CreateImplementationTypeKey(Type entityContractInterface)
        {
            return new TypeKey(
                primaryInterface: entityContractInterface,
                baseType: BaseTypeConvention.GetBaseType(this, base.MetadataCache.GetTypeMetadata(entityContractInterface)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PropertyImplementationStrategyMap BuildPropertyStrategyMap(
            ObjectFactoryContext context, 
            ITypeMetadata metaType, 
            ConventionContext conventionContext)
        {
            var builder = new PropertyImplementationStrategyMap.Builder();
            Type collectionItemType;

            //-- calculated properties

            builder.AddRule(
                p => p.IsCalculated,
                p => new NotSupportedPropertyStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            //-- entity parts

            builder.AddRule(
                p => p.Kind == PropertyKind.Part && p.IsCollection,
                p => new CollectionAdapterStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Part && !p.IsCollection,
                p => new RelationTypecastStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            //-- entity relations - collections

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && p.IsCollection && ShouldPersistAsEmbeddedDocuments(p),
                p => new CollectionAdapterStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && p.IsCollection && ShouldPersistAsArrayOfDocumentIds(p),
                p => new ArrayOfDocumentIdsStrategy(conventionContext, builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && p.IsCollection,
                p => new LazyLoadCollectionByForeignKeyStrategy(conventionContext, builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            //-- entity relations - single (non-collection)
            
            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && !p.IsCollection && ShouldPersistAsEmbeddedDocuments(p),
                p => new RelationTypecastStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && !p.IsCollection && ShouldPersistAsDocumentId(p),
                p => new DocumentIdStrategy(conventionContext, builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && !p.IsCollection,
                p => new LazyLoadDocumentByForeignKeyStrategy(conventionContext, builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            //-- scalar properties

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && p.RelationalMapping != null && p.RelationalMapping.StorageType != null,
                p => new StorageDataTypeStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && !(p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite),
                p => new PublicAccessorWrapperStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite,
                p => new AutomaticPropertyStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            return builder.Build(MetadataCache, metaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ShouldPersistAsEmbeddedDocuments(IPropertyMetadata metaProperty)
        {
            if ( metaProperty.Kind != PropertyKind.Relation )
            {
                return false;
            }

            if ( metaProperty.Relation.RelatedPartyType.IsEntityPart )
            {
                return true;
            }

            if ( metaProperty.Relation.RelatedPartyType.IsEntity )
            {
                if ( metaProperty.RelationalMapping != null && metaProperty.RelationalMapping.StorageStyle != PropertyStorageStyle.Undefined )
                {
                    return metaProperty.RelationalMapping.IsEmbeddedInParent;
                }
                else
                {
                    return (metaProperty.Relation.Kind == RelationKind.Composition);
                }
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ShouldPersistAsArrayOfDocumentIds(IPropertyMetadata metaProperty)
        {
            if ( metaProperty.Kind != PropertyKind.Relation || !metaProperty.Relation.RelatedPartyType.IsEntity )
            {
                return false;
            }

            switch ( metaProperty.Relation.Multiplicity )
            {
                case RelationMultiplicity.ManyToMany:
                    return (metaProperty.Relation.ThisPartyKind == RelationPartyKind.Dependent || metaProperty.Relation.InverseProperty == null);
                case RelationMultiplicity.OneToMany:
                    return (metaProperty.Relation.InverseProperty == null);
                default:
                    return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ShouldPersistAsDocumentId(IPropertyMetadata metaProperty)
        {
            if ( metaProperty.Kind != PropertyKind.Relation || !metaProperty.Relation.RelatedPartyType.IsEntity )
            {
                return false;
            }

            switch ( metaProperty.Relation.Multiplicity )
            {
                case RelationMultiplicity.OneToOne:
                    return (metaProperty.Relation.ThisPartyKind == RelationPartyKind.Dependent || metaProperty.Relation.InverseProperty == null);
                case RelationMultiplicity.ManyToOne:
                    return true;
                default:
                    return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, PropertyStorageStyle> _s_storageStyleByStrategyType = new Dictionary<Type, PropertyStorageStyle>() {
            { typeof(RelationTypecastStrategy), PropertyStorageStyle.EmbeddedObject },
            { typeof(CollectionAdapterStrategy), PropertyStorageStyle.EmbeddedObjectCollection },
            { typeof(DocumentIdStrategy), PropertyStorageStyle.InlineForeignKey },
            { typeof(ArrayOfDocumentIdsStrategy), PropertyStorageStyle.EmbeddedForeignKeyCollection },
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void UpdateProperyRelationalMappings(PropertyImplementationStrategyMap propertyMap)
        {
            foreach ( var strategy in propertyMap.Strategies )
            {
                PropertyStorageStyle storageStyle;

                if ( _s_storageStyleByStrategyType.TryGetValue(strategy.GetType(), out storageStyle) )
                {
                    var writableMetaProperty = (PropertyMetadataBuilder)strategy.MetaProperty;
                    writableMetaProperty.SafeGetRelationalMapping().StorageStyle = storageStyle;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConventionContext
        {
            public Field<Type> ContextImplTypeField { get; set; }
        }
    }
}

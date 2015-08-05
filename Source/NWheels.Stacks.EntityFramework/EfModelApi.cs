using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Hapil;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Stacks.EntityFramework.Factories;

namespace NWheels.Stacks.EntityFramework
{
    public static class EfModelApi
    {
        public static Expression<Func<TEntity, TProperty>> PropertyExpression<TEntity, TProperty>(PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            return Expression.Lambda<Func<TEntity, TProperty>>(Expression.Property(parameter, property), new[] { parameter });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Expression<Func<TEntity, TComplexProperty>> PropertyExpression<TEntity, TComplexProperty>(
            PropertyInfo entityProperty, 
            PropertyInfo complexTypeProperty)
        {
            ParameterExpression expression;
            return (
                Expression.Lambda<Func<TEntity, TComplexProperty>>(
                    Expression.Property(
                        Expression.Property(
                            expression = Expression.Parameter(typeof(TEntity), "x"), 
                            property: entityProperty
                        ),
                        complexTypeProperty
                    ),
                    new ParameterExpression[] { expression }
                )
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityTypeConfiguration<TEntity> EntityType<TEntity>(
            DbModelBuilder builder, 
            ITypeMetadata entity, 
            ITypeMetadataCache metadataCache)
            where TEntity : class
        {
            var entityConfiguration = builder.Entity<TEntity>();

            entityConfiguration.HasEntitySetName(entity.Name);

            if ( entity.RelationalMapping != null )
            {
                if ( !string.IsNullOrEmpty(entity.RelationalMapping.PrimaryTableName) )
                {
                    string tableName;

                    if ( !string.IsNullOrEmpty(entity.NamespaceQualifier) )
                    {
                        var namingConvention = metadataCache.Conventions.RelationalMappingConventions.OfType<IStorageSchemaNamingConvention>().First();
                        tableName = namingConvention.QualifyTableNameWithNamespace(entity, entity.RelationalMapping.PrimaryTableName, entity.NamespaceQualifier);
                    }
                    else
                    {
                        tableName = entity.RelationalMapping.PrimaryTableName;
                    }

                    entityConfiguration.ToTable(tableName);
                }
            }

            return entityConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityTypeConfiguration<TDerivedEntity> InheritedEntityType<TBaseEntity, TDerivedEntity>(
            DbModelBuilder builder, 
            ITypeMetadata derivedEntity, 
            string discriminatorColumnName,
            string discriminatorColumnValue)
            where TBaseEntity : class
            where TDerivedEntity : class, TBaseEntity
        {
            var derivedEntityConfiguration = builder.Entity<TDerivedEntity>();
            
            builder.Entity<TBaseEntity>().Map<TDerivedEntity>(m => m.Requires(discriminatorColumnName).HasValue(discriminatorColumnValue));
            
            return derivedEntityConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ComplexType<TEntity>(DbModelBuilder builder)
            where TEntity : class
        {
            //builder.ComplexType<TEntity>().Property()
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PrimitivePropertyConfiguration ValueTypePrimitiveProperty<TEntity, TProperty>(
            EntityTypeConfiguration<TEntity> entity,
            IPropertyMetadata property)
            where TEntity : class
            where TProperty : struct
        {
            var propertyConfiguration = entity.Property<TProperty>(PropertyExpression<TEntity, TProperty>(property.GetImplementationBy<EfEntityObjectFactory>()));
            ConfigurePrimitivePropertyMapping(property, propertyConfiguration);
            return propertyConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PrimitivePropertyConfiguration NullableValueTypePrimitiveProperty<TEntity, TProperty>(
            EntityTypeConfiguration<TEntity> entity,
            IPropertyMetadata property)
            where TEntity : class
            where TProperty : struct
        {
            var propertyConfiguration = entity.Property<TProperty>(PropertyExpression<TEntity, TProperty?>(property.GetImplementationBy<EfEntityObjectFactory>()));
            ConfigurePrimitivePropertyMapping(property, propertyConfiguration);
            return propertyConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PrimitivePropertyConfiguration StringProperty<TEntity>(
            EntityTypeConfiguration<TEntity> entity,
            IPropertyMetadata property)
            where TEntity : class
        {
            var propertyConfiguration = entity.Property(PropertyExpression<TEntity, string>(property.GetImplementationBy<EfEntityObjectFactory>()));
            ConfigurePrimitivePropertyMapping(property, propertyConfiguration);
            return propertyConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PrimitivePropertyConfiguration ByteArrayProperty<TEntity>(
            EntityTypeConfiguration<TEntity> entity,
            IPropertyMetadata property)
            where TEntity : class
        {
            var propertyConfiguration = entity.Property(PropertyExpression<TEntity, byte[]>(property.GetImplementationBy<EfEntityObjectFactory>()));
            ConfigurePrimitivePropertyMapping(property, propertyConfiguration);
            return propertyConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static CascadableNavigationPropertyConfiguration OneToManyNoInverseRelationProperty<TOneEntity, TManyEntity>(
            EntityTypeConfiguration<TOneEntity> oneEntity,
            IPropertyMetadata oneProperty)
            where TOneEntity : class
            where TManyEntity : class
        {
            var navigation = oneEntity.HasMany(PropertyExpression<TOneEntity, ICollection<TManyEntity>>(oneProperty.GetImplementationBy<EfEntityObjectFactory>()))
                .WithRequired()
                .Map(cfg => cfg.MapKey(oneProperty.RelationalMapping.RelatedColumnName));

            return navigation;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ForeignKeyNavigationPropertyConfiguration ManyToOneRelationProperty<TManyEntity, TOneEntity>(
            EntityTypeConfiguration<TManyEntity> manyEntity,
            IPropertyMetadata manyProperty)
            where TManyEntity : class
            where TOneEntity : class
        {
            var required = manyEntity.HasRequired<TOneEntity>(
                PropertyExpression<TManyEntity, TOneEntity>(manyProperty.GetImplementationBy<EfEntityObjectFactory>()));

            ForeignKeyNavigationPropertyConfiguration foreignKey;

            if ( manyProperty.Relation.InverseProperty != null && manyProperty.Relation.InverseProperty.ClrType.IsCollectionType() )
            {
                foreignKey = required.WithMany(PropertyExpression<TOneEntity, ICollection<TManyEntity>>(
                    manyProperty.Relation.InverseProperty.GetImplementationBy<EfEntityObjectFactory>()));
            }
            else
            {
                foreignKey = required.WithMany();
            }

            foreignKey.Map(m => m.MapKey(new[] { manyProperty.RelationalMapping.ColumnName }));
            return foreignKey;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ManyToManyNavigationPropertyConfiguration<TThisEntity, TOtherEntity> ManyToManyRelationProperty<TThisEntity, TOtherEntity>(
            EntityTypeConfiguration<TThisEntity> thisEntity,
            IPropertyMetadata thisProperty,
            ITypeMetadataCache metadataCache)
            where TThisEntity : class
            where TOtherEntity : class
        {
            var otherEntityType = thisProperty.Relation.RelatedPartyType.GetImplementationBy<EfEntityObjectFactory>();
            var inverseProperty = otherEntityType.GetProperty(GetGeneratedInversePropertyName(thisProperty));

            var toManyConfig = thisEntity.HasMany<TOtherEntity>(
                PropertyExpression<TThisEntity, ICollection<TOtherEntity>>(
                    thisProperty.GetImplementationBy<EfEntityObjectFactory>()));

            var manyToManyConfig = toManyConfig.WithMany(PropertyExpression<TOtherEntity, ICollection<TThisEntity>>(inverseProperty));

            var leftKeyColumnName = thisProperty.RelationalMapping.ColumnName ?? string.Format(
                "{0}{1}",
                thisProperty.DeclaringContract.Name,
                thisProperty.DeclaringContract.PrimaryKey.Properties.First().Name);

            var rightKeyColumnName = thisProperty.RelationalMapping.RelatedColumnName ?? string.Format(
                "{0}{1}",
                thisProperty.Relation.RelatedPartyType.Name,
                thisProperty.Relation.RelatedPartyType.PrimaryKey.Properties.First().Name);

            var relationTableName = thisProperty.RelationalMapping.TableName ?? string.Format(
                "{0}{1}",
                thisProperty.DeclaringContract.Name,
                thisProperty.Name);

            var namingConvention = metadataCache.Conventions.RelationalMappingConventions.OfType<IStorageSchemaNamingConvention>().First();
            relationTableName = namingConvention.QualifyTableNameWithNamespace(
                thisProperty.DeclaringContract, 
                relationTableName, 
                thisProperty.DeclaringContract.NamespaceQualifier);

            manyToManyConfig.Map(cfg => {
                cfg.MapLeftKey(leftKeyColumnName);
                cfg.MapRightKey(rightKeyColumnName);
                cfg.ToTable(relationTableName);
            });

            return manyToManyConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ComplexTypeProperty<TEntity, TProperty>(
            DbModelBuilder modelBuilder,
            EntityTypeConfiguration<TEntity> entity, 
            IPropertyMetadata entityProperty,
            IPropertyMetadata complexTypeProperty) 
            where TEntity : class
        {
            var propertyExpression = PropertyExpression<TEntity, TProperty>(
                entityProperty.GetImplementationBy<EfEntityObjectFactory>(),
                complexTypeProperty.GetImplementationBy<EfEntityObjectFactory>());

            var complexTypePropertyCopy = complexTypeProperty;

            modelBuilder.Types<TEntity>().Configure(cfg => cfg.Property(propertyExpression).HasColumnName(
                entityProperty.RelationalMapping.ColumnName + 
                complexTypePropertyCopy.RelationalMapping.ColumnName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetGeneratedInversePropertyName(IPropertyMetadata property)
        {
            return string.Format("Inverse_{0}_{1}", property.DeclaringContract.Name, property.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ConfigurePrimitivePropertyMapping(IPropertyMetadata metadata, PrimitivePropertyConfiguration configuration)
        {
            if ( metadata.RelationalMapping != null )
            {
                if ( !string.IsNullOrWhiteSpace(metadata.RelationalMapping.ColumnName) )
                {
                    configuration.HasColumnName(metadata.RelationalMapping.ColumnName);
                }

                if ( !string.IsNullOrWhiteSpace(metadata.RelationalMapping.ColumnType) )
                {
                    configuration.HasColumnType(metadata.RelationalMapping.ColumnType);
                }
            }
        }
    }
}

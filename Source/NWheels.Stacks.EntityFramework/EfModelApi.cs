using System;
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

        public static EntityTypeConfiguration<TEntity> EntityType<TEntity>(DbModelBuilder builder, ITypeMetadata entity)
            where TEntity : class
        {
            var entityConfiguration = builder.Entity<TEntity>();

            entityConfiguration.HasEntitySetName(entity.Name);

            if ( entity.RelationalMapping != null )
            {
                if ( !string.IsNullOrEmpty(entity.RelationalMapping.PrimaryTableName) )
                {
                    entityConfiguration.ToTable(entity.RelationalMapping.PrimaryTableName);
                }
            }

            return entityConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ComplexType<TEntity>(DbModelBuilder builder)
            where TEntity : class
        {
            builder.ComplexType<TEntity>();
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
            IPropertyMetadata thisProperty)
            where TThisEntity : class
            where TOtherEntity : class
        {
            var otherEntityType = thisProperty.Relation.RelatedPartyType.GetImplementationBy<EfEntityObjectFactory>();
            var inverseProperty = otherEntityType.GetProperty(GetGeneratedInversePropertyName(thisProperty));

            var toManyConfig = thisEntity.HasMany<TOtherEntity>(
                PropertyExpression<TThisEntity, ICollection<TOtherEntity>>(
                    thisProperty.GetImplementationBy<EfEntityObjectFactory>()));

            var manyToManyConfig = toManyConfig.WithMany(PropertyExpression<TOtherEntity, ICollection<TThisEntity>>(inverseProperty));
            
            var leftKeyColumnName = string.Format(
                "{0}{1}",
                thisProperty.DeclaringContract.Name,
                thisProperty.DeclaringContract.PrimaryKey.Properties.First().Name);
            
            var rightKeyColumnName = string.Format(
                "{0}{1}",
                thisProperty.Relation.RelatedPartyType.Name,
                thisProperty.Relation.RelatedPartyType.PrimaryKey.Properties.First().Name);

            var relationTableName = string.Format(
                "{0}{1}",
                thisProperty.DeclaringContract.Name,
                thisProperty.Name);

            manyToManyConfig.Map(cfg => {
                cfg.MapLeftKey(leftKeyColumnName);
                cfg.MapRightKey(rightKeyColumnName);
                cfg.ToTable(relationTableName);
            });

            return manyToManyConfig;
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

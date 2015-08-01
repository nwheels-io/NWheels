using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq.Expressions;
using System.Reflection;
using Hapil;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
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

        //public static ForeignKeyNavigationPropertyConfiguration ManyToManyRelationProperty<TThisEntity, TOtherEntity>(
        //    EntityTypeConfiguration<TThisEntity> thisEntity,
        //    PropertyImplementationStrategy implementor)
        //    where TThisEntity : class
        //    where TOtherEntity : class
        //{
        //    var required = thisEntity
        //        .HasMany<TOtherEntity>(PropertyExpression<TThisEntity, ICollection<TOtherEntity>>(thisProperty.GetImplementationBy<EfEntityObjectFactory>()))
        //        .WithMany(PropertyExpression<TOtherEntity, ICollection<TThisEntity>>(thisProperty.))

        //    ForeignKeyNavigationPropertyConfiguration foreignKey;

        //    if (thisProperty.Relation.InverseProperty != null && thisProperty.Relation.InverseProperty.ClrType.IsCollectionType())
        //    {
        //        foreignKey = required.WithMany(PropertyExpression<TOneEntity, ICollection<TManyEntity>>(
        //            thisProperty.Relation.InverseProperty.GetImplementationBy<EfEntityObjectFactory>()));
        //    }
        //    else
        //    {
        //        foreignKey = required.WithMany();
        //    }

        //    foreignKey.Map(m => m.MapKey(new[] { thisProperty.RelationalMapping.ColumnName }));
        //    return foreignKey;
        //}

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

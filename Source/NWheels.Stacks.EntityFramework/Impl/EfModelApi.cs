using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.DataObjects;

namespace NWheels.Stacks.EntityFramework.Impl
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

        public static PrimitivePropertyConfiguration ValueTypePrimitiveProperty<TEntity, TProperty>(
            EntityTypeConfiguration<TEntity> entity,
            IPropertyMetadata property)
            where TEntity : class
            where TProperty : struct
        {
            var propertyConfiguration = entity.Property<TProperty>(PropertyExpression<TEntity, TProperty>(property.ImplementationPropertyInfo));
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
            var propertyConfiguration = entity.Property<TProperty>(PropertyExpression<TEntity, TProperty?>(property.ImplementationPropertyInfo));
            ConfigurePrimitivePropertyMapping(property, propertyConfiguration);
            return propertyConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PrimitivePropertyConfiguration StringProperty<TEntity>(
            EntityTypeConfiguration<TEntity> entity,
            IPropertyMetadata property)
            where TEntity : class
        {
            var propertyConfiguration = entity.Property(PropertyExpression<TEntity, string>(property.ImplementationPropertyInfo));
            ConfigurePrimitivePropertyMapping(property, propertyConfiguration);
            return propertyConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PrimitivePropertyConfiguration ByteArrayProperty<TEntity>(
            EntityTypeConfiguration<TEntity> entity,
            IPropertyMetadata property)
            where TEntity : class
        {
            var propertyConfiguration = entity.Property(PropertyExpression<TEntity, byte[]>(property.ImplementationPropertyInfo));
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
                PropertyExpression<TManyEntity, TOneEntity>(manyProperty.ImplementationPropertyInfo));

            ForeignKeyNavigationPropertyConfiguration foreignKey;

            if ( manyProperty.Relation.InverseProperty != null && manyProperty.Relation.InverseProperty.ClrType.IsCollectionType() )
            {
                foreignKey = required.WithMany(PropertyExpression<TOneEntity, ICollection<TManyEntity>>(
                    manyProperty.Relation.InverseProperty.ImplementationPropertyInfo));
            }
            else
            {
                foreignKey = required.WithMany();
            }

            foreignKey.Map(m => m.MapKey(new[] { manyProperty.RelationalMapping.ColumnName }));
            return foreignKey;
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

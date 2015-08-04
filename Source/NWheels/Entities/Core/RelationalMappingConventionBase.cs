using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;

namespace NWheels.Entities.Core
{
    public abstract class RelationalMappingConventionBase : IRelationalMappingConvention
    {
        private PluralizationService _pluralizationService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IRelationalMappingConvention Members

        void IRelationalMappingConvention.Preview(ITypeMetadata type)
        {
            PreviewType((TypeMetadataBuilder)type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IRelationalMappingConvention.Apply(ITypeMetadata type)
        {
            ApplyToType((TypeMetadataBuilder)type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IRelationalMappingConvention.Finalize(ITypeMetadata type)
        {
            FinalizeType((TypeMetadataBuilder)type);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void PreviewType(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void FinalizeType(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void ApplyToType(TypeMetadataBuilder type)
        {
            SetTypeDefaultMapping(type);

            var keyProperties = new HashSet<PropertyMetadataBuilder>();

            foreach ( var key in type.AllKeys )
            {
                foreach ( var property in key.Properties )
                {
                    keyProperties.Add(property);
                    SetKeyPropertyDefaultMapping(type, property, key);
                }
            }

            foreach ( var property in type.Properties.Where(p => !keyProperties.Contains(p)) )
            {
                SetNonKeyPropertyDefaultMapping(type, property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract string NameTypePrimaryTable(TypeMetadataBuilder type);
        protected abstract string NameTypeRelationTable(TypeMetadataBuilder type1, TypeMetadataBuilder type2);
        protected abstract string NamePropertyColumnTable(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        protected abstract string NamePropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        protected abstract string NamePropertyColumnType(TypeMetadataBuilder type, PropertyMetadataBuilder property);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual RelationalInheritanceKind? GetInheritanceKind(TypeMetadataBuilder type)
        {
            return RelationalInheritanceKind.TablePerHierarchy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameKeyPropertyColumnType(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property)
        {
            return NamePropertyColumnType(type, property);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameKeyPropertyColumn(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property)
        {
            if ( key.Kind == KeyKind.Foreign )
            {
                return NameForeignKeyPropertyColumn(type, property);
            }
            else
            {
                return NamePropertyColumn(type, property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameForeignKeyPropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return NamePropertyColumn(type, property) + property.Relation.RelatedPartyType.PrimaryKey.Properties.First().Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameRelationTableForeignKeyColumn(TypeMetadataBuilder type)
        {
            return type.Name + type.PrimaryKey.Properties[0].Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameKeyPropertyColumnTable(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property)
        {
            return NamePropertyColumnTable(type, property);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameEntityPartColumnPrefix(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return property.Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PluralizationService PluralizationService
        {
            get
            {
                if ( _pluralizationService == null )
                {
                    _pluralizationService = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en"));
                }

                return _pluralizationService;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetTypeDefaultMapping(TypeMetadataBuilder type)
        {
            var mapping = type.SafeGetRelationalMapping();

            if ( string.IsNullOrWhiteSpace(mapping.PrimaryTableName) )
            {
                mapping.PrimaryTableName = NameTypePrimaryTable(type);
            }

            if ( !mapping.InheritanceKind.HasValue )
            {
                mapping.InheritanceKind = GetInheritanceKind(type);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetNonKeyPropertyDefaultMapping(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            if ( property.Kind == PropertyKind.Relation && property.Relation.Multiplicity == RelationMultiplicity.ManyToMany )
            {
                SetPropertyDefaultMapping(
                    property,
                    defaultTableName: NameTypeRelationTable(type, property.Relation.RelatedPartyType),
                    defaultColumnName: NameRelationTableForeignKeyColumn(type),
                    defaultColumnType: NamePropertyColumnType(type, type.PrimaryKey.Properties[0]),
                    defaultRelatedColumnName: NameRelationTableForeignKeyColumn(property.Relation.RelatedPartyType),
                    defaultRelatedColumnType:
                        NamePropertyColumnType(property.Relation.RelatedPartyType, property.Relation.RelatedPartyType.PrimaryKey.Properties[0]));
            }
            else if ( property.Kind == PropertyKind.Relation && property.Relation.Multiplicity == RelationMultiplicity.OneToMany )
            {
                SetPropertyDefaultMapping(
                    property,
                    defaultTableName: null,
                    defaultColumnName: null,
                    defaultColumnType: null,
                    defaultRelatedColumnName: NameRelationTableForeignKeyColumn(type),
                    defaultRelatedColumnType: NamePropertyColumnType(type, type.PrimaryKey.Properties[0]));
            }
            else if ( property.Kind == PropertyKind.Part )
            {
                SetPropertyDefaultMapping(
                    property,
                    defaultTableName: null,
                    defaultColumnName: NameEntityPartColumnPrefix(type, property),
                    defaultColumnType: null);
            }
            else
            {
                SetPropertyDefaultMapping(
                    property,
                    defaultTableName: NamePropertyColumnTable(type, property),
                    defaultColumnName: NamePropertyColumn(type, property),
                    defaultColumnType: NamePropertyColumnType(type, property));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetKeyPropertyDefaultMapping(TypeMetadataBuilder type, PropertyMetadataBuilder property, KeyMetadataBuilder key)
        {
            SetPropertyDefaultMapping(
                property,
                defaultTableName: NameKeyPropertyColumnTable(type, key, property),
                defaultColumnName: NameKeyPropertyColumn(type, key, property),
                defaultColumnType: NameKeyPropertyColumnType(type, key, property));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetPropertyDefaultMapping(
            PropertyMetadataBuilder property, 
            string defaultTableName, 
            string defaultColumnName, 
            string defaultColumnType,
            string defaultRelatedColumnName,
            string defaultRelatedColumnType)
        {
            SetPropertyDefaultMapping(property, defaultTableName, defaultColumnName, defaultColumnType);
            
            var mapping = property.SafeGetRelationalMapping();

            if ( string.IsNullOrWhiteSpace(mapping.RelatedColumnName) )
            {
                mapping.RelatedColumnName = defaultRelatedColumnName;
            }

            if ( string.IsNullOrWhiteSpace(mapping.RelatedColumnType) )
            {
                mapping.RelatedColumnType = defaultRelatedColumnType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetPropertyDefaultMapping(PropertyMetadataBuilder property, string defaultTableName, string defaultColumnName, string defaultColumnType)
        {
            var mapping = property.SafeGetRelationalMapping();

            if ( string.IsNullOrWhiteSpace(mapping.TableName) )
            {
                mapping.TableName = defaultTableName;
            }

            if ( string.IsNullOrWhiteSpace(mapping.ColumnName) )
            {
                mapping.ColumnName = defaultColumnName;
            }

            if ( string.IsNullOrWhiteSpace(mapping.ColumnType) )
            {
                mapping.ColumnType = defaultColumnType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static IRelationalMappingConvention FromDefault(RelationalMappingConventionDefault @default)
        {
            switch ( @default.Type )
            {
                case RelationalMappingConventionDefault.ConventionType.PascalCase:
                    return new PascalCaseRelationalMappingConvention(@default.UsePluralTableNames);
                case RelationalMappingConventionDefault.ConventionType.Underscore:
                    return new UnderscoreRelationalMappingConvention(@default.UsePluralTableNames);
                default:
                    throw new ArgumentOutOfRangeException("default.Type");
            }
        }
    }
}

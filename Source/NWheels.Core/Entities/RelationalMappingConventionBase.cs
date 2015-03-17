using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Core.DataObjects;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Core.Entities
{
    public abstract class RelationalMappingConventionBase : IRelationalMappingConvention
    {
        private PluralizationService _pluralizationService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IRelationalMappingConvention Members

        void IRelationalMappingConvention.ApplyToType(ITypeMetadata type)
        {
            ApplyToType((TypeMetadataBuilder)type);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void ApplyToType(TypeMetadataBuilder type)
        {
            type.RelationalMapping = new TypeRelationalMappingBuilder() {
                PrimaryTableName = NameTypePrimaryTable(type),
                InheritanceKind = GetInheritanceKind(type)
            };

            var keyProperties = new HashSet<PropertyMetadataBuilder>();

            foreach ( var key in type.AllKeys )
            {
                foreach ( var property in key.Properties )
                {
                    keyProperties.Add(property);

                    property.RelationalMapping = new PropertyRelationalMappingBuilder() {
                        TableName = NameKeyPropertyColumnTable(type, key, property),
                        ColumnName = NameKeyPropertyColumn(type, key, property),
                        DataTypeName = NameKeyPropertyColumnDataType(type, key, property)
                    };
                }
            }

            foreach ( var property in type.Properties.Where(p => !keyProperties.Contains(p)) )
            {
                property.RelationalMapping = new PropertyRelationalMappingBuilder() {
                    TableName = NamePropertyColumnTable(type, property),
                    ColumnName = NamePropertyColumn(type, property),
                    DataTypeName = NamePropertyColumnDataType(type, property)
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract string NameTypePrimaryTable(TypeMetadataBuilder type);
        protected abstract string NamePropertyColumnTable(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        protected abstract string NamePropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        protected abstract string NamePropertyColumnDataType(TypeMetadataBuilder type, PropertyMetadataBuilder property);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual RelationalInheritanceKind? GetInheritanceKind(TypeMetadataBuilder type)
        {
            return RelationalInheritanceKind.TablePerHierarchy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameKeyPropertyColumnDataType(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property)
        {
            return NamePropertyColumnDataType(type, property);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameKeyPropertyColumn(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property)
        {
            return NamePropertyColumn(type, property);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NameKeyPropertyColumnTable(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property)
        {
            return NamePropertyColumnTable(type, property);
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

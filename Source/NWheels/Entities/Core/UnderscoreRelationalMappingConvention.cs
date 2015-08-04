using System.Linq;
using Hapil;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Extensions;

namespace NWheels.Entities.Core
{
    public class UnderscoreRelationalMappingConvention : RelationalMappingConventionBase
    {
        private readonly bool _usePluralTableNames;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UnderscoreRelationalMappingConvention(bool usePluralTableNames)
        {
            _usePluralTableNames = usePluralTableNames;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NameTypePrimaryTable(TypeMetadataBuilder type)
        {
            return ToUnderscoreConvention(_usePluralTableNames ? base.PluralizationService.Pluralize(type.Name.TrimSuffix("Entity")) : type.Name);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NameTypeRelationTable(TypeMetadataBuilder type1, TypeMetadataBuilder type2)
        {
            return ToUnderscoreConvention(type1.Name.TrimSuffix("Entity")) + "_" + NameTypePrimaryTable(type2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NamePropertyColumnTable(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return type.RelationalMapping.PrimaryTableName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NamePropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return ToUnderscoreConvention(property.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NamePropertyColumnType(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NameForeignKeyPropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return (
                NamePropertyColumn(type, property) + 
                "_" + 
                NamePropertyColumn(property.Relation.RelatedPartyType, property.Relation.RelatedPartyType.PrimaryKey.Properties.First()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NameRelationTableForeignKeyColumn(TypeMetadataBuilder type)
        {
            return ToUnderscoreConvention(type.Name) + "_" + ToUnderscoreConvention(type.PrimaryKey.Properties[0].Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NameEntityPartColumnPrefix(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return ToUnderscoreConvention(property.Name) + "_";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string ToUnderscoreConvention(string pascalCase)
        {
            return pascalCase.SplitPascalCase(delimiter: '_').ToLower();
        }
    }
}

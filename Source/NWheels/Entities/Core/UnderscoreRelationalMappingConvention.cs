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

        public override string NameTypePrimaryTable(TypeMetadataBuilder type)
        {
            return ToUnderscoreConvention(_usePluralTableNames ? base.PluralizationService.Pluralize(type.Name.TrimSuffix("Entity")) : type.Name);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NameTypeRelationTable(TypeMetadataBuilder type1, TypeMetadataBuilder type2)
        {
            return ToUnderscoreConvention(type1.Name.TrimSuffix("Entity")) + "_" + NameTypePrimaryTable(type2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NamePropertyColumnTable(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return type.RelationalMapping.PrimaryTableName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NamePropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return ToUnderscoreConvention(property.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NamePropertyColumnType(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NameForeignKeyPropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return (
                NamePropertyColumn(type, property) + 
                "_" + 
                NamePropertyColumn(property.Relation.RelatedPartyType, property.Relation.RelatedPartyType.PrimaryKey.Properties.First()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NameRelationTableForeignKeyColumn(TypeMetadataBuilder type)
        {
            return ToUnderscoreConvention(type.Name) + "_" + ToUnderscoreConvention(type.PrimaryKey.Properties[0].Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NameEntityPartColumnPrefix(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return ToUnderscoreConvention(property.Name) + "_";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string QualifyTableNameWithNamespace(ITypeMetadata type, string tableName, string namespaceName)
        {
            if ( !string.IsNullOrEmpty(namespaceName) )
            {
                return ToUnderscoreConvention(namespaceName) + "_" + tableName;
            }
            else
            {
                return tableName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string ToUnderscoreConvention(string pascalCase)
        {
            return pascalCase.SplitPascalCase(delimiter: '_').ToLower();
        }
    }
}

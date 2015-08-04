using Hapil;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;

namespace NWheels.Entities.Core
{
    public class PascalCaseRelationalMappingConvention : RelationalMappingConventionBase
    {
        private readonly bool _usePluralTableNames;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PascalCaseRelationalMappingConvention(bool usePluralTableNames)
        {
            _usePluralTableNames = usePluralTableNames;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NameTypePrimaryTable(TypeMetadataBuilder type)
        {
            return (_usePluralTableNames ? base.PluralizationService.Pluralize(type.Name.TrimSuffix("Entity")) : type.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NameTypeRelationTable(TypeMetadataBuilder type1, TypeMetadataBuilder type2)
        {
            return type1.Name.TrimSuffix("Entity") + NameTypePrimaryTable(type2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NamePropertyColumnTable(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return type.RelationalMapping.PrimaryTableName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NamePropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return property.Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string NamePropertyColumnType(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string QualifyTableNameWithNamespace(ITypeMetadata type, string tableName, string namespaceName)
        {
            if ( !string.IsNullOrEmpty(namespaceName) )
            {
                return namespaceName + "." + tableName;
            }
            else
            {
                return tableName;
            }
        }
    }
}

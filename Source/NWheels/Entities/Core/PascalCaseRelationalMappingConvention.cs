using Hapil;
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

        protected override string NameTypePrimaryTable(TypeMetadataBuilder type)
        {
            return (_usePluralTableNames ? base.PluralizationService.Pluralize(type.Name.TrimSuffix("Entity")) : type.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NameTypeRelationTable(TypeMetadataBuilder type1, TypeMetadataBuilder type2)
        {
            return type1.Name.TrimSuffix("Entity") + NameTypePrimaryTable(type2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NamePropertyColumnTable(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return type.RelationalMapping.PrimaryTableName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NamePropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return property.Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string NamePropertyColumnType(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Core.DataObjects;
using NWheels.Extensions;

namespace NWheels.Core.Entities
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
            return ToUnderscoreConvention(_usePluralTableNames ? base.PluralizationService.Pluralize(type.Name) : type.Name);
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

        protected override string NamePropertyColumnDataType(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string ToUnderscoreConvention(string pascalCase)
        {
            return pascalCase.SplitPascalCase(delimiter: '_').ToLower();
        }
    }
}

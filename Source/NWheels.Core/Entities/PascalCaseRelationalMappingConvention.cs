using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.DataObjects;
using NWheels.Core.DataObjects;

namespace NWheels.Core.Entities
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
            return (_usePluralTableNames ? base.PluralizationService.Pluralize(type.Name) : type.Name);
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

        protected override string NamePropertyColumnDataType(TypeMetadataBuilder type, PropertyMetadataBuilder property)
        {
            return null;
        }
    }
}

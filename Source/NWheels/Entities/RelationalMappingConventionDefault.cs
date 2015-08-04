using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public class RelationalMappingConventionDefault
    {
        private readonly ConventionType _type;
        private readonly bool _usePluralTableNames;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RelationalMappingConventionDefault(ConventionType type, bool usePluralTableNames)
        {
            _usePluralTableNames = usePluralTableNames;
            _type = type;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConventionType Type
        {
            get { return _type; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool UsePluralTableNames
        {
            get { return _usePluralTableNames; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum ConventionType
        {
            PascalCase,
            Underscore
        }
    }
}

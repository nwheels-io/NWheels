using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public class TypeRelationalMappingBuilder : ITypeRelationalMapping
    {
        #region ITypeRelationalMapping Members

        public string PrimaryTableName { get; set; }
        public RelationalInheritanceKind? InheritanceKind { get; set; }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public class TypeRelationalMappingBuilder : MetadataElement<ITypeRelationalMapping>, ITypeRelationalMapping
    {
        #region IMetadataElement Members

        public override string ReferenceName
        {
            get { return this.PrimaryTableName; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region ITypeRelationalMapping Members

        public string PrimaryTableName { get; set; }
        public RelationalInheritanceKind? InheritanceKind { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(IMetadataElementVisitor visitor)
        {
            PrimaryTableName = visitor.VisitAttribute("PrimaryTableName", PrimaryTableName);
            InheritanceKind = visitor.VisitAttribute("InheritanceKind", InheritanceKind);
        }
    }
}

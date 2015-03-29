using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NWheels.DataObjects;
using System.ComponentModel.DataAnnotations;

namespace NWheels.Core.DataObjects
{
    public class PropertyValidationMetadataBuilder : MetadataElement<IPropertyValidationMetadata>, IPropertyValidationMetadata
    {
        public PropertyValidationMetadataBuilder()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IMetadataElement Members

        public override string ReferenceName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IPropertyValidationMetadata Members

        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string RegularExpression { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(IMetadataElementVisitor visitor)
        {
            IsReadOnly = visitor.VisitAttribute("IsEditable", IsReadOnly);
            MinLength = visitor.VisitAttribute("MinLength", MinLength);
            MaxLength = visitor.VisitAttribute("MaxLength", MaxLength);
            MinValue = visitor.VisitAttribute("MinValue", MinValue);
            MaxValue = visitor.VisitAttribute("MaxValue", MaxValue);
            RegularExpression = visitor.VisitAttribute("RegularExpression", RegularExpression);
        }
    }
}

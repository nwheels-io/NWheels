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
            this.ValidationAttributes = new List<ValidationAttribute>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IPropertyValidationMetadata Members

        public DataType SemanticDataType { get; set; }
        public bool IsRequired { get; set; }
        public bool IsEditable { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string RegularExpression { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<ValidationAttribute> IPropertyValidationMetadata.ValidationAttributes
        {
            get
            {
                return this.ValidationAttributes;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<ValidationAttribute> ValidationAttributes { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(IMetadataElementVisitor visitor)
        {
            SemanticDataType = visitor.VisitAttribute("SemanticDataType", SemanticDataType);
            IsEditable = visitor.VisitAttribute("IsEditable", IsEditable);
            MinLength = visitor.VisitAttribute("MinLength", MinLength);
            MaxLength = visitor.VisitAttribute("MaxLength", MaxLength);
            MinValue = visitor.VisitAttribute("MinValue", MinValue);
            MaxValue = visitor.VisitAttribute("MaxValue", MaxValue);
            RegularExpression = visitor.VisitAttribute("RegularExpression", RegularExpression);

            //TODO: visit ValidationAttributes
        }
    }
}

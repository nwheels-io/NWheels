using System;

namespace NWheels.DataObjects.Core
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
        public bool IsUnique { get; set; }
        public bool IsEmptyAllowed { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public bool MinValueExclusive { get; set; }
        public bool MaxValueExclusive { get; set; }
        public string RegularExpression { get; set; }
        public Type AncestorClrType { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void MergeWith(IPropertyValidationMetadata other)
        {
            this.IsRequired |= other.IsRequired;
            this.IsUnique |= other.IsUnique;
            this.IsEmptyAllowed &= other.IsEmptyAllowed;

            if ( this.MinLength.HasValue )
            {
                this.MinLength = Math.Max(this.MinLength.Value, other.MinLength.GetValueOrDefault(0));
            }
            else
            {
                this.MinLength = other.MinLength;
            }

            if ( this.MaxLength.HasValue )
            {
                this.MaxLength = Math.Min(this.MaxLength.Value, other.MaxLength.GetValueOrDefault(Int32.MaxValue));
            }
            else
            {
                this.MaxLength = other.MaxLength;
            }

            if ( this.MinValue == null )
            {
                this.MinValue = other.MinValue;
            }

            if ( this.MaxValue == null )
            {
                this.MaxValue = other.MaxValue;
            }

            this.MinValueExclusive |= other.MinValueExclusive;
            this.MaxValueExclusive |= other.MaxValueExclusive;
            this.RegularExpression = this.RegularExpression ?? other.RegularExpression;
            this.AncestorClrType = this.AncestorClrType ?? other.AncestorClrType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(ITypeMetadataVisitor visitor)
        {
            IsRequired = visitor.VisitAttribute("IsRequired", IsRequired);
            IsUnique = visitor.VisitAttribute("IsUnique", IsUnique);
            IsEmptyAllowed = visitor.VisitAttribute("IsEmptyAllowed", IsEmptyAllowed);
            MinLength = visitor.VisitAttribute("MinLength", MinLength);
            MaxLength = visitor.VisitAttribute("MaxLength", MaxLength);
            MinValue = visitor.VisitAttribute("MinValue", MinValue);
            MaxValue = visitor.VisitAttribute("MaxValue", MaxValue);
            RegularExpression = visitor.VisitAttribute("RegularExpression", RegularExpression);
        }
    }
}

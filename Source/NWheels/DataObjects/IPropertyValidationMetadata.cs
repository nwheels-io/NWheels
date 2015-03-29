using System.Collections.Generic;

namespace NWheels.DataObjects
{
    public interface IPropertyValidationMetadata : IMetadataElement
    {
        bool IsRequired { get; }
        bool IsReadOnly { get; }
        int? MinLength { get; }
        int? MaxLength { get; }
        decimal? MinValue { get; }
        decimal? MaxValue { get; }
        string RegularExpression { get; }
    }
}

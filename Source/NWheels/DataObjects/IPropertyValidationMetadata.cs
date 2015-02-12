using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWheels.DataObjects
{
    public interface IPropertyValidationMetadata
    {
        IReadOnlyList<ValidationAttribute> ValidationAttributes { get; }
        DataType SemanticDataType { get; }
        bool IsRequired { get; }
        bool IsEditable { get; }
        int? MinLength { get; }
        int? MaxLength { get; }
        decimal? MinValue { get; }
        decimal? MaxValue { get; }
        string RegularExpression { get; }
    }
}

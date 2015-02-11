using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Metadata
{
    public interface IEntityPropertyValidationMetadata
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

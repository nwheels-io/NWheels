using System;
using System.Collections.Generic;

namespace NWheels.DataObjects
{
    public interface IPropertyValidationMetadata : IMetadataElement
    {
        bool IsRequired { get; }
        bool IsUnique { get; }
        bool IsEmptyAllowed { get; }
        int? MinLength { get; }
        int? MaxLength { get; }
        object MinValue { get; }
        object MaxValue { get; }
        bool MinValueExclusive { get; }
        bool MaxValueExclusive { get; }
        string RegularExpression { get; }
        Type AncestorClrType { get; }
    }
}

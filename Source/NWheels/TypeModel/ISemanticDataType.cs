using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.TypeModel;

namespace NWheels.DataObjects
{
    public interface ISemanticDataType
    {
        System.ComponentModel.DataAnnotations.DataType GetDataTypeAnnotation();
        IPropertyValidationMetadata GetDefaultValidation();
        PropertyAccess? GetDefaultPropertyAccess(); 
        string Name { get; }
        Type ClrType { get; }
        WellKnownSemanticType WellKnownSemantic { get; }
        object DefaultValue { get; }
        string DefaultDisplayName { get; }
        string DefaultDisplayFormat { get; }
        bool? DefaultSortAscending { get; }
        TimeUnits? TimeUnits { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISemanticDataType<T>
    {
        bool IsValid(T value);
        T DefaultValue { get; }
    }
}

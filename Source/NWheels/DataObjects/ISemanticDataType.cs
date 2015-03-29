using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface ISemanticDataType
    {
        System.ComponentModel.DataAnnotations.DataType GetDataTypeAnnotation();
        IPropertyValidationMetadata GetDefaultValidation();
        string Name { get; }
        Type ClrType { get; }
        object DefaultValue { get; }
        string DefaultDisplayName { get; }
        string DefaultDisplayFormat { get; }
        bool? DefaultSortAscending { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISemanticDataType<T>
    {
        bool IsValid(T value);
        T DefaultValue { get; }
    }
}

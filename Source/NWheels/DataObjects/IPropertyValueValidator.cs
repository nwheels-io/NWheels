using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface IPropertyValueValidator
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPropertyValueValidator<T> : IPropertyValueValidator
    {
    }
}

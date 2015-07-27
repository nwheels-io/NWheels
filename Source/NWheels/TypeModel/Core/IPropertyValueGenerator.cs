using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil.Writers;

namespace NWheels.DataObjects.Core
{
    /// <summary>
    /// Defines a strategy for automatically generating (usually initial) value for a property
    /// </summary>
    /// <remarks>
    /// The client of this interface is property metadata object, for which the derived generic interface is not known.
    /// </remarks>
    public interface IPropertyValueGenerator
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Defines a strategy for automatically generating (usually initial) value for a property
    /// </summary>
    /// <remarks>
    /// The client of this interface is entity object that initializes its properties.
    /// </remarks>
    public interface IPropertyValueGenerator<T> : IPropertyValueGenerator
    {
        /// <summary>
        /// Generates a new value.
        /// </summary>
        /// <returns>
        /// The newly generated value.
        /// </returns>
        T GenerateValue(string propertyQualifiedName);
    }
}

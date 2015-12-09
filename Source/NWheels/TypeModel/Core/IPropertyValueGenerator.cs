using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil.Operands;
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

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// When this interface is implemented in addition to IPropertyValueGenerator, the value generation code in inlined in entity constructor for 
    /// better performance, because this avoids resolution of generator object from component container and calling its GenerateValue method.
    /// </summary>
    public interface IPropertyValueGeneratorWriter
    {
        /// <summary>
        /// Writes code that generates a value and assigns it to specified destination operand.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value to generate (can be a template type TT.*).
        /// </typeparam>
        /// <param name="propertyQualifiedName">
        /// Qualified name of property whose value is being generated (in the form 'EntityType.PropertyName').
        /// </param>
        /// <param name="method">
        /// Method writer object for method where the value generation code is inlined.
        /// </param>
        /// <param name="destination">
        /// The operand to assign the generated value to.
        /// </param>
        void WriteGenerateValue<T>(string propertyQualifiedName, MethodWriterBase method, MutableOperand<T> destination);
    }
}

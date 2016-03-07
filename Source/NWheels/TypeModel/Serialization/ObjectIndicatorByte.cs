using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Serialization
{
    public static class ObjectIndicatorByte
    {
        /// <summary>
        /// designates null reference, indicator byte is the only data serialized for the object
        /// </summary>
        public const byte Null = 0;
        /// <summary>
        /// indicator byte is followed by the contents of the object
        /// </summary>
        public const byte NotNull = 1;
        /// <summary>
        /// indicator byte is followed by 16-bit integer which deisgnates serialized type, as registered by ObjectCompactSerializerDictionary, then by the contents of the object
        /// </summary>
        public const byte NotNullWithTypeKey = 2;
        /// <summary>
        /// indicator byte is followed by 32-bit integer which designates reference to serialized object - reserved for circular references support
        /// </summary>
        public const byte NotNullWithObjectId = 3;
    }
}

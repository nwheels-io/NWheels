using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Serialization
{
    public class CompactDeserializationContext
    {
        public CompactDeserializationContext(ObjectCompactSerializer serializer, ObjectCompactSerializerDictionary dictionary, CompactBinaryReader input)
        {
            Serializer = serializer;
            Dictionary = dictionary;
            Input = input;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ReadObject(Type declaredType)
        {
            return Serializer.ReadObject(declaredType, this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectCompactSerializer Serializer { get; private set; }
        public ObjectCompactSerializerDictionary Dictionary { get; private set; }
        public CompactBinaryReader Input { get; private set; }
    }
}

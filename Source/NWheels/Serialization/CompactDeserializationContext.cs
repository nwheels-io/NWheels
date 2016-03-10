using System;

namespace NWheels.Serialization
{
    public class CompactDeserializationContext
    {
        public CompactDeserializationContext(CompactSerializer serializer, CompactSerializerDictionary dictionary, CompactBinaryReader input)
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

        public CompactSerializer Serializer { get; private set; }
        public CompactSerializerDictionary Dictionary { get; private set; }
        public CompactBinaryReader Input { get; private set; }
    }
}

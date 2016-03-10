using System;

namespace NWheels.Serialization
{
    public class CompactSerializationContext
    {
        public CompactSerializationContext(CompactSerializer serializer, CompactSerializerDictionary dictionary, CompactBinaryWriter output)
        {
            Serializer = serializer;
            Dictionary = dictionary;
            Output = output;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObject(Type declaredType, object obj)
        {
            Serializer.WriteObject(declaredType, obj, this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteStruct<T>(T value) where T : struct
        {
            Serializer.WriteStruct<T>(ref value, this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactSerializer Serializer { get; private set; }
        public CompactSerializerDictionary Dictionary { get; private set; }
        public CompactBinaryWriter Output { get; private set; }
    }
}

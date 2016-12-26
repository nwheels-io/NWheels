using System;
using Autofac;

namespace NWheels.Serialization
{
    public class CompactDeserializationContext
    {
        public CompactDeserializationContext(
            CompactSerializer serializer, 
            CompactSerializerDictionary dictionary, 
            CompactBinaryReader input, 
            IComponentContext components)
        {
            this.Serializer = serializer;
            this.Dictionary = dictionary;
            this.Input = input;
            this.Components = components;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ReadObject(Type declaredType)
        {
            return Serializer.ReadObject(declaredType, this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T ReadStruct<T>() where T : struct
        {
            return Serializer.ReadStruct<T>(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T? ReadNullable<T>() where T : struct
        {
            return Serializer.ReadNullable<T>(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactSerializer Serializer { get; private set; }
        public CompactSerializerDictionary Dictionary { get; private set; }
        public CompactBinaryReader Input { get; private set; }
        public IComponentContext Components { get; private set; }
    }
}

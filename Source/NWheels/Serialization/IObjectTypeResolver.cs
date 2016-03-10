using System;

namespace NWheels.Serialization
{
    public interface IObjectTypeResolver
    {
        Type GetSerializationType(Type declaredType, object obj);
        Type GetDeserializationType(Type declaredType, Type serializedType);
        bool CanMaterialize(Type declaredType, Type serializedType);
        object Materialize(Type declaredType, Type serializedType);
    }
}

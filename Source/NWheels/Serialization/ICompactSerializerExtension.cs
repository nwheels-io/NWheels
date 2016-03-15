using System;

namespace NWheels.Serialization
{
    public interface ICompactSerializerExtension
    {
        Type GetSerializationType(Type declaredType, object obj);
        Type GetMaterializationType(Type declaredType, Type serializedType);
        bool CanMaterialize(Type declaredType, Type serializedType);
        object Materialize(Type declaredType, Type serializedType);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Serialization
{
    public interface IObjectTypeResolver
    {
        Type GetSerializationType(Type declaredType, object obj);
        Type GetDeserializationType(Type declaredType, Type serializedType);
        bool CanMaterialize(Type declaredType, Type serializedType);
        object Materialize(Type declaredType, Type serializedType);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Serialization
{
    public abstract class CompactSerializerExtensionBase : ICompactSerializerExtension
    {
        #region Implementation of ICompactSerializerExtension

        public virtual Type GetSerializationType(Type declaredType, object obj)
        {
            return obj.GetType();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Type GetMaterializationType(Type declaredType, Type serializedType)
        {
            return serializedType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool CanMaterialize(Type declaredType, Type serializedType)
        {
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual object Materialize(Type declaredType, Type serializedType)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}

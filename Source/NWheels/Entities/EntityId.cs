using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Core;
using NWheels.Extensions;

namespace NWheels.Entities
{
    public static class EntityId
    {
        public static IEntityId Of(object entity)
        {
            return GetValidatedEntityObject(entity).GetId();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object ValueOf(object entity)
        {
            return GetValidatedEntityObject(entity).GetId().Value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T GetValue<T>(object entity)
        {
            return GetValidatedEntityObject(entity).GetId().ValueAs<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IEntityObject GetValidatedEntityObject(object obj)
        {
            var entityObject = obj.AsOrNull<IEntityObject>();

            if ( entityObject == null )
            {
                entityObject = (IEntityObject)obj.AsOrNull<IPersistableObject>();
            }

            if ( entityObject == null )
            {
                throw new ArgumentException("Not an entity object", "entity");
            }

            return entityObject;
        }
    }
}

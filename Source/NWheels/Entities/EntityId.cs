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
            return GetValidatedDomainObject(entity).GetId();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object ValueOf(object entity)
        {
            return GetValidatedDomainObject(entity).GetId().Value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T GetValue<T>(object entity)
        {
            return GetValidatedDomainObject(entity).GetId().ValueAs<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IDomainObject GetValidatedDomainObject(object obj)
        {
            var domainObject = obj.AsOrNull<IDomainObject>();

            if ( domainObject == null )
            {
                throw new ArgumentException("Not an entity object", "obj");
            }

            return domainObject;
        }
    }
}

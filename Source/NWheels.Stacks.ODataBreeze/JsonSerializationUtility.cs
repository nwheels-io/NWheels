using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    public static class JsonSerializationUtility
    {
        public static DataRepositoryBase GetCurrentDomainContext()
        {
            var domainContext = RuntimeEntityModelHelpers.CurrentDomainContext;

            if ( domainContext == null )
            {
                throw new InvalidOperationException("No domain context on current thread.");
            }

            return domainContext;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetEntityContractType(Type type)
        {
            if ( type.IsEntityContract() )
            {
                return type;
            }
            else
            {
                return type.GetInterfaces().First(intf => intf.IsEntityContract());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEntityId ParseEntityId(ITypeMetadata metaType, IPropertyMetadata idProperty, string entityIdString)
        {
            var parseMethod = idProperty.ClrType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
            object parsedIdValue = parseMethod.Invoke(null, new object[] { entityIdString });

            var closedEntityIdType = typeof(EntityId<,>).MakeGenericType(metaType.ContractType, idProperty.ClrType);
            var entityIdInstance = (IEntityId)Activator.CreateInstance(closedEntityIdType, parsedIdValue);
            return entityIdInstance;
        }
    }
}

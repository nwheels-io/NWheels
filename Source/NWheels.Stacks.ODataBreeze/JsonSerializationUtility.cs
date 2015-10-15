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

        public static Type TryGetEntityContractType(Type type)
        {
            if ( type.IsEntityContract() )
            {
                return type;
            }
            else if ( !type.IsValueType )
            {
                return type.GetInterfaces().FirstOrDefault(intf => intf.IsEntityContract());
            }
            else
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetEntityContractType(Type type)
        {
            var contractType = TryGetEntityContractType(type);

            if ( contractType == null )
            {
                throw new ArgumentException("Cannot determine entity contract for type: " + type.FullName, "type");
            }

            return contractType;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetForeignKeyPropertyName(string entityPropertyName)
        {
            return entityPropertyName + "_FK";
        }
    }
}

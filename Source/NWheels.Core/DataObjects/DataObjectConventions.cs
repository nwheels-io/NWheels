using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.DataObjects;
using NWheels.Extensions;

namespace NWheels.Core.DataObjects
{
    public class DataObjectConventions
    {
        public bool IsDataObjectContract(Type type)
        {
            return (type.IsInterface && type.HasAttribute<DataObjectContractAttribute>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsToOneRelationProperty(IPropertyMetadata property)
        {
            return IsDataObjectContract(property.ClrType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsKeyProperty(IPropertyMetadata property)
        {
            return (property.Name.EqualsIgnoreCase("Id") || property.HasContractAttribute<PropertyContract.KeyAttribute>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsToManyRelationProperty(IPropertyMetadata property, out Type relatedContract)
        {
            Type elementType;
            var isCollection = property.ClrType.IsCollectionType(out elementType);

            if ( isCollection && IsDataObjectContract(elementType) )
            {
                relatedContract = elementType;
                return true;
            }
            else
            {
                relatedContract = null;
                return false;
            }
        }
    }
}

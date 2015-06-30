using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Conventions;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;

namespace NWheels.Entities
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class EntityContractAttribute : DataObjectContractAttribute
    {
        #region Overrides of DataObjectContractAttribute

        public override void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache)
        {
            base.ApplyTo(type, cache);

            type.IsAbstract = IsAbstract;

            if ( BaseEntity != null )
            {
                type.BaseType = cache.FindTypeMetadataAllowIncomplete(BaseEntity);
            }

            type.Name = type.Name.TrimSuffix("Entity");
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAbstract { get; set; }
        public Type BaseEntity { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsEntityContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<EntityContractAttribute>() != null);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AggregationEntityContractAttribute : DataObjectContractAttribute
    {
        #region Overrides of DataObjectContractAttribute

        public override void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache)
        {
            base.ApplyTo(type, cache);
        }

        #endregion
    }
}

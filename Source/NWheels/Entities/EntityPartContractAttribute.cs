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
using NWheels.Extensions;

namespace NWheels.Entities
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class EntityPartContractAttribute : DataObjectPartContractAttribute
    {
        #region Overrides of DataObjectPartContractAttribute

        public override void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache)
        {
            base.ApplyTo(type, cache);

            type.IsEntityPart = true;
            type.IsAbstract = this.IsAbstract;

            if ( BaseEntityPart != null )
            {
                type.BaseType = cache.FindTypeMetadataAllowIncomplete(BaseEntityPart);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsEntityPartContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<EntityPartContractAttribute>() != null);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type BaseEntityPart { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAbstract { get; set; }
    }
}

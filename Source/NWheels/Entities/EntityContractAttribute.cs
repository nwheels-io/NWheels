using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions;
using NWheels.DataObjects;

namespace NWheels.Entities
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class EntityContractAttribute : DataObjectContractAttribute
    {
        public bool IsAbstract { get; set; }
        public Type BaseEntity { get; set; }
        public bool IsAggregation { get; set; }

        public static bool IsEntityContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<EntityContractAttribute>() != null);
        }
    }
}

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
    public class EntityPartContractAttribute : DataObjectPartContractAttribute
    {
        public static bool IsEntityPartContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<EntityPartContractAttribute>() != null);
        }
    }
}

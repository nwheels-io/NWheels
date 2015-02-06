using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions;

namespace NWheels.Entities
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class EntityContractAttribute : DataTransferObjectAttribute
    {
        public static bool IsEntityContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<EntityContractAttribute>() != null);
        }
    }
}

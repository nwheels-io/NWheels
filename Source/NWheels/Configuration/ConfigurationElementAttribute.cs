using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions;
using NWheels.DataObjects;

namespace NWheels.Configuration
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ConfigurationElementAttribute : DataObjectContractAttribute
    {
        public string XmlName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConfigurationElementAttribute From(Type interfaceType)
        {
            return interfaceType.GetCustomAttribute<ConfigurationElementAttribute>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsConfigElement(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<ConfigurationElementAttribute>() != null);
        }
    }
}

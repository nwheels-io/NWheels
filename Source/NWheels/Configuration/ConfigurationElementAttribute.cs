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
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ConfigurationElementAttribute : DataObjectContractAttribute
    {
        public string XmlName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAbstract { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConfigurationElementAttribute From(Type interfaceType)
        {
            return interfaceType.GetCustomAttribute<ConfigurationElementAttribute>(inherit: true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsConfigElement(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<ConfigurationElementAttribute>() != null);
        }
    }
}

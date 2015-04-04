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
    public class ConfigurationSectionAttribute : ConfigurationElementAttribute
    {
        public static new ConfigurationSectionAttribute From(Type interfaceType)
        {
            return interfaceType.GetCustomAttribute<ConfigurationSectionAttribute>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsConfigSection(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<ConfigurationSectionAttribute>() != null);
        }
    }
}

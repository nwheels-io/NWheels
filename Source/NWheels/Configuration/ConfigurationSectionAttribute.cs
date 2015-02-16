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
    public class ConfigurationSectionAttribute : DataTransferObjectAttribute
    {
        public string XmlName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConfigurationSectionAttribute From(Type interfaceType)
        {
            return interfaceType.GetCustomAttribute<ConfigurationSectionAttribute>();
        }
    }
}

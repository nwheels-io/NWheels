using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions;

namespace NWheels.Configuration
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ConfigurationSectionAttribute : DataTransferObjectAttribute
    {
        public string XmlName { get; set; }
    }
}

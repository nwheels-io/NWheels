using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Configuration
{
    [ConfigurationElement]
    public interface INamedConfigurationElement : IConfigurationElement
    {
        [PropertyContract.Required(AllowEmpty = true)]
        string Name { get; set; }
    }
}

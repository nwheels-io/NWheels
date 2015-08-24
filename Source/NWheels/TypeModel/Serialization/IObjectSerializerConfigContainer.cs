using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;

namespace NWheels.TypeModel.Serialization
{
    public interface IObjectSerializerConfigContainer : IConfigurationElement
    {
        IAbstractObjectSerializerConfig ConcreteConfig { get; set; }
    }

    [ConfigurationElement(IsAbstract = true)]
    public interface IAbstractObjectSerializerConfig : IConfigurationElement
    {
        IObjectSerializer CreateConfiguredComponent();
    }
}

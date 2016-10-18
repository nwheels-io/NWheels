using NWheels.Configuration;

namespace NWheels.Serialization
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

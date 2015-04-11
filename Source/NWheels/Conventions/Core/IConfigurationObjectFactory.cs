using NWheels.Configuration.Core;

namespace NWheels.Conventions.Core
{
    public interface IConfigurationObjectFactory
    {
        TElement CreateConfigurationElement<TElement>(ConfigurationElementBase parent, string xmlElementName);
    }
}
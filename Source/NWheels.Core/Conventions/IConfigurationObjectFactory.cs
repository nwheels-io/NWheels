using NWheels.Configuration;
using NWheels.Core.Configuration;

namespace NWheels.Core.Conventions
{
    public interface IConfigurationObjectFactory
    {
        TElement CreateConfigurationElement<TElement>(ConfigurationElementBase parent, string xmlElementName);
    }
}
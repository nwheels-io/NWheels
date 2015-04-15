using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;
using NWheels.DataObjects;

namespace NWheels.Endpoints
{
    [ConfigurationSection(XmlName = "Framework.Endpoints")]
    public interface IFrameworkEndpointsConfig : IConfigurationSection
    {
        INamedObjectCollection<IEndpointConfig> Endpoints { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ConfigurationElement(XmlName = "Endpoint")]
    public interface IEndpointConfig : INamedConfigurationElement
    {
        [PropertyContract.Required, PropertyContract.Semantic.Url]
        string Address { get; set; }

        [PropertyContract.DefaultValue(true)]
        bool PuiblishMetadata { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.Url]
        string MetadataAddress { get; set; }

        [PropertyContract.DefaultValue(false)]
        bool ExposeExceptionDetails { get; set; }
    }
}

using NWheels.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.UI
{
    [ConfigurationSection(XmlName = "Framework.UI")]
    public interface IFrameworkUIConfig : IConfigurationSection
    {
        string WebContentRootPath { get; set; }

        [PropertyContract.DefaultValue(true)]
        bool EnableContentBundling { get; set; }

        [PropertyContract.DefaultValue(true)]
        bool EnableContentMinification { get; set; }

        [PropertyContract.DefaultValue(true)]
        bool EnableContentCompression { get; set; }
    }
}

using NWheels.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    [ConfigurationSection(XmlName = "Framework.UI")]
    public interface IFrameworkUIConfig : IConfigurationSection
    {
        string WebContentRootPath { get; set; }
    }
}

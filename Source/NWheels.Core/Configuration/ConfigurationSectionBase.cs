using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hapil;
using NWheels.Configuration;
using NWheels.Core.Conventions;
using NWheels.Utilities;

namespace NWheels.Core.Configuration
{
    public abstract class ConfigurationSectionBase : ConfigurationElementBase, IConfigurationSection
    {
        protected ConfigurationSectionBase(IConfigurationObjectFactory factory, Auto<IConfigurationLogger> logger, string configPath)
            : base(factory, logger, configPath)
        {
        }
    }
}

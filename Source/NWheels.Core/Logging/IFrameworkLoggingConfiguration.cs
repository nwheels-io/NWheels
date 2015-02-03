using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;
using System.ComponentModel;
using NWheels.Logging;

namespace NWheels.Core.Logging
{
    [ConfigurationSection(XmlName = "Framework.Logging")]
    public interface IFrameworkLoggingConfiguration : IConfigurationSection
    {
        [DefaultValue("")]
        string PlainLogFolder { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue("Logs")]
        string ThreadLogFolder { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue(LogLevel.Info)]
        LogLevel Level { get; }
    }
}

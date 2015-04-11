using System.ComponentModel;
using NWheels.Configuration;

namespace NWheels.Logging.Core
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

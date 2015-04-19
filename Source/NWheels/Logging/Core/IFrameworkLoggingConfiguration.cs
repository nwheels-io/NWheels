using System.ComponentModel;
using NWheels.Configuration;

namespace NWheels.Logging.Core
{
    [ConfigurationSection(XmlName = "Framework.Logging")]
    public interface IFrameworkLoggingConfiguration : IConfigurationSection
    {
        [DefaultValue("..\\Logs\\PlainLog")]
        string PlainLogFolder { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue("..\\Logs\\ThreadLog")]
        string ThreadLogFolder { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue(false)]
        bool SuppressDynamicArtifacts { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue(LogLevel.Info)]
        LogLevel Level { get; }
    }
}

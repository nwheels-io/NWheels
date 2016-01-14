using System.ComponentModel;
using NWheels.Configuration;

namespace NWheels.Logging.Core
{
    [ConfigurationSection(XmlName = "Framework.Logging")]
    public interface IFrameworkLoggingConfiguration : IConfigurationSection
    {
        [DefaultValue("..\\Logs\\PlainLog")]
        string PlainLogFolder { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue("..\\Logs\\ThreadLog")]
        string ThreadLogFolder { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue(LogLevel.Info)]
        LogLevel Level { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue(ThreadLogPersistenceLevel.StartupShutdownErrors)]
        ThreadLogPersistenceLevel ThreadLogPersistenceLevel { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string ThreadLogDbConnectionString { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultValue(true)]
        bool SuppressDynamicArtifacts { get; set; }
    }
}

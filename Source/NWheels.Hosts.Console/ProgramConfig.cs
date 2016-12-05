using System.Collections.Generic;
using NWheels.Hosting;
using NWheels.Logging;
using Topshelf.Runtime;

namespace NWheels.Hosts.Console
{
    public class ProgramConfig
    {
        public ProgramConfig()
        {
            LogLevel = NWheels.Logging.LogLevel.Info;
            CommandLineConfigValues = new HashSet<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsBatchJob { get; set; }
        public string BootConfigFilePath { get; set; }
        public NWheels.Logging.LogLevel LogLevel { get; set; }
        public BootConfiguration BootConfig { get; set; }
        public HostSettings HostSettings { get; set; }
        public HashSet<string> CommandLineConfigValues { get; set; }
    }
}

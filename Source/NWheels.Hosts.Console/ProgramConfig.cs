using System.Collections.Generic;
using NWheels.Hosting;
using Topshelf.Runtime;

namespace NWheels.Hosts.Console
{
    public class ProgramConfig
    {
        public ProgramConfig()
        {
            CommandLineConfigValues = new HashSet<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsBatchJob { get; set; }
        public string BootConfigFilePath { get; set; }
        public BootConfiguration BootConfig { get; set; }
        public HostSettings HostSettings { get; set; }
        public HashSet<string> CommandLineConfigValues { get; set; }
    }
}

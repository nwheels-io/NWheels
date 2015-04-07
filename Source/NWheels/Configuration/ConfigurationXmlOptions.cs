using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Configuration
{
    public class ConfigurationXmlOptions
    {
        public ConfigurationSourceLevel? OverrideLevel { get; set; }
        public bool IncludeOverrideHistory { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly ConfigurationXmlOptions _s_default = new ConfigurationXmlOptions() {
            OverrideLevel = null,
            IncludeOverrideHistory = false
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConfigurationXmlOptions Default
        {
            get { return _s_default; }
        }
    }
}

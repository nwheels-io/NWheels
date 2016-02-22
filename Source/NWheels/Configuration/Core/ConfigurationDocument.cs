using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NWheels.Configuration.Core
{
    public class ConfigurationDocument
    {
        public ConfigurationDocument(XDocument xml, ConfigurationSourceInfo sourceInfo)
        {
            this.Xml = xml;
            this.SourceInfo = sourceInfo;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public XDocument Xml { get; private set; }
        public ConfigurationSourceInfo SourceInfo { get; private set; }
    }
}

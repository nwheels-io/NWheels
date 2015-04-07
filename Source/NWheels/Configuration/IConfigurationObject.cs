using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NWheels.Configuration
{
    public interface IConfigurationObject
    {
        string GetXmlName();
        string GetConfigPath();
        IOverrideHistory GetOverrideHistory();
        void SaveXml(XElement xml, ConfigurationXmlOptions options);
    }
}

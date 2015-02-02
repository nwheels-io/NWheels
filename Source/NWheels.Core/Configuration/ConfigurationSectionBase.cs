using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hapil;
using NWheels.Utilities;

namespace NWheels.Core.Configuration
{
    public abstract class ConfigurationSectionBase
    {
        private readonly IConfigurationLogger _logger;
        private readonly string _configPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ConfigurationSectionBase(Auto<IConfigurationLogger> logger, string configPath)
        {
            _configPath = configPath + "/" + GetXmlName();
            _logger = logger.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadObject(XElement xml)
        {
            var element = xml.Element(GetXmlName());

            if ( element != null )
            {
                LoadProperties(element);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void LoadProperties(XElement xml);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string GetXmlName();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void TryReadScalarValue<T>(XElement xml, string propertyName, ref T value)
        {
            var attributeName = propertyName.ToCamelCase();
            var attribute = xml.Attribute(attributeName);

            if ( attribute != null )
            {
                try
                {
                    value = ParseUtility.Parse<T>(attribute.Value);
                }
                catch ( Exception e )
                {
                    _logger.BadPropertyValue(_configPath, propertyName, e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ConfigPath
        {
            get { return _configPath; }
        }
    }
}

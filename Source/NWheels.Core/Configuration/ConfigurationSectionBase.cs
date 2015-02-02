using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hapil;
using NWheels.Configuration;
using NWheels.Utilities;

namespace NWheels.Core.Configuration
{
    public abstract class ConfigurationSectionBase : IConfigurationObject, IConfigurationSection, IInternalConfigurationObject
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
            LoadProperties(xml);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string GetXmlName();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetConfigPath()
        {
            return _configPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IConfigurationObject AsConfigurationObject()
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void LoadProperties(XElement xml);

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
    }
}

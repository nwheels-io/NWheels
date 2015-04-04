using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hapil;
using NWheels.Configuration;
using NWheels.Core.Conventions;
using NWheels.Utilities;

namespace NWheels.Core.Configuration
{
    public abstract class ConfigurationElementBase : IConfigurationObject, IConfigurationElement, IInternalConfigurationObject
    {
        private readonly IConfigurationObjectFactory _factory;
        private readonly IConfigurationLogger _logger;
        private readonly string _configPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ConfigurationElementBase(IConfigurationObjectFactory factory, Auto<IConfigurationLogger> logger, string configPath)
        {
            _factory = factory;
            _configPath = configPath + "/" + GetXmlName();
            _logger = logger.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ConfigurationElementBase(ConfigurationElementBase parentElement)
        {
            _factory = parentElement.Factory;
            _configPath = parentElement.ConfigPath + "/" + GetXmlName();
            _logger = parentElement.Logger;
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

        public void ReadScalarValue<T>(XElement xml, string propertyName, ref T value)
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

        public void ReadNestedElement(XElement xml, string propertyName, IConfigurationElement element)
        {
            var elementName = propertyName.ToPascalCase();
            var elementXml = xml.Element(elementName);

            if ( elementXml != null )
            {
                ((IInternalConfigurationObject)element).LoadObject(elementXml);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReadNestedElementCollection<T>(XElement xml, string propertyName, ICollection<T> collection)
        {
            var collectionElementName = propertyName.ToPascalCase();
            var collectionElementXml = xml.Element(collectionElementName);

            if ( collectionElementXml != null )
            {
                foreach ( var collectionItemXml in collectionElementXml.Elements() )
                {
                    var itemElement = _factory.CreateConfigurationElement<T>(this, collectionItemXml.Name.LocalName);
                    ((IInternalConfigurationObject)itemElement).LoadObject(collectionItemXml);
                    collection.Add(itemElement);

                    //TODO: add support for location modifiers (top/bottom/midde) + collection modifiers (clear all/remove specific item(s))
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IConfigurationObjectFactory Factory
        {
            get
            {
                return _factory;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IConfigurationLogger Logger
        {
            get
            {
                return _logger;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string ConfigPath
        {
            get
            {
                return _configPath;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Hapil;
using NWheels.Conventions.Core;
using NWheels.Extensions;
using NWheels.Utilities;

namespace NWheels.Configuration.Core
{
    public abstract class ConfigurationElementBase : IConfigurationObject, IConfigurationElement, IInternalConfigurationObject
    {
        private readonly IConfigurationObjectFactory _factory;
        private readonly IConfigurationLogger _logger;
        private readonly string _configPath;
        private readonly OverrideHistory _overrideHistory = new OverrideHistory();
        private ConfigurationXmlOptions _currentSaveOptions;

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
            _overrideHistory.PushElementOverride(xml);
            LoadProperties(xml);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SaveXml(XElement xml, ConfigurationXmlOptions options)
        {
            _currentSaveOptions = options ?? ConfigurationXmlOptions.Default;

            try
            {
                if ( _currentSaveOptions.IncludeOverrideHistory )
                {
                    WriteOverrideHistoryXml(xml, string.Empty, _overrideHistory);
                }

                SaveProperties(xml);
            }
            finally
            {
                _currentSaveOptions = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IOverrideHistory GetOverrideHistory()
        {
            return _overrideHistory;
        }

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

        public abstract string GetXmlName();
        public abstract void LoadProperties(XElement xml);
        public abstract void SaveProperties(XElement xml);

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
                    _overrideHistory.PushPropertyOverride(propertyName, attribute.Value, lineInfo: xml);
                }
                catch ( Exception e )
                {
                    _logger.BadPropertyValue(_configPath, propertyName, e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteScalarValue<T>(XElement xml, string propertyName, ref T value)
        {
            if ( ShouldWriteToXml(_overrideHistory[propertyName]) && !object.ReferenceEquals(null, value) )
            {
                if ( _currentSaveOptions.IncludeOverrideHistory )
                {
                    WriteOverrideHistoryXml(xml, propertyName, _overrideHistory[propertyName]);
                }

                xml.SetAttributeValue(propertyName.ToCamelCase(), value.ToString());
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

        public void WriteNestedElement(XElement xml, string propertyName, IConfigurationElement element)
        {
            if ( ShouldWriteToXml(element.AsConfigurationObject().GetOverrideHistory()) )
            {
                var nestedElement = new XElement(propertyName.ToPascalCase());
                xml.Add(nestedElement);
                element.AsConfigurationObject().SaveXml(nestedElement, _currentSaveOptions);
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

        public void WriteNestedElementCollection<T>(XElement xml, string propertyName, ICollection<T> collection)
        {
            var itemsToWrite = collection.Cast<IConfigurationObject>().Where(obj => ShouldWriteToXml(obj.GetOverrideHistory())).ToArray();

            if ( itemsToWrite.Length > 0 )
            {
                var collectionElement = new XElement(propertyName.ToPascalCase());

                foreach ( var item in itemsToWrite )
                {
                    var itemElement = new XElement(item.GetXmlName());
                    collectionElement.Add(itemElement);
                    item.SaveXml(itemElement, _currentSaveOptions);
                }

                xml.Add(collectionElement);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReadNestedNamedElementCollection<T>(XElement xml, string propertyName, INamedObjectCollection<T> collection)
        {
            var collectionElementName = propertyName.ToPascalCase();
            var collectionElementXml = xml.Element(collectionElementName);

            if ( collectionElementXml != null )
            {
                foreach ( var collectionItemXml in collectionElementXml.Elements() )
                {
                    T itemElement;
                    var name = collectionItemXml.GetAttributeIgnoreCase("name") ?? string.Empty;

                    if ( !collection.TryGetElementByName(name, out itemElement) )
                    {
                        itemElement = _factory.CreateConfigurationElement<T>(this, collectionItemXml.Name.LocalName);
                        collection.Add(itemElement);
                        //TODO: add support for location modifiers (top/bottom/midde) + collection modifiers (clear all/remove specific item(s))
                    }

                    ((IInternalConfigurationObject)itemElement).LoadObject(collectionItemXml);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TElementImpl CreateNestedElement<TElementContract, TElementImpl>()
            where TElementImpl : TElementContract
        {
            return (TElementImpl)_factory.CreateConfigurationElement<TElementContract>(this, xmlElementName: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected void PushPropertyOverrideHistory(string propertyName, object value)
        {
            _overrideHistory.PushPropertyOverride(propertyName, value.ToStringOrDefault(), lineInfo: null);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private bool ShouldWriteToXml(IEnumerable<IOverrideHistoryEntry> overrideHistory)
        {
            return (
                !_currentSaveOptions.OverrideLevel.HasValue ||
                _overrideHistory.Any(h => h.Source.Level >= _currentSaveOptions.OverrideLevel.Value));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteOverrideHistoryXml(XElement xml, string propertyName, IEnumerable<IOverrideHistoryEntry> history)
        {
            foreach ( var entry in history )
            {
                var commentText = string.Format(
                    "{0}{1} {2}", 
                    xml.Name,
                    string.IsNullOrEmpty(propertyName) ? string.Empty : "." + propertyName + " =", 
                    entry.ToString());

                xml.AddBeforeSelf(new XComment(commentText));
            }
        }
    }
}

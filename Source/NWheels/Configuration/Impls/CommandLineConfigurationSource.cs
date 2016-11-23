using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWheels.Configuration.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Extensions;

namespace NWheels.Configuration.Impls
{
    public class CommandLineConfigurationSource : ConfigurationSourceBase
    {
        private readonly string[] _configurationParameters;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CommandLineConfigurationSource(string[] configurationParameters)
            : base("CommandLine", ConfigurationSourceLevel.Deployment)
        {
            _configurationParameters = configurationParameters;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConfigurationSourceBase

        public override IEnumerable<ConfigurationDocument> GetConfigurationDocuments()
        {
            var xml = CreateConfigurationXml();
            var document = new ConfigurationDocument(xml, new ConfigurationSourceInfo(this.SourceLevel, this.SourceType, name: null));

            return new[] { document };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private XDocument CreateConfigurationXml()
        {
            XElement alwaysElement;

            var xml = new XDocument(
                new XElement(XmlConfigurationLoader.ConfigurationElementName,
                    new XAttribute(XmlConfigurationLoader.CommentAttributeName, "Read from command line arguments"),
                    alwaysElement = new XElement(XmlConfigurationLoader.AlwaysElementName,
                        new XAttribute(XmlConfigurationLoader.CommentAttributeName, "Read from command line arguments"))));

            foreach (var arg in _configurationParameters)
            {
                string sectionName;
                string propertyName;
                string value;

                if (IsConfigurationArgument(arg, out sectionName, out propertyName, out value))
                {
                    alwaysElement.Add(new XElement(sectionName, new XAttribute(propertyName, value)));
                }
            }

            return xml;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsConfigurationArgument(string argBody, out string sectionName, out string propertyName, out string value)
        {
            sectionName = null;
            propertyName = null;
            value = null;

            var equalsIndex = argBody.IndexOf('=');

            if (equalsIndex < 1)
            {
                return false;
            }

            var dotNames = argBody.Substring(0, equalsIndex).Split('.');

            if (dotNames.Length < 2)
            {
                return false;
            }

            sectionName = string.Join(".", dotNames.Take(dotNames.Length - 1));
            propertyName = dotNames.Last();
            value = argBody.Substring(equalsIndex + 1);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetCommandLineArgumentString(ITypeMetadataCache metadataCache, IConfigurationSection section)
        {
            var sectionXmlName = section.AsConfigurationObject().GetXmlName();
            var contractType = section.As<IObject>().ContractType;
            var metaType = metadataCache.GetTypeMetadata(contractType);
            var argumentList = new List<string>();

            foreach (var metaProperty in metaType.Properties.Where(p => p.Kind == PropertyKind.Scalar))
            {
                var value = metaProperty.ReadValue(section);

                if (value != null)
                {
                    var argument = string.Format("/config:{0}.{1}={2}", sectionXmlName, metaProperty.Name, value);

                    if (argument.Contains(" "))
                    {
                        argument = "\"" + argument + "\"";
                    }

                    argumentList.Add(argument);
                }
            }

            return string.Join(" ", argumentList);
        }
    }
}

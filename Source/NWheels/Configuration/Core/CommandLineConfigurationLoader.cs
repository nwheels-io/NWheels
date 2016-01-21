using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NWheels.Hosting;

namespace NWheels.Configuration.Core
{
    public class CommandLineConfigurationLoader : LifecycleEventListenerBase
    {
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly XmlConfigurationLoader _loader;
        private readonly string[] _configurationParameters;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CommandLineConfigurationLoader(XmlConfigurationLoader loader, string[] configurationParameters)
        {
            _loader = loader;
            _configurationParameters = configurationParameters;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeConfigured(List<ILifecycleEventListener> additionalComponentsToHost)
        {
            XElement alwaysXml;

            var document = new XDocument(
                new XElement(XmlConfigurationLoader.ConfigurationElementName, 
                    new XAttribute(XmlConfigurationLoader.CommentAttributeName, "Read from command line arguments"),
                    alwaysXml = new XElement(XmlConfigurationLoader.AlwaysElementName, 
                        new XAttribute(XmlConfigurationLoader.CommentAttributeName, "Read from command line arguments"))));

            foreach ( var arg in _configurationParameters )
            {
                string sectionName;
                string propertyName;
                string value;

                if ( IsConfigurationArgument(arg, out sectionName, out propertyName, out value) )
                {
                    alwaysXml.Add(new XElement(sectionName, new XAttribute(propertyName, value)));
                }
            }

            using ( ConfigurationSourceInfo.UseSource(ConfigurationSourceLevel.StartupArgument, string.Empty) )
            {
                _loader.LoadConfigurationDocument(document);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsConfigurationArgument(string argBody, out string sectionName, out string propertyName, out string value)
        {
            sectionName = null;
            propertyName = null;
            value = null;

            var equalsIndex = argBody.IndexOf('=');

            if ( equalsIndex < 1 )
            {
                return false;
            }

            var dotNames = argBody.Substring(0, equalsIndex).Split('.');

            if ( dotNames.Length < 2 )
            {
                return false;
            }

            sectionName = string.Join(".", dotNames.Take(dotNames.Length - 1));
            propertyName = dotNames.Last();
            value = argBody.Substring(equalsIndex + 1);

            return true;
        }
    }
}

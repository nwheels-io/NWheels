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

        public CommandLineConfigurationLoader(XmlConfigurationLoader loader, CommandLineParameters commandLine)
        {
            _loader = loader;
            _configurationParameters = commandLine.Parameters;
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

            using ( ConfigurationSourceInfo.UseSource(ConfigurationSourceLevel.Deployment, "CommandLine", name: null) )
            {
                _loader.LoadConfigurationXml(document);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class CommandLineParameters
        {
            public CommandLineParameters(string[] parameters)
            {
                Parameters = parameters;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string[] Parameters { get; private set; }
        }
    }
}

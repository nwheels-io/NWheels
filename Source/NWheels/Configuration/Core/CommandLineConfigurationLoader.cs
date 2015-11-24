using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWheels.Extensions;
using NWheels.Hosting;

namespace NWheels.Configuration.Core
{
    public class CommandLineConfigurationLoader : LifecycleEventListenerBase
    {
        private const string ConfigurationArgumentName = "config:";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly XmlConfigurationLoader _loader;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CommandLineConfigurationLoader(XmlConfigurationLoader loader)
        {
            _loader = loader;
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

            var arguments = Environment.GetCommandLineArgs();

            foreach ( var arg in arguments )
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

        private bool IsConfigurationArgument(string arg, out string sectionName, out string propertyName, out string value)
        {
            sectionName = null;
            propertyName = null;
            value = null;

            if ( arg[0] != '/' && arg[0] != '-' )
            {
                return false;
            }

            if ( !arg.Substring(1).StartsWith(ConfigurationArgumentName, StringComparison.InvariantCultureIgnoreCase) )
            {
                return false;
            }

            var body = arg.Substring(1 + ConfigurationArgumentName.Length);
            var equalsIndex = arg.IndexOf('=');

            if ( equalsIndex < 1 )
            {
                return false;
            }

            var dotNames = body.Substring(0, equalsIndex).Split('.');

            if ( dotNames.Length < 2 )
            {
                return false;
            }

            sectionName = string.Join(".", dotNames.Take(dotNames.Length - 1));
            propertyName = dotNames.Last();
            value = body.Substring(equalsIndex + 1);

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autofac;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Hosting;

namespace NWheels.Configuration.Core
{
    public class XmlConfigurationLoader
    {
        public const string ConfigurationElementName = "Configuration";
        public const string AlwaysElementName = "Always";
        public const string IfElementName = "If";
        public const string CommentAttributeName = "comment";
        public const string MachineAttributeName = "machine"; // same as MachineNameAttributeName
        public const string MachineNameAttributeName = "machine-name"; // same as MachineAttributeName
        public const string NodeAttributeName = "node";
        public const string InstanceIdAttributeName = "instance-id";
        public const string EnvironmentAttributeName = "environment"; // same as EnvironmentNameAttributeName
        public const string EnvironmentNameAttributeName = "environment-name"; // same sa EnvironmentAttributeName
        public const string EnvironmentTypeAttributeName = "environment-type";
        public const string NullPropertyValueAttributeSuffix = ".null"; // myValue.null attribute can be used in place of myValue to set value to null

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public const int DescriptionTextMinLength = 15;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IComponentContext _components;
        private readonly IFramework _framework;
        private readonly IConfigurationLogger _logger;
        private readonly Dictionary<string, IInternalConfigurationObject> _sectionsByXmlName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public XmlConfigurationLoader(
            IComponentContext components, 
            IFramework framework, 
            IConfigurationLogger logger, 
            IEnumerable<IConfigurationSection> configurationSections)
        {
            _components = components;
            _framework = framework;
            _logger = logger;
            
            _sectionsByXmlName = configurationSections.Cast<IInternalConfigurationObject>().ToDictionary(
                s => s.GetXmlName(), 
                StringComparer.InvariantCultureIgnoreCase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadConfiguration(Pipeline<IConfigurationSource> configSources)
        {
            using (_logger.LoadingConfiguration())
            {
                foreach (var source in configSources)
                {
                    LoadConfigurationFromSource(source);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void LoadConfigurationXml(XDocument xml)
        {
            if ( xml.Root == null || !xml.Root.NameEqualsIgnoreCase(ConfigurationElementName) )
            {
                throw new XmlConfigurationException("The root element must be CONFIGURATION.");
            }

            foreach ( var scopeElement in xml.Root.Elements() )
            {
                if ( scopeElement.NameEqualsIgnoreCase(AlwaysElementName) )
                {
                    ApplyScope(scopeElement);
                }
                else if ( scopeElement.NameEqualsIgnoreCase(IfElementName) )
                {
                    var comment = scopeElement.GetAttributeIgnoreCase(CommentAttributeName);
                    var result = ShouldApplyScope(scopeElement);

                    _logger.EvaluatingXmlIfScope(comment, result);

                    if ( result )
                    {
                        ApplyScope(scopeElement);
                    }
                }
                else
                {
                    throw new XmlConfigurationException("Only IF and ALWAYS elements are allowed under the root element.");
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteConfigurationDocument(XDocument document, ConfigurationXmlOptions options)
        {
            var rootElement = new XElement(ConfigurationElementName.ToUpper());
            var scopeElement = new XElement(
                IfElementName.ToUpper(),
                new XAttribute(NodeAttributeName, _framework.CurrentNode.NodeName.EmptyIfNull()),
                new XAttribute(InstanceIdAttributeName, _framework.CurrentNode.InstanceId.EmptyIfNull()),
                new XAttribute(EnvironmentAttributeName, _framework.CurrentNode.EnvironmentName.EmptyIfNull()),
                new XAttribute(EnvironmentTypeAttributeName, _framework.CurrentNode.EnvironmentType.EmptyIfNull()));
            
            rootElement.Add(scopeElement);
            document.Add(rootElement);

            var delimiterText = new string('|', 100);

            foreach ( var section in _sectionsByXmlName.Values.OrderBy(s => s.GetXmlName(), StringComparer.InvariantCultureIgnoreCase) )
            {
                if ( options.IncludeOverrideHistory )
                {
                    scopeElement.Add(new XComment(delimiterText));
                    scopeElement.Add(new XComment(delimiterText));
                    scopeElement.Add(new XComment(delimiterText));
                    scopeElement.Add(new XComment(string.Format(" {0,-99}", section.GetXmlName())));
                    scopeElement.Add(new XComment(delimiterText));
                }

                var sectionElement = new XElement(section.GetXmlName());
                scopeElement.Add(sectionElement);
                section.SaveXml(sectionElement, options);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LoadConfigurationFromSource(IConfigurationSource source)
        {
            using (_logger.LoadingConfigurationSource(source.SourceType, source.SourceLevel))
            {
                try
                {
                    LoadSourceDocuments(source);

                    using (var activity = _logger.ApplyingProgrammaticConfiguration())
                    {
                        using (ConfigurationSourceInfo.UseSource(source.SourceLevel, source.SourceType, name: null))
                        {
                            try
                            {
                                source.ApplyConfigurationProgrammatically(_components);
                            }
                            catch (Exception e)
                            {
                                activity.Fail(e);
                                throw;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.FailedToLoadConfigurationSource(source.SourceType, source.SourceLevel, e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LoadSourceDocuments(IConfigurationSource source)
        {
            foreach (var document in source.GetConfigurationDocuments())
            {
                using (ConfigurationSourceInfo.UseSource(document.SourceInfo))
                {
                    LoadConfigurationXml(document.Xml);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldApplyScope(XElement scopeElement)
        {
            if (!Match(_framework.CurrentNode.MachineName, scopeElement.GetAttributeIgnoreCase(MachineAttributeName)))
            {
                return false;
            }
            
            if (!Match(_framework.CurrentNode.MachineName, scopeElement.GetAttributeIgnoreCase(MachineNameAttributeName)))
            {
                return false;
            }

            if (!Match(_framework.CurrentNode.NodeName, scopeElement.GetAttributeIgnoreCase(NodeAttributeName)))
            {
                return false;
            }

            if (!Match(_framework.CurrentNode.InstanceId, scopeElement.GetAttributeIgnoreCase(InstanceIdAttributeName)))
            {
                return false;
            }

            if (!Match(_framework.CurrentNode.EnvironmentName, scopeElement.GetAttributeIgnoreCase(EnvironmentAttributeName)))
            {
                return false;
            }

            if (!Match(_framework.CurrentNode.EnvironmentName, scopeElement.GetAttributeIgnoreCase(EnvironmentNameAttributeName)))
            {
                return false;
            }

            if (!Match(_framework.CurrentNode.EnvironmentType, scopeElement.GetAttributeIgnoreCase(EnvironmentTypeAttributeName)))
            {
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ApplyScope(XElement scopeElement)
        {
            var comment = scopeElement.GetAttributeIgnoreCase(CommentAttributeName);

            if ( string.IsNullOrEmpty(comment) || comment.Trim().Length < DescriptionTextMinLength )
            {
                throw _logger.EveryScopeMustHaveComment(DescriptionTextMinLength);
            }

            _logger.ApplyingXmlScope(comment);

            foreach ( var sectionElement in scopeElement.Elements() )
            {
                IInternalConfigurationObject sectionObject;

                if ( _sectionsByXmlName.TryGetValue(sectionElement.Name.ToString(), out sectionObject) )
                {
                    _logger.ApplyingXmlConfigSection(sectionElement.Name.ToString());
                    sectionObject.LoadObject(sectionElement);
                }
                else
                {
                    throw _logger.SectionNotRegistered(sectionElement.Name.ToString());
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool Match(string value, string expectedValueExpression)
        {
            if ( string.IsNullOrEmpty(expectedValueExpression) )
            {
                return true;
            }

            bool inverse;
            string[] expectedValueList;
            var expression = expectedValueExpression.Trim();

            if ( expression.StartsWith("!") )
            {
                inverse = true;
                expectedValueList = expression.Substring(1).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                inverse = false;
                expectedValueList = expression.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var valueFound = expectedValueList.Any(pattern => ValueMatchesPattern(value, pattern));
            return (valueFound ^ inverse);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ValueMatchesPattern(string value, string pattern)
        {
            if (pattern[pattern.Length - 1] == '*' && value != null)
            {
                return value.StartsWithIgnoreCase(pattern.Substring(0, pattern.Length - 1));
            }
            else
            {
                return pattern.EqualsIgnoreCase(value);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
        public const string NodeAttributeName = "node";
        public const string InstanceIdAttributeName = "instance-id";
        public const string EnvironmentAttributeName = "environment";
        public const string EnvironmentTypeAttributeName = "environment-type";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public const int DescriptionTextMinLength = 15;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IFramework _framework;
        private readonly IConfigurationLogger _logger;
        private readonly Dictionary<string, IInternalConfigurationObject> _sectionsByXmlName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public XmlConfigurationLoader(IFramework framework, Auto<IConfigurationLogger> logger, IEnumerable<IConfigurationSection> configurationSections)
        {
            _framework = framework;
            _logger = logger.Instance;
            
            _sectionsByXmlName = configurationSections.Cast<IInternalConfigurationObject>().ToDictionary(
                s => s.GetXmlName(), 
                StringComparer.InvariantCultureIgnoreCase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadConfiguration(IEnumerable<BootConfiguration.ConfigFile> configFiles)
        {
            using ( _logger.LoadingConfiguration() )
            {
                foreach ( var file in configFiles )
                {
                    LoadConfigurationFile(file);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LoadConfigurationFile(BootConfiguration.ConfigFile file)
        {
            if ( file.IsOptionalAndMissing )
            {
                _logger.OptionalFileNotPresentSkipping(file.Path);
                return;
            }

            using ( _logger.LoadingConfigurationFile(file.Path) )
            {
                using ( ConfigurationSourceInfo.UseSource(ConfigurationSourceLevel.Deployment, file.Path) )
                {
                    try
                    {
                        var document = XDocument.Load(file.Path, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                        LoadConfigurationDocument(document);
                    }
                    catch ( Exception e )
                    {
                        _logger.FailedToLoadConfigurationFile(file.Path, e);
                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void LoadConfigurationDocument(XDocument document)
        {
            if ( !document.Root.NameEqualsIgnoreCase(ConfigurationElementName) )
            {
                throw new XmlConfigurationException("The root element must be CONFIGURATION.");
            }

            foreach ( var scopeElement in document.Root.Elements() )
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

        private bool ShouldApplyScope(XElement scopeElement)
        {
            if ( !Match(_framework.CurrentNode.NodeName, scopeElement.GetAttributeIgnoreCase(NodeAttributeName)) )
            {
                return false;
            }

            if ( !Match(_framework.CurrentNode.InstanceId, scopeElement.GetAttributeIgnoreCase(InstanceIdAttributeName)) )
            {
                return false;
            }

            if ( !Match(_framework.CurrentNode.EnvironmentName, scopeElement.GetAttributeIgnoreCase(EnvironmentAttributeName)) )
            {
                return false;
            }

            if ( !Match(_framework.CurrentNode.EnvironmentType, scopeElement.GetAttributeIgnoreCase(EnvironmentTypeAttributeName)) )
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

            var valueFound = expectedValueList.Any(v => v.EqualsIgnoreCase(value));
            return (valueFound ^ inverse);
        }
    }
}

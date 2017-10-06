using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NWheels.Kernel.Api.Exceptions;
using NWheels.Microservices.Runtime;

namespace NWheels.Microservices.Api.Exceptions
{
    [Serializable]
    public class InvalidMicroserviceXmlException : ExplainableExceptionBase
    {
        private InvalidMicroserviceXmlException(
            string reason, 
            XElement element,
            string expectedElement = null,
            string moduleName = null, 
            string featureName = null,
            string microserviceNameInXml = null,
            string microserviceNameInBootConfig = null)
            : base(reason)
        {       
            this.FileName = element.BaseUri;
            if (element is IXmlLineInfo lineInfo)
            {
                this.LineNumber = lineInfo.LineNumber;
                this.LinePosition = lineInfo.LinePosition;
            }

            this.FoundElement = element.Name.LocalName;
            this.ExpectedElement = expectedElement;
            this.ModuleName = moduleName;
            this.FeatureName = featureName;
            this.MicroserviceNameInXml = microserviceNameInXml;
            this.MicroserviceNameInBootConfig = microserviceNameInBootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected InvalidMicroserviceXmlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FileName { get; }
        public int LineNumber { get; }
        public int LinePosition { get; }
        public string ExpectedElement { get; }
        public string FoundElement { get; }
        public string MicroserviceNameInXml { get; }
        public string MicroserviceNameInBootConfig { get; }
        public string ModuleName { get; }
        public string FeatureName { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
        {
            yield return new KeyValuePair<string, string>(_s_stringFileName, this.FileName);
            yield return new KeyValuePair<string, string>(_s_stringLineNumber, this.LineNumber.ToString());
            yield return new KeyValuePair<string, string>(_s_stringLinePosition, this.LinePosition.ToString());
            yield return new KeyValuePair<string, string>(_s_stringExpectedElement, this.ExpectedElement);
            yield return new KeyValuePair<string, string>(_s_stringFoundElement, this.FoundElement);
            yield return new KeyValuePair<string, string>(_s_stringModuleName, this.ModuleName);
            yield return new KeyValuePair<string, string>(_s_stringFeatureName, this.FeatureName);
            yield return new KeyValuePair<string, string>(_s_stringMicroserviceNameInXml, this.MicroserviceNameInXml);
            yield return new KeyValuePair<string, string>(_s_stringMicroserviceNameInBootConfig, this.MicroserviceNameInBootConfig);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly static string _s_stringRootElementInvalid = nameof(RootElementInvalid);
        private readonly static string _s_stringModuleElementInvalid = nameof(ModuleElementInvalid);
        private readonly static string _s_stringFeatureElementInvalid = nameof(FeatureElementInvalid);
        private readonly static string _s_stringMicroserviceNameNotSpecified = nameof(MicroserviceNameNotSpecified);
        private readonly static string _s_stringMicroserviceNameConflict = nameof(MicroserviceNameConflict);
        private readonly static string _s_stringModuleNameNotSpecified = nameof(ModuleNameNotSpecified);
        private readonly static string _s_stringFeatureNameNotSpecified = nameof(FeatureNameNotSpecified);
        private readonly static string _s_stringFileName = nameof(FileName);
        private readonly static string _s_stringLineNumber = nameof(LineNumber);
        private readonly static string _s_stringLinePosition = nameof(LinePosition);
        private readonly static string _s_stringExpectedElement = nameof(ExpectedElement);
        private readonly static string _s_stringFoundElement = nameof(FoundElement);
        private readonly static string _s_stringModuleName = nameof(ModuleName);
        private readonly static string _s_stringFeatureName = nameof(FeatureName);
        private readonly static string _s_stringMicroserviceNameInXml = nameof(MicroserviceNameInXml);
        private readonly static string _s_stringMicroserviceNameInBootConfig = nameof(MicroserviceNameInBootConfig);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static InvalidMicroserviceXmlException RootElementInvalid(XElement element)
        {
            return new InvalidMicroserviceXmlException(
                reason: _s_stringRootElementInvalid, 
                element: element,
                expectedElement: MicroserviceXmlReader.MicroserviceElementName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static InvalidMicroserviceXmlException ModuleElementInvalid(XElement element)
        {
            return new InvalidMicroserviceXmlException(
                reason: _s_stringModuleElementInvalid, 
                element: element,
                expectedElement: MicroserviceXmlReader.ModuleElementName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static InvalidMicroserviceXmlException FeatureElementInvalid(XElement element)
        {
            return new InvalidMicroserviceXmlException(
                reason: _s_stringFeatureElementInvalid, 
                element: element,
                expectedElement: MicroserviceXmlReader.FeatureElementName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static InvalidMicroserviceXmlException MicroserviceNameNotSpecified(XElement element)
        {
            return new InvalidMicroserviceXmlException(_s_stringMicroserviceNameNotSpecified, element);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static InvalidMicroserviceXmlException MicroserviceNameConflict(XElement element, string nameInXml, string nameInBootConfig)
        {
            return new InvalidMicroserviceXmlException(
                reason: _s_stringMicroserviceNameConflict,
                element: element,
                microserviceNameInXml: nameInXml,
                microserviceNameInBootConfig: nameInBootConfig);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static InvalidMicroserviceXmlException ModuleNameNotSpecified(XElement element)
        {
            return new InvalidMicroserviceXmlException(
                reason: _s_stringModuleNameNotSpecified,
                element: element);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static InvalidMicroserviceXmlException FeatureNameNotSpecified(XElement element)
        {
            return new InvalidMicroserviceXmlException(
                reason: _s_stringFeatureNameNotSpecified,
                element: element);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Microservices.Runtime
{
    public static class MicroserviceXmlReader
    {
        public static readonly string DefaultFileName = "microservice.xml";

        public static readonly string MicroserviceElementName = "microservice";
        public static readonly string FrameworkModulesElementName = "framework-modules";
        public static readonly string ApplicationModulesElementName = "application-modules";
        public static readonly string CustomizationModulesElementName = "customization-modules";
        public static readonly string ModuleElementName = "module";
        public static readonly string FeatureElementName = "feature";

        public static readonly string MicroserviceNameAttribute = "name";
        public static readonly string ModuleAssemblyAttribute = "assembly";
        public static readonly string FeatureNameAttribute = "name";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void PopulateBootConfiguration(XElement xml, MutableBootConfiguration bootConfig)
        {
            ReadMicroserviceElement(xml, bootConfig);
            ReadModuleCollectionElement(xml.Element(FrameworkModulesElementName), bootConfig, bootConfig.FrameworkModules);
            ReadModuleCollectionElement(xml.Element(ApplicationModulesElementName), bootConfig, bootConfig.ApplicationModules);
            ReadModuleCollectionElement(xml.Element(CustomizationModulesElementName), bootConfig, bootConfig.CustomizationModules);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ReadMicroserviceElement(XElement element, MutableBootConfiguration bootConfig)
        {
            if (element.Name != MicroserviceElementName)
            {
                throw new InvalidMicroserviceXmlException(); //TODO: provide correct reason
            }

            var microserviceName = element.Attribute(MicroserviceNameAttribute)?.Value;

            if (string.IsNullOrEmpty(microserviceName) && string.IsNullOrEmpty(bootConfig.MicroserviceName))
            {
                throw new InvalidMicroserviceXmlException(); //TODO: provide correct reason
            }

            if (string.IsNullOrEmpty(bootConfig.MicroserviceName))
            {
                bootConfig.MicroserviceName = microserviceName;
            }
            else if (microserviceName != bootConfig.MicroserviceName)
            {
                throw new InvalidMicroserviceXmlException(); //TODO: provide correct reason
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ReadModuleCollectionElement(
            XElement element, 
            MutableBootConfiguration bootConfig, 
            List<MutableBootConfiguration.ModuleConfiguration> moduleList)
        {
            if (element == null)
            {
                return;
            }

            foreach (var moduleElement in element.Elements())
            {
                ReadModuleElement(moduleElement, bootConfig, moduleList);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ReadModuleElement(
            XElement element, 
            MutableBootConfiguration bootConfig, 
            List<MutableBootConfiguration.ModuleConfiguration> moduleList)
        {
            if (element.Name != ModuleElementName)
            {
                throw new InvalidMicroserviceXmlException(); //TODO: provide correct reason
            }

            var assemblyName = element.Attribute(ModuleAssemblyAttribute)?.Value;

            if (string.IsNullOrEmpty(assemblyName))
            {
                throw new InvalidMicroserviceXmlException(); //TODO: provide correct reason
            }

            var features = new List<string>();

            foreach (var featureElement in element.Elements())
            {
                ReadFeatureElement(featureElement, out string featureName);
                features.Add(featureName);
            }

            bootConfig.AddFeatures(moduleList, assemblyName, features);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ReadFeatureElement(XElement element, out string featureName)
        {
            if (element.Name != FeatureElementName)
            {
                throw new InvalidMicroserviceXmlException(); //TODO: provide correct reason
            }

            featureName = element.Attribute(FeatureNameAttribute)?.Value;

            if (string.IsNullOrEmpty(featureName))
            {
                throw new InvalidMicroserviceXmlException(); //TODO: provide correct reason
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class InvalidMicroserviceXmlException : Exception
    {
    }
}
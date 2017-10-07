using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Api;
using NWheels.Microservices.Api.Exceptions;

namespace NWheels.Microservices.Runtime
{
    public static class MicroserviceXmlReader
    {
        public static readonly string DefaultFileName = "microservice.xml";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly string MicroserviceElementName = "microservice";
        public static readonly string FrameworkModulesElementName = "framework-modules";
        public static readonly string ApplicationModulesElementName = "application-modules";
        public static readonly string CustomizationModulesElementName = "customization-modules";
        public static readonly string ModuleElementName = "module";
        public static readonly string FeatureElementName = "feature";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
                throw InvalidMicroserviceXmlException.RootElementInvalid(element);
            }

            var microserviceName = element.Attribute(MicroserviceNameAttribute)?.Value;

            if (string.IsNullOrEmpty(microserviceName))
            {
                if (string.IsNullOrEmpty(bootConfig.MicroserviceName))
                {
                    throw InvalidMicroserviceXmlException.MicroserviceNameNotSpecified(element);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(bootConfig.MicroserviceName))
                {
                    bootConfig.MicroserviceName = microserviceName;
                }
                else if (microserviceName != bootConfig.MicroserviceName)
                {
                    throw InvalidMicroserviceXmlException.MicroserviceNameConflict(element, microserviceName, bootConfig.MicroserviceName);
                }
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
                throw InvalidMicroserviceXmlException.ModuleElementInvalid(element);
            }

            var assemblyName = element.Attribute(ModuleAssemblyAttribute)?.Value;

            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                throw InvalidMicroserviceXmlException.ModuleAssemblyNotSpecified(element);
            }

            var features = new List<string>();

            foreach (var featureElement in element.Elements())
            {
                ReadFeatureElement(assemblyName, featureElement, out string featureName);
                features.Add(featureName);
            }

            bootConfig.AddFeatures(moduleList, assemblyName, features);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ReadFeatureElement(string moduleName, XElement element, out string featureName)
        {
            if (element.Name != FeatureElementName)
            {
                throw InvalidMicroserviceXmlException.FeatureElementInvalid(moduleName, element);
            }

            featureName = element.Attribute(FeatureNameAttribute)?.Value;

            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw InvalidMicroserviceXmlException.FeatureNameNotSpecified(moduleName, element);
            }
        }
    }
}
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NWheels.Microservices
{
    [XmlRoot(ElementName = "microservice", IsNullable = false)]
    public class MicroserviceConfig
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlElement("injection-adapter")]
        public InjectionAdapterWrapper InjectionAdapter { get; set; }

        [XmlArray("framework-modules")]
        [XmlArrayItem("module")]
        public List<ModuleConfig> FrameworkModules { get; set; }
        
        [XmlArray("application-modules")]
        [XmlArrayItem("module")]
        public List<ModuleConfig> ApplicationModules { get; set; }
        
        public class InjectionAdapterWrapper
        {
            [XmlAttribute(AttributeName = "assembly")]
            public string Assembly { get; set; }
        }

        public class ModuleConfig
        {
            [XmlAttribute(AttributeName = "assembly")]
            public string Assembly { get; set; }

            [XmlElement(ElementName = "feature")]
            public List<FeatureConfig> Features { get; set; }

            public class FeatureConfig
            {
                [XmlAttribute(AttributeName = "name")]
                public string Name { get; set; }
            }
        }
    }
}

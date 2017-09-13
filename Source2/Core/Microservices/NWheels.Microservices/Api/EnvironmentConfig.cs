using System.Xml.Serialization;

namespace NWheels.Microservices.Api
{
    [XmlRoot(ElementName = "environment", IsNullable = false)]
    public class EnvironmentConfig
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlArray("variable-list")]
        [XmlArrayItem("variable")]
        public VariableConfig[] Variables { get; set; }

        public class VariableConfig
        {
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            [XmlAttribute(AttributeName = "value")]
            public string Value { get; set; }
        }
    }
}

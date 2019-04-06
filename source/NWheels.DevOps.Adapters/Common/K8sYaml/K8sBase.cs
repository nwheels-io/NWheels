using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public abstract class K8sBase
    {
        private static readonly ISerializer YamlSerializer = 
            new SerializerBuilder()
            .WithNamingConvention(new CamelCaseNamingConvention())
            .Build(); 
        
        public string ToYamlString()
        {
            var yaml = YamlSerializer.Serialize(this);
            return yaml;
        }
        
        public string ApiVersion { get; set; }
        public string Kind { get; set; }
        public K8sMetadata Metadata { get; set; }
    }
}

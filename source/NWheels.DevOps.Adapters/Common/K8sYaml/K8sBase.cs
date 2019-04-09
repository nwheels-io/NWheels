using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using MetaPrograms.Extensions;
using YamlDotNet.RepresentationModel;
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
            var text = new StringBuilder();

            Comment?.ForEach(line => text.AppendLine($"# {line}"));

            var yaml = YamlSerializer.Serialize(this);
            text.Append(yaml);
            
            return text.ToString();
        }
        
        [YamlMember(Order = -3)]
        public string ApiVersion { get; set; }
        
        [YamlMember(Order = -2)]
        public string Kind { get; set; }
        
        [YamlMember(Order = -1)]
        public K8sMetadata Metadata { get; set; }
        
        [YamlIgnore]
        public List<string> Comment { get; set; }
    }
    
    public static class EnumerableExtensions
    {
        public static string ToYamlString(this IEnumerable<K8sBase> k8sObjects, params string[] commentLines)
        {
            var text = new StringBuilder();

            commentLines?.ForEach((line, index) => text.AppendLine($"# {line}"));
            
            k8sObjects.ToList().ForEach((obj, index) => {
                if (index > 0)
                {
                    text.AppendLine("---");
                }
                text.Append(obj.ToYamlString());
            });
            
            return text.ToString();
        }
    }
}

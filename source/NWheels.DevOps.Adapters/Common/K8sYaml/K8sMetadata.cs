using System.Collections.Generic;

namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sMetadata
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Labels { get; set; }
        public Dictionary<string, string> Annotations { get; set; }
    }
}

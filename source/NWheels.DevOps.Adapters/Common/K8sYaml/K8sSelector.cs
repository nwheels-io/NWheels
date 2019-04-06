using System.Collections.Generic;

namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sSelector
    {
        public Dictionary<string, string> MatchLabels { get; set; }
    }
}

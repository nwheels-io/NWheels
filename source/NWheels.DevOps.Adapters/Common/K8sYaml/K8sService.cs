using System.Collections.Generic;

namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sService : K8sBase
    {
        public K8sService()
        {
            this.ApiVersion = "apps/v1";
            this.Kind = K8sKind.Service;
        }

        public SpecType Spec { get; set; }
        
        public class SpecType
        {
            public ServiceKind Type { get; set; }
            public Dictionary<string, string> Selector { get; set; }
            public List<K8sPort> Ports { get; set; }
            public string ClusterIP { get; set; }
        }

        public enum ServiceKind
        {
            ClusterIP = 0,
            NodePort,
            LoadBalancer,
            ExternalName
        }
    }
}
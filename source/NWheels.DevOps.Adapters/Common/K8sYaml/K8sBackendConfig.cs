namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sBackendConfig : K8sBase
    {
        public K8sBackendConfig()
        {
            this.ApiVersion = "cloud.google.com/v1beta1";
            this.Kind = K8sKind.BackendConfig;
        }

        public SpecType Spec { get; set; }
        
        public class SpecType
        {
            public CdnType Cdn { get; set; }
        }

        public class CdnType
        {
            public bool Enabled { get; set; }
            public CdnCachePolicyType CachePolicy { get; set; }
        }

        public class CdnCachePolicyType
        {
            public bool IncludeHost { get; set; }
            public bool IncludeProtocol { get; set; }
            public bool IncludeQueryString { get; set; }
        }
    }
}

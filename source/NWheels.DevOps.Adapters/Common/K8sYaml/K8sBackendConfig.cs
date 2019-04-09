namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sBackendConfig : K8sBase
    {
        public K8sBackendConfig()
        {
            this.ApiVersion = "cloud.google.com/v1beta1";
            this.Kind = K8sKind.BackendConfig;
        }
    }
}

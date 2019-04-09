namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sService : K8sBase
    {
        public K8sService()
        {
            this.ApiVersion = "apps/v1";
            this.Kind = K8sKind.Service;
        }
    }
}
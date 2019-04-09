namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sIngress : K8sBase
    {
        public K8sIngress()
        {
            this.ApiVersion = "extensions/v1beta1"; // TODO: can use "apps/v1"?
            this.Kind = K8sKind.Ingress;
        }
    }
}
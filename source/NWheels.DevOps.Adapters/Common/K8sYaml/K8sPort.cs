namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sPort 
    {
        public string Name { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public object TargetPort { get; set; }
    }
}
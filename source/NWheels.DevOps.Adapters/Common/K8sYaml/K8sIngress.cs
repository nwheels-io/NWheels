using System.Collections.Generic;
using System.Data;

namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sIngress : K8sBase
    {
        public K8sIngress()
        {
            this.ApiVersion = "extensions/v1beta1"; // TODO: can use "apps/v1"?
            this.Kind = K8sKind.Ingress;
        }

        public SpecType Spec { get; set; }
        
        public class SpecType
        {
            public List<RuleType> Rules { get; set; } 
        }

        public abstract class RuleType
        {
        }

        public class HttpRuleType : RuleType
        {
            public HttpRuleBody Http { get; set; }
        }
        
        public class HttpRuleBody
        {
            public List<RulePathType> Paths { get; set; }
        }

        public class RulePathType
        {
            public string Path { get; set; }
            public K8sBackend Backend { get; set; }
        }
    }
}
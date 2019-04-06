using System.Collections.Generic;

namespace NWheels.DevOps.Adapters.Common.K8sYaml
{
    public class K8sDeployment : K8sBase
    {
        public K8sDeployment()
        {
            this.ApiVersion = "apps/v1";
            this.Kind = K8sKind.Deployment;
        }

        public SpecType Spec { get; set; }

        public class SpecType
        {
            public K8sSelector Selector { get; set; }
            public int? Replicas { get; set; }
            public TemplateType Template { get; set; }
        }

        public class TemplateType
        {
            public TemplateMetadataType Metadata { get; set; }
            public TemplateSpecType Spec { get; set; }
        }

        public class TemplateMetadataType
        {
            public Dictionary<string, string> Labels { get; set; }
        }

        public class TemplateSpecType
        {
            public List<TemplateSpecContainerType> Containers { get; set; }           
        }

        public class TemplateSpecContainerType
        {
            public string Name { get; set; }
            public string Image { get; set; }
        }
    }
}
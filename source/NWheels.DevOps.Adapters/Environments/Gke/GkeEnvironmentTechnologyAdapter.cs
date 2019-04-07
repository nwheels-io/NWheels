using System.Collections.Generic;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.DevOps.Adapters.Common.K8sYaml;
using NWheels.DevOps.Model.Impl.Metadata;

namespace NWheels.DevOps.Adapters.Environments.Gke
{
    public class GkeEnvironmentTechnologyAdapter : ITechnologyAdapter
    {
        public void Execute(ITechnologyAdapterContext context)
        {
            var outputFolder = new[] {"devops", "gke"};
            var environment = (EnvironmentMetadata)context.Input;

            var deployment = new K8sDeployment {
                Metadata = new K8sMetadata {
                    Namespace = "demo-hello-world",
                    Name = "hello-world-deployment"
                },
                Spec = new K8sDeployment.SpecType {
                    Selector = new K8sSelector {
                        MatchLabels = new Dictionary<string, string> {
                            { "purpose", "deploy-hello-world" }
                        }
                    },
                    Replicas = 1,
                    Template = new K8sDeployment.TemplateType {
                        Metadata = new K8sDeployment.TemplateMetadataType {
                            Labels = new Dictionary<string, string> {
                                { "purpose", "deploy-hello-world" }
                            }
                        },
                        Spec = new K8sDeployment.TemplateSpecType {
                            Containers = new List<K8sDeployment.TemplateSpecContainerType> {
                                new K8sDeployment.TemplateSpecContainerType {
                                    Name = "hello-world-site",
                                    Image = "gcr.io/galvanic-wall-235207/demo-hello-world"
                                }
                            }
                        }
                    }
                }
            };
            
            context.Output.AddSourceFile(
                outputFolder,
                "hello-world-deployment.yaml",
                deployment.ToYamlString()
            );
        }
    }
}

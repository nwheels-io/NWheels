using System.Collections.Generic;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.DevOps.Adapters.Common.K8sYaml;
using NWheels.DevOps.Model.Impl.Metadata;

namespace NWheels.DevOps.Adapters.Environments.Gke
{
    public class StaticContentStrategy : GkeConfigStrategy
    {

        public StaticContentStrategy(
            TechnologyAdapterContext<EnvironmentMetadata> context,
            DeploymentImageMetadata image) 
            : base(context, image)
        {
        }

        public override IEnumerable<K8sBase> BuildConfiguration()
        {
            var matchTagName = "purpose";
            var matchTagValue = $"deploy-{Image.Name}";
            
            yield return CreateDeployment();
            yield return CreateBackendConfig();
            yield return CreateService();
            yield return CreateIngress();

            K8sDeployment CreateDeployment()
            {
                var deployment = new K8sDeployment {
                    Metadata = new K8sMetadata {
                        Namespace = NamespaceName,
                        Name = EnvironmentName
                    },
                    Spec = new K8sDeployment.SpecType {
                        Selector = new K8sSelector {
                            MatchLabels = new Dictionary<string, string> {
                                {matchTagName, matchTagValue}
                            }
                        },
                        Replicas = 1,
                        Template = new K8sDeployment.TemplateType {
                            Metadata = new K8sDeployment.TemplateMetadataType {
                                Labels = new Dictionary<string, string> {
                                    {matchTagName, matchTagValue}
                                }
                            },
                            Spec = new K8sDeployment.TemplateSpecType {
                                Containers = new List<K8sDeployment.TemplateSpecContainerType> {
                                    new K8sDeployment.TemplateSpecContainerType {
                                        Name = Image.Name,
                                        Image = Image.Header.Extensions.Get<GcrNameExtension>().GcrName
                                    }
                                }
                            }
                        }
                    }
                };
                return deployment;
            }

            K8sBackendConfig CreateBackendConfig()
            {
                return new K8sBackendConfig();
            }

            K8sService CreateService()
            {
                return new K8sService();
            }

            K8sIngress CreateIngress()
            {
                return new K8sIngress();
            }
        }
    }
}
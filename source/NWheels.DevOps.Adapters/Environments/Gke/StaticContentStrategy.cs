using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using MetaPrograms;
using MetaPrograms.CSharp.Writer.SyntaxEmitters;
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
            var matchTagName = "component";
            var matchTagValue = Image.Name;
            
            yield return CreateDeployment();
            yield return CreateBackendConfig(out var backendConfigName);
            yield return CreateService(out var serviceName);
            yield return CreateIngress();

            K8sDeployment CreateDeployment()
            {
                return new K8sDeployment {
                    Metadata = new K8sMetadata {
                        Namespace = NamespaceName,
                        Name = $"{Image.Name}-deployment"
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
            }

            K8sBackendConfig CreateBackendConfig(out string name)
            {
                name = $"{Image.Name}-backend-config";
                
                return new K8sBackendConfig {
                    Metadata = new K8sMetadata {
                        Namespace = NamespaceName,
                        Name = backendConfigName
                    },
                    Spec = new K8sBackendConfig.SpecType {
                        Cdn = new K8sBackendConfig.CdnType {
                            Enabled = true,
                            CachePolicy = new K8sBackendConfig.CdnCachePolicyType {
                                IncludeHost = true,
                                IncludeProtocol = true,
                                IncludeQueryString = true
                            }
                        }
                    }
                };
            }

            K8sService CreateService(out string name)
            {
                name = $"{Image.Name}-service";
                var targetPort = (Image.ListenPorts?.Count > 0 ? Image.ListenPorts[0] : 80);
                
                return new K8sService {
                    Metadata = new K8sMetadata {
                        Namespace = NamespaceName,
                        Name = name,
                        Labels = new Dictionary<string, string> {
                            { matchTagName, matchTagValue }
                        },
                        Annotations = new Dictionary<string, string> {
                            {
                                "beta.cloud.google.com/backend-config",
                                $"{{\"ports\": {{\"80\":\"{backendConfigName}\"}} }}"
                            }
                        }
                    },
                    Spec = new K8sService.SpecType {
                        Type = K8sService.ServiceKind.NodePort,
                        Selector = new Dictionary<string, string> {
                            { matchTagName, matchTagValue }
                        },
                        Ports = new List<K8sPort> {
                            new K8sPort {
                                Port = 80,
                                Protocol = "TCP",
                                TargetPort = targetPort 
                            }
                        }
                    }
                };
            }

            K8sIngress CreateIngress()
            {
                return new K8sIngress() {
                    Metadata = new K8sMetadata {
                        Namespace = NamespaceName,
                        Name = $"{Image.Name}-ingress",
                        Labels = new Dictionary<string, string> {
                            {matchTagName, matchTagValue}
                        },
                        Annotations = new Dictionary<string, string> {
                            {
                                "kubernetes.io/ingress.global-static-ip-name", 
                                Image.PublicEndpoint //TODO: log warning if not specified
                            }
                        }
                    },
                    Spec = new K8sIngress.SpecType {
                        Rules = new List<K8sIngress.RuleType> {
                            new K8sIngress.HttpRuleType {
                                Http = new K8sIngress.HttpRuleBody {
                                    Paths = new List<K8sIngress.RulePathType> { 
                                        new K8sIngress.RulePathType {
                                            Path = "/*",
                                            Backend = new K8sBackend {
                                                ServiceName = serviceName,
                                                ServicePort = 80
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}

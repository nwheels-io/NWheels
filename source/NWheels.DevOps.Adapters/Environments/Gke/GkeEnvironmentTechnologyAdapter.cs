using System.Collections.Generic;
using System.Linq;
using MetaPrograms;
using Microsoft.Extensions.Logging.Abstractions;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.DevOps.Adapters.Common.Dockerfiles;
using NWheels.DevOps.Adapters.Common.K8sYaml;
using NWheels.DevOps.Model.Impl.Metadata;
using FilePath = MetaPrograms.FilePath;

namespace NWheels.DevOps.Adapters.Environments.Gke
{
    public class GkeEnvironmentTechnologyAdapter : IDeploymentTechnologyAdapter
    {
        public void GenerateOutputs(ITechnologyAdapterContext context)
        {
            var folderPath = new FilePath("devops", "gke");
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
                                    Image = GetImageGcrName(context)
                                }
                            }
                        }
                    }
                }
            };
            
            context.Output.AddSourceFile(
                folderPath.Append("hello-world-deployment.yaml"),
                deployment.ToYamlString()
            );

        }

        public void GenerateDeploymentOutputs(ITechnologyAdapterContext context)
        {
            var yamlPath = new FilePath("devops", "gke", "hello-world-deployment.yaml");

            GenerateDockerfiles();
            context.DeploymentScript.AddDeployCommand($"kubectl apply -f {yamlPath.FullPath}");
            
            void GenerateDockerfiles()
            {
                GenerateMetaObjectDockerfiles(context.Input);

                foreach (var prop in context.Input.Header.SourceType.GetAllProperties())
                {
                    if (context.Preprocessor.GetByConcreteType(prop.Type)?.ParsedMetadata is IMetadataObject propMeta)
                    {
                        GenerateMetaObjectDockerfiles(propMeta);
                    }
                }
            }

            void GenerateMetaObjectDockerfiles(IMetadataObject meta)
            {
                foreach (var image in meta.Header.DeploymentScript.Images)
                {
                    GenerateImageDockerfile(image);
                }
            }

            void GenerateImageDockerfile(DeploymentImageMetadata image)
            {
                var path = image.BuildContextPath.Append("Dockerfile");
                var contents = image.ToDockerfileText();
                var gcrName = GetImageGcrName(context);
                    
                context.Output.AddSourceFile(path, contents);
                context.DeploymentScript.AddBuildCommand($"docker build -t {image.Name} -t {gcrName} {image.BuildContextPath.FullPath}");
                context.DeploymentScript.AddDeployCommand($"docker push {gcrName}");
            }
        }

        private Config ParseConfig(ITechnologyAdapterContext context)
        {
            return new Config {
                Project = context.Input.Header.TechnologyAdapters[0].Parameters["project"].ToString(),
                Zone = context.Input.Header.TechnologyAdapters[0].Parameters["zone"].ToString()
            };
        }

        private string GetImageGcrName(ITechnologyAdapterContext context)
        {
            var config = ParseConfig(context);
            return $"gcr.io/{config.Project}/{context.Input.Header.QualifiedName}";
        }
        
        private class Config
        {
            public string Project { get; set; }
            public string Zone { get; set; }
        }
    }
}

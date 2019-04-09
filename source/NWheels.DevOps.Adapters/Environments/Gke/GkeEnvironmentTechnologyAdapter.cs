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
    public class GkeEnvironmentTechnologyAdapter : DeploymentTechnologyAdapter<EnvironmentMetadata>
    {
        protected override void GenerateOutputs(TechnologyAdapterContext<EnvironmentMetadata> context)
        {
            var folderPath = new FilePath("devops", "gke");
            var gkeNamespace = IdentifierName
                .Flatten(context.Input.Header.Namespace, CasingStyle.Pascal)
                .ToString(CasingStyle.Kebab);
            var environmentName = context.Input.Header.Name.ToString(CasingStyle.Kebab);
 
            
            
            var deployment = new K8sDeployment {
                Metadata = new K8sMetadata {
                    Namespace = gkeNamespace,
                    Name = environmentName
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

        protected override void GenerateDeploymentOutputs(TechnologyAdapterContext<EnvironmentMetadata> context)
        {
            var yamlFolder = new FilePath("devops", "gke");
            GenerateAllDockerfiles();
            
            void GenerateAllDockerfiles()
            {
                GenerateMetaObjectDockerfiles(context.Input);

                var sourceType = context.Input.Header.SourceType;
                
                foreach (var prop in sourceType.GetAllProperties())
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
                
                image.Header.Extensions.Set(new GcrNameExtension { GcrName = gcrName });
                
                context.Output.AddSourceFile(path, contents);
                context.DeploymentScript.AddBuildCommand($"docker build -t {image.Name} -t {gcrName} {image.BuildContextPath.FullPath}");
                context.DeploymentScript.AddDeployCommand($"docker push {gcrName}");
                
                GenerateK8sConfiguration(image);
            }

            void GenerateK8sConfiguration(DeploymentImageMetadata image)
            {
                var configStrategy = GkeConfigStrategy.Create(context, image);
                var config = configStrategy.BuildConfiguration();
                var yamlText = config.ToYamlString();
                var filePath = yamlFolder.Append($"{image.Name}.yaml");
                
                context.Output.AddSourceFile(filePath, yamlText);
                context.DeploymentScript.AddDeployCommand($"kubectl apply -f {filePath}");
            }
        }

        private Config ParseConfig(TechnologyAdapterContext<EnvironmentMetadata> context)
        {
            return new Config {
                Project = context.Input.Header.TechnologyAdapters[0].Parameters["project"].ToString(),
                Zone = context.Input.Header.TechnologyAdapters[0].Parameters["zone"].ToString()
            };
        }

        private string GetImageGcrName(TechnologyAdapterContext<EnvironmentMetadata> context)
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

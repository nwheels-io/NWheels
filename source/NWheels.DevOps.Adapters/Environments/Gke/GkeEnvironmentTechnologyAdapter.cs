using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using MetaPrograms;
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
        }

        protected override void GenerateDeploymentOutputs(TechnologyAdapterContext<EnvironmentMetadata> context)
        {
            var config = ParseConfig(context);
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
                    GenerateImageDockerfile(meta, image);
                }
            }

            void GenerateImageDockerfile(IMetadataObject meta, DeploymentImageMetadata image)
            {
                var path = image.BuildContextPath.Append("Dockerfile");
                var contents = image.ToDockerfileText();
                var gcrName = GetImageGcrName(image);
                
                image.Header.Extensions.Set(new GcrNameExtension { GcrName = gcrName });
                
                context.Output.AddSourceFile(path, contents);
                context.DeploymentScript.AddBuildCommand($"docker build -t {image.Name} -t {gcrName} {image.BuildContextPath.FullPath}");
                context.DeploymentScript.AddDeployCommand($"docker push {gcrName}");
                
                GenerateK8sConfiguration(meta, image);
            }

            void GenerateK8sConfiguration(IMetadataObject meta, DeploymentImageMetadata image)
            {
                var configStrategy = GkeConfigStrategy.Create(context, image);
                var gkeConfig = configStrategy.BuildConfiguration().ToList();
                var yamlText = gkeConfig.ToYamlString();
                var filePath = yamlFolder.Append($"{image.Name}.yaml");
                
                context.Output.AddSourceFile(filePath, yamlText);
                context.DeploymentScript.AddDeployCommand($"kubectl apply -f {filePath}");
            }

            string GetImageGcrName(DeploymentImageMetadata image)
            {
                return $"gcr.io/{config.Project}/{image.Name}";
            }
        }

        private Config ParseConfig(TechnologyAdapterContext<EnvironmentMetadata> context)
        {
            return new Config {
                Project = context.Adapter.Parameters["project"].ToString(),
                Zone = context.Adapter.Parameters["zone"].ToString()
            };
        }

        
        private class Config
        {
            public string Project { get; set; }
            public string Zone { get; set; }
        }
    }
}

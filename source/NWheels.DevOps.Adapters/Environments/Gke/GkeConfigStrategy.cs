using System.Collections.Generic;
using MetaPrograms;
using NWheels.Composition.Model.Impl;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.DevOps.Adapters.Common.K8sYaml;
using NWheels.DevOps.Model.Impl.Metadata;

namespace NWheels.DevOps.Adapters.Environments.Gke
{
    public abstract class GkeConfigStrategy
    {
        protected GkeConfigStrategy(
            TechnologyAdapterContext<EnvironmentMetadata> context,
            DeploymentImageMetadata image)
        {
            Context = context;
            Image = image;
            NamespaceName = IdentifierName
                .Flatten(context.Input.Header.Namespace, CasingStyle.Pascal)
                .ToString(CasingStyle.Kebab);
            EnvironmentName = context.Input.Header.Name.ToString(CasingStyle.Kebab);
        }
        
        public abstract IEnumerable<K8sBase> BuildConfiguration();

        public TechnologyAdapterContext<EnvironmentMetadata> Context { get; }
        public DeploymentImageMetadata Image { get; }
        public string NamespaceName { get; }
        public string EnvironmentName { get; }

        public static GkeConfigStrategy Create(
            TechnologyAdapterContext<EnvironmentMetadata> context,
            DeploymentImageMetadata image)
        {
            switch (image.ContentType)
            {
                case DeploymentContentType.Static:
                    return new StaticContentStrategy(context, image);
                default:
                    throw new BuildErrorException(
                        context.Input.Header.SourceType.ConcreteType, 
                        $"Unsupported image content type: {image.ContentType}");
            }
        }
    }
}












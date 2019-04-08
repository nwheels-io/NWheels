using System.Linq;
using System.Text;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.DevOps.Adapters.Common.Dockerfiles
{
    public static class DockerfileGenerator
    {
        public static string ToDockerfileText(this DeploymentImageMetadata image)
        {
            var contents = new StringBuilder();

            if (image.BaseImage != null)
            {
                contents.AppendLine($"FROM {image.BaseImage}");
            }

            foreach (var copy in image.FilesToCopy)
            {
                contents.AppendLine($"COPY {copy.Key.NormalizedFullPath} {copy.Value.NormalizedFullPath}");
            }

            if (image.EntryPointCommand != null)
            {
                contents.AppendLine($"ENTRYPOINT [ {string.Join(" , ", image.EntryPointCommand.Select(s => $"\"{s}\""))} ]");
            }

            return contents.ToString();
        }
    }
}

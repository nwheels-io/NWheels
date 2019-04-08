using System.Collections.Generic;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public class DeploymentScript : IDeploymentScriptBuilder
    {
        private readonly List<DeploymentImageMetadata> _images = new List<DeploymentImageMetadata>();
        private readonly List<PlatformCommand> _initOnceCommands = new List<PlatformCommand>();
        private readonly List<PlatformCommand> _buildCommands = new List<PlatformCommand>();
        private readonly List<PlatformCommand> _deployCommands= new List<PlatformCommand>();

        void IDeploymentScriptBuilder.AddImage(DeploymentImageMetadata image)
        {
            _images.Add(image);
        }

        void IDeploymentScriptBuilder.AddInitOnceCommand(PlatformCommand command)
        {
            _initOnceCommands.Add(command);
        }

        void IDeploymentScriptBuilder.AddBuildCommand(PlatformCommand command)
        {
            _buildCommands.Add(command);
        }

        void IDeploymentScriptBuilder.AddDeployCommand(PlatformCommand command)
        {
            _deployCommands.Add(command);
        }

        public IEnumerable<DeploymentImageMetadata> Images => _images;
        public IEnumerable<PlatformCommand> InitOnceCommands => _initOnceCommands;
        public IEnumerable<PlatformCommand> BuildCommands => _buildCommands;
        public IEnumerable<PlatformCommand> DeployCommands => _deployCommands;
    }
}

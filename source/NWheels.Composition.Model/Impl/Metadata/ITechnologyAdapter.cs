using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MetaPrograms;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public interface ITechnologyAdapter
    {
        void GenerateOutputs(ITechnologyAdapterContext context);
    }
    
    public interface ITechnologyAdapterContext
    {
        void AddMessage(string category, string message);
        IReadOnlyPreprocessorOutput Preprocessor { get; }
        IMetadataObject Input { get; }
        ICodeGeneratorOutput Output { get; }
        IDeploymentScriptBuilder DeploymentScript { get; }
    }

    public interface IDeploymentScriptBuilder
    {
        void AddImage(DeploymentImageMetadata image);
        void AddInitOnceCommand(PlatformCommand command);
        void AddBuildCommand(PlatformCommand command);
        void AddDeployCommand(PlatformCommand command);
    }

    public class PlatformCommand
    {
        private string _xplatCommand;
        private Dictionary<OSPlatform, string> _commandByPlatform = new Dictionary<OSPlatform, string>();
        
        public PlatformCommand()
        {
        }

        public PlatformCommand(string xplatCommand)
        {
            _xplatCommand = xplatCommand;
        }

        public void Add(OSPlatform platform, string command)
        {
            _commandByPlatform[platform] = command;
        }

        public string this[OSPlatform platform]
        {
            get
            {
                if (_commandByPlatform.TryGetValue(platform, out var specialCommand))
                {
                    return specialCommand;
                }

                return _xplatCommand;
            }
            set
            {
                _commandByPlatform[platform] = value;
            }
        }

        public override string ToString()
        {
            return ToString(CurrentPlatform);
        }

        public string ToString(OSPlatform? platform)
        {
            if (CurrentPlatform.HasValue && _commandByPlatform.TryGetValue(CurrentPlatform.Value, out var platformSpecific))
            {
                return platformSpecific;
            }

            return _xplatCommand;
        }

        public static readonly OSPlatform? CurrentPlatform;

        static PlatformCommand()
        {
            CurrentPlatform = GetCurrentOSPlatform();
        }
        
        private static OSPlatform? GetCurrentOSPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }
            return null;
        }
        
        public static implicit operator PlatformCommand(string xplatCommand)
        {
            return new PlatformCommand(xplatCommand);
        }
    }
}

using System;
using Buildalyzer;
using MetaPrograms;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Build
{
    public class TechnologyAdapterContext : ITechnologyAdapterContext
    {
        public TechnologyAdapterContext(
            IReadOnlyPreprocessorOutput preprocessor, 
            IMetadataObject input, 
            TechnologyAdapterMetadata adapter,
            ICodeGeneratorOutput output)
        {
            Preprocessor = preprocessor;
            Input = input;
            Adapter = adapter;
            Output = output;
        }

        public void AddMessage(string category, string message)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyPreprocessorOutput Preprocessor { get; }
        public IMetadataObject Input { get; }
        public TechnologyAdapterMetadata Adapter { get; } 
        public ICodeGeneratorOutput Output { get; }
        public IDeploymentScriptBuilder DeploymentScript => Input.Header.DeploymentScript;
    }
}

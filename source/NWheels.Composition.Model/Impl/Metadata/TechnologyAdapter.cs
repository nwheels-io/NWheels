using MetaPrograms;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public abstract class TechnologyAdapter<TMetadata> : ITechnologyAdapter
        where TMetadata : IMetadataObject
    {
        void ITechnologyAdapter.GenerateOutputs(ITechnologyAdapterContext context)
        {
            this.GenerateOutputs(new TechnologyAdapterContext<TMetadata>(context));
        }

        protected abstract void GenerateOutputs(TechnologyAdapterContext<TMetadata> context);
    }

    public class TechnologyAdapterContext<TMetadata> : ITechnologyAdapterContext
        where TMetadata : IMetadataObject
    {
        private readonly ITechnologyAdapterContext _inner;
        private readonly TMetadata _input;

        public TechnologyAdapterContext(ITechnologyAdapterContext inner)
        {
            _inner = inner;
            _input = (TMetadata) inner.Input;
        }

        public void AddMessage(string category, string message)
        {
            _inner.AddMessage(category, message);
        }

        IMetadataObject ITechnologyAdapterContext.Input => _input;
        public IReadOnlyPreprocessorOutput Preprocessor => _inner.Preprocessor;
        public TechnologyAdapterMetadata Adapter => _inner.Adapter;
        public TMetadata Input => _input;
        public ICodeGeneratorOutput Output => _inner.Output;
        public IDeploymentScriptBuilder DeploymentScript => _inner.DeploymentScript;
    }
}

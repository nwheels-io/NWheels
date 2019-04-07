using System.Collections.Generic;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Build
{
    public class ModelParserContext : IModelParserContext
    {
        private readonly ImperativeCodeModel _code;
        private readonly RoslynCodeModelReader _codeReader;
        private readonly IReadOnlyPreprocessorOutput _preprocessor;
        private readonly List<MetadataObject> _output;
        private readonly PreprocessedType _input;

        public ModelParserContext(
            ImperativeCodeModel code,
            RoslynCodeModelReader codeReader,
            IReadOnlyPreprocessorOutput preprocessor,
            List<MetadataObject> output = null,
            PreprocessedType input = null)
        {
            _preprocessor = preprocessor;
            _input = input;
            _output = output ?? new List<MetadataObject>();
            _code = code;
            _codeReader = codeReader;
        }

        IReadOnlyPreprocessorOutput IModelParserContext.Preprocessor => _preprocessor;

        PreprocessedType IModelParserContext.Input => _input;

        IList<MetadataObject> IModelParserContext.Output => _output;

        ImperativeCodeModel IModelParserContext.Code => _code;

        RoslynCodeModelReader IModelParserContext.CodeReader => _codeReader;

        public ModelParserContext WithInput(PreprocessedType input)
        {
            return new ModelParserContext(_code, _codeReader, _preprocessor, _output, input);            
        }
    }
}